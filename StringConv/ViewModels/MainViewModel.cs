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
    private List<StringConverter> converters = StringConverterProvider.Converters;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    private StringConverter selectedConverter;
    [ObservableProperty]
    private ObservableCollection<ConverterViewModel> converterViewModels = [];
    [ObservableProperty]
    private ObservableCollection<ConverterCategoryViewModel> converterCategories = [];

    public MainViewModel(IShowDialogService showDialogService, ISettingsService settingsService)
    {
        if (Design.IsDesignMode)
            return;

        _showDialogService = showDialogService;
        _settingsService = settingsService;
        WeakReferenceMessenger.Default.Register<StatusMessage>(this, (_, m) => Message = m);

        ConfigureFixedConverters();
        ConfigureCategories();
    }

    public MainViewModel() : this(null, null) { }

    private void ConfigureFixedConverters()
    {
        ConverterViewModels.Clear();
        foreach (int index in _settingsService.Settings.FixedConverterIndices)
        {
            if (index >= 0 && index < Converters.Count)
            {
                StringConverter converter = Converters[index];
                if (converter?.CanConvert == true)
                {
                    ConverterViewModel cvm = new(converter);
                    cvm.TextChanged += OnConverterTextChanged;
                    ConverterViewModels.Add(cvm);
                }
            }
        }
    }

    private void ConfigureCategories()
    {
        ConverterCategoryViewModel charEncodingGroup = new()
        {
            Name = GuiStrings.CharacterEncoding,
            Converters = Converters.Where(vm => vm is CharEncodingConverter),
        };
        ConverterCategoryViewModel dataEncodingGroup = new()
        {
            Name = GuiStrings.DataEncoding,
            Converters = Converters.Where(vm => vm is DataEncodingConverter),
        };
        ConverterCategoryViewModel codeSnippetGroup = new()
        {
            Name = GuiStrings.CodeSnippets,
            Converters = Converters.Where(vm => vm is CodeSnippetConverter),
        };
        ConverterCategoryViewModel hashGroup = new()
        {
            Name = GuiStrings.Hash,
            Converters = Converters.Where(vm => vm is HashConverter),
        };
        ConverterCategories.Add(charEncodingGroup);
        ConverterCategories.Add(dataEncodingGroup);
        ConverterCategories.Add(codeSnippetGroup);
        ConverterCategories.Add(hashGroup);
    }

    private void OnConverterTextChanged(object sender, byte[] data)
    {
        if (data == null)
            return;
        InputData = data;
        foreach (ConverterViewModel vm in ConverterViewModels)
        {
            if (vm != sender)
            {
                vm.UpdateToString(data);
            }
        }
        UpdateHexText();
        UpdateCopyText();
    }

    private void UpdateHexText()
    {
        if (InputData?.Length == 0)
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
        if (SelectedConverter == null || InputData?.Length == 0)
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
            WeakReferenceMessenger.Default.Send(new StatusMessage($"Conversion error: {ex.Message}", Brushes.Red));
        }
    }

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add()
    {
        int index = Converters.IndexOf(SelectedConverter);
        if (index >= 0 && !_settingsService.Settings.FixedConverterIndices.Contains(index))
        {
            _settingsService.Settings.FixedConverterIndices.Add(index);
            ConverterViewModel cvm = new(SelectedConverter);
            cvm.TextChanged += OnConverterTextChanged;
            ConverterViewModels.Add(cvm);
            WeakReferenceMessenger.Default.Send(new StatusMessage($"Added converter: {SelectedConverter.Name}"));
        }
    }

    [RelayCommand]
    private void Delete(ConverterViewModel cvm)
    {
        int index = Converters.IndexOf(cvm.Converter);
        if (index >= 0 && _settingsService.Settings.FixedConverterIndices.Contains(index))
        {
            _settingsService.Settings.FixedConverterIndices.Remove(index);
            cvm.TextChanged -= OnConverterTextChanged;
            ConverterViewModels.Remove(cvm);
            WeakReferenceMessenger.Default.Send(new StatusMessage($"Removed converter: {cvm.Converter.Name}"));
        }
    }

    [RelayCommand]
    private void Clear()
    {
        ConverterViewModels.Clear();
        _settingsService.Settings.FixedConverterIndices.Clear();
        WeakReferenceMessenger.Default.Send(new StatusMessage("Cleared all fixed converters."));
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
            WeakReferenceMessenger.Default.Send(new StatusMessage("Text copied to clipboard."));
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new StatusMessage($"Failed to copy text: {ex.Message}"));
        }
    }

    [RelayCommand]
    private async Task ShowAboutAsync(Window window)
    {
        await _showDialogService.ShowDialogAsync<AboutView, AboutViewModel>(window);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static char GetHexChar(int value) => (char)(value < 10 ? '0' + value : 'A' + value - 10);

    private bool CanAdd => SelectedConverter?.CanConvert == true &&
        !_settingsService.Settings.FixedConverterIndices.Contains(Converters.IndexOf(SelectedConverter));

    partial void OnSelectedConverterChanged(StringConverter value)
    {
        UpdateCopyText();
    }
}
