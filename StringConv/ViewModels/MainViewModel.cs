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
        foreach (int index in _settingsService.Settings.PinnedConverterIndices)
        {
            if (index >= 0 && index < Converters.Count)
            {
                StringConverter converter = Converters[index];
                if (converter?.CanConvert == true)
                {
                    ConverterViewModel cvm = new(converter);
                    cvm.TextChanged += OnConverterTextChanged;
                    PinnedConverterViewModels.Add(cvm);
                }
            }
        }
    }

    private void ConfigureCategories()
    {
        ConverterCategories.Add(new ConverterCategoryViewModel()
        {
            Name = GuiStrings.CharacterEncoding,
            Converters = Converters.Where(vm => vm is CharEncodingConverter)
        });
        ConverterCategories.Add(new ConverterCategoryViewModel()
        {
            Name = GuiStrings.DataEncoding,
            Converters = Converters.Where(vm => vm is DataEncodingConverter)
        });
        ConverterCategories.Add(new ConverterCategoryViewModel()
        {
            Name = GuiStrings.CodeSnippets,
            Converters = Converters.Where(vm => vm is CodeSnippetConverter)
        });
        ConverterCategories.Add(new ConverterCategoryViewModel()
        {
            Name = GuiStrings.Hash,
            Converters = Converters.Where(vm => vm is HashConverter)
        });
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
                span[spanIndex++] = GetHexChar(b >> 4);
                span[spanIndex++] = GetHexChar(b & 0xF);
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
        int index = Converters.IndexOf(SelectedConverter);
        if (index >= 0 && SelectedConverter.CanConvert && !_settingsService.Settings.PinnedConverterIndices.Contains(index))
        {
            _settingsService.Settings.PinnedConverterIndices.Add(index);
            ConverterViewModel cvm = new(SelectedConverter);
            cvm.TextChanged += OnConverterTextChanged;
            PinnedConverterViewModels.Add(cvm);
            WeakReferenceMessenger.Default.Send(new StatusMessage(string.Format(MsgStrings.ConverterPinned, SelectedConverter.Name)));
            cvm.UpdateToString(InputData);
        }
    }

    [RelayCommand(CanExecute = nameof(CanPinMultiple))]
    private void PinMultiple()
    {
        List<string> pinnedNames = [];
        foreach (StringConverter converter in SelectedConverters.OfType<StringConverter>())
        {
            int index = Converters.IndexOf(converter);
            if (index >= 0 && converter.CanConvert && !_settingsService.Settings.PinnedConverterIndices.Contains(index))
            {
                _settingsService.Settings.PinnedConverterIndices.Add(index);
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
        int index = Converters.IndexOf(cvm.Converter);
        if (index >= 0 && _settingsService.Settings.PinnedConverterIndices.Contains(index))
        {
            _settingsService.Settings.PinnedConverterIndices.Remove(index);
            cvm.TextChanged -= OnConverterTextChanged;
            PinnedConverterViewModels.Remove(cvm);
            WeakReferenceMessenger.Default.Send(new StatusMessage(string.Format(MsgStrings.ConverterRemoved, cvm.Converter.Name)));
        }
    }

    [RelayCommand]
    private void Clear()
    {
        PinnedConverterViewModels.Clear();
        _settingsService.Settings.PinnedConverterIndices.Clear();
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static char GetHexChar(int value) => (char)(value < 10 ? '0' + value : 'A' + value - 10);

    private bool CanPin => SelectedConverter?.CanConvert == true &&
        !_settingsService.Settings.PinnedConverterIndices.Contains(Converters.IndexOf(SelectedConverter));

    private bool CanPinMultiple => SelectedConverters.OfType<StringConverter>()?.Any(c => c.CanConvert &&
        !_settingsService.Settings.PinnedConverterIndices.Contains(Converters.IndexOf(c))) == true;

    partial void OnSelectedConverterChanged(StringConverter value)
    {
        UpdateCopyText();
    }
}
