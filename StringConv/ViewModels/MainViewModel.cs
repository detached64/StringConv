using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using StringConv.I18n;
using StringConv.Models.Converters;
using StringConv.Models.Messages;
using StringConv.Services;
using StringConv.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace StringConv.ViewModels;

internal partial class MainViewModel : ViewModelBase
{
    private readonly IShowDialogService _showDialogService;
    private readonly ISettingsService _settingsService;
    private byte[] InputData;
    [ObservableProperty]
    private string hexText;
    [ObservableProperty]
    private string copyText;
    [ObservableProperty]
    private StatusMessage message;
    [ObservableProperty]
    private List<StringConverter> converters = [.. StringConverterProvider.Converters.OrderBy(c => c.Name)];
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PinCommand))]
    private StringConverter selectedConverter;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PinMultipleCommand))]
    private ObservableCollection<object> selectedConverters = [];
    [ObservableProperty]
    private ObservableCollection<ConverterViewModel> pinnedConverterViewModels = [];
    [ObservableProperty]
    private ObservableCollection<ConverterCategoryViewModel> converterCategories = [];

    public MainViewModel(IShowDialogService showDialogService, ISettingsService settingsService)
    {
        if (Design.IsDesignMode)
            return;

        _showDialogService = showDialogService;
        _settingsService = settingsService;
        WeakReferenceMessenger.Default.Register<StatusMessage>(this, (_, m) => Message = m);

        ConfigurePinnedConverters();
        ConfigureCategories();
    }

    public MainViewModel() : this(null, null) { }

    private void ConfigurePinnedConverters()
    {
        PinnedConverterViewModels.Clear();
        foreach (string id in _settingsService.Settings.PinnedConverterIds)
        {
            StringConverter converter = StringConverterProvider.Find(id);
            if (converter?.CanConvert == true)
            {
                ConverterViewModel cvm = new(converter);
                cvm.TextChanged += OnConverterTextChanged;
                PinnedConverterViewModels.Add(cvm);
            }
        }
    }

    private void ConfigureCategories()
    {
        foreach (IGrouping<string, StringConverter> group in Converters.GroupBy(c => c.Category))
        {
            ConverterCategories.Add(new ConverterCategoryViewModel()
            {
                Name = group.Key,
                Converters = group
            });
        }
    }

    private void OnConverterTextChanged(object sender, byte[] data)
    {
        if (data == null)
            return;
        InputData = data;
        foreach (ConverterViewModel cvm in PinnedConverterViewModels)
        {
            if (cvm != sender)
            {
                cvm.UpdateToString(data);
            }
        }
        UpdateHexText();
        UpdateCopyText();
    }

    private void UpdateHexText()
    {
        if (InputData == null || InputData.Length == 0)
        {
            HexText = string.Empty;
            return;
        }

        HexText = string.Create((InputData.Length * 3) - 1, InputData, (span, data) =>
        {
            int spanIndex = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (i > 0)
                {
                    span[spanIndex++] = ' ';
                }

                byte b = data[i];
                int hi = b >> 4, lo = b & 0xF;
                span[spanIndex++] = (char)(hi < 10 ? '0' + hi : 'A' + hi - 10);
                span[spanIndex++] = (char)(lo < 10 ? '0' + lo : 'A' + lo - 10);
            }
        });
    }

    private void UpdateCopyText()
    {
        if (SelectedConverter is null || InputData == null || InputData.Length == 0)
        {
            CopyText = string.Empty;
            return;
        }
        try
        {
            CopyText = SelectedConverter.ToString(InputData);
        }
        catch (Exception ex)
        {
            CopyText = string.Empty;
            WeakReferenceMessenger.Default.Send(new StatusMessage(string.Format(MsgStrings.ConversionError, ex.Message), Brushes.Red));
        }
    }

    [RelayCommand(CanExecute = nameof(CanPin))]
    private void Pin()
    {
        List<string> pinnedNames = [];
        if (SelectedConverter.CanConvert && !_settingsService.Settings.PinnedConverterIds.Contains(SelectedConverter.Id))
        {
            _settingsService.Settings.PinnedConverterIds.Add(SelectedConverter.Id);
            ConverterViewModel cvm = new(SelectedConverter);
            cvm.TextChanged += OnConverterTextChanged;
            PinnedConverterViewModels.Add(cvm);
            pinnedNames.Add(SelectedConverter.Name);
            cvm.UpdateToString(InputData);
        }
        if (SelectedConverter.Dependencies.Length > 0)
        {
            foreach (string dependencyId in SelectedConverter.Dependencies)
            {
                StringConverter dependency = StringConverterProvider.Find(dependencyId);
                if (dependency.CanConvert && !_settingsService.Settings.PinnedConverterIds.Contains(dependency.Id))
                {
                    _settingsService.Settings.PinnedConverterIds.Add(dependency.Id);
                    ConverterViewModel cvm = new(dependency);
                    cvm.TextChanged += OnConverterTextChanged;
                    PinnedConverterViewModels.Add(cvm);
                    pinnedNames.Add(dependency.Name);
                    cvm.UpdateToString(InputData);
                }
            }
        }
        switch (pinnedNames.Count)
        {
            case 0:
                break;
            case 1:
                WeakReferenceMessenger.Default.Send(new StatusMessage(string.Format(MsgStrings.ConverterPinned, pinnedNames[0])));
                break;
            default:
                WeakReferenceMessenger.Default.Send(new StatusMessage(string.Format(MsgStrings.ConvertersPinned, pinnedNames.Count, string.Join(", ", pinnedNames))));
                break;
        }
    }

    [RelayCommand(CanExecute = nameof(CanPinMultiple))]
    private void PinMultiple()
    {
        List<string> pinnedNames = [];
        foreach (StringConverter converter in SelectedConverters.OfType<StringConverter>())
        {
            if (converter.CanConvert && !_settingsService.Settings.PinnedConverterIds.Contains(converter.Id))
            {
                _settingsService.Settings.PinnedConverterIds.Add(converter.Id);
                ConverterViewModel cvm = new(converter);
                cvm.TextChanged += OnConverterTextChanged;
                PinnedConverterViewModels.Add(cvm);
                pinnedNames.Add(converter.Name);
                cvm.UpdateToString(InputData);
            }
        }
        if (pinnedNames.Count > 0)
        {
            WeakReferenceMessenger.Default.Send(new StatusMessage(string.Format(MsgStrings.ConvertersPinned, pinnedNames.Count, string.Join(", ", pinnedNames))));
        }
    }

    [RelayCommand]
    private void Remove(ConverterViewModel cvm)
    {
        if (_settingsService.Settings.PinnedConverterIds.Remove(cvm.Converter.Id))
        {
            cvm.TextChanged -= OnConverterTextChanged;
            PinnedConverterViewModels.Remove(cvm);
            WeakReferenceMessenger.Default.Send(new StatusMessage(string.Format(MsgStrings.ConverterRemoved, cvm.Converter.Name)));
        }
    }

    [RelayCommand]
    private void Clear()
    {
        PinnedConverterViewModels.Clear();
        _settingsService.Settings.PinnedConverterIds.Clear();
        WeakReferenceMessenger.Default.Send(new StatusMessage(string.Format(MsgStrings.ConverterCleared)));
    }

    [RelayCommand]
    private async Task CopyAsync(Window window)
    {
        if (string.IsNullOrEmpty(CopyText))
        {
            return;
        }
        try
        {
            await window.Clipboard.SetTextAsync(CopyText);
            WeakReferenceMessenger.Default.Send(new StatusMessage(MsgStrings.TextCopied));
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new StatusMessage(string.Format(MsgStrings.CopyError, ex.Message), Brushes.Red));
        }
    }

    [RelayCommand]
    private async Task ShowAboutAsync(Window window)
    {
        await _showDialogService.ShowDialogAsync<AboutView, AboutViewModel>(window);
    }

    private bool CanPin => SelectedConverter?.CanConvert == true &&
        !_settingsService.Settings.PinnedConverterIds.Contains(SelectedConverter.Id);

    private bool CanPinMultiple => SelectedConverters.OfType<StringConverter>()
        .Any(c => c.CanConvert && !_settingsService.Settings.PinnedConverterIds.Contains(c.Id)) == true;

    partial void OnSelectedConverterChanged(StringConverter value)
    {
        UpdateCopyText();
    }
}
