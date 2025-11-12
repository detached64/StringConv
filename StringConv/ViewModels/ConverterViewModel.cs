using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using StringConv.Models.Converters;
using StringConv.Models.Messages;
using System;

namespace StringConv.ViewModels;

internal partial class ConverterViewModel(StringConverter converter) : ViewModelBase
{
    private bool isUpdating;

    [ObservableProperty]
    private string name = converter.Name;
    [ObservableProperty]
    private string text;

    public StringConverter Converter => converter;
    public event EventHandler<byte[]> TextChanged;

    partial void OnTextChanged(string value)
    {
        if (isUpdating)
            return;
        try
        {
            if (!string.IsNullOrEmpty(text))
            {
                byte[] data = converter.FromString(text);
                TextChanged?.Invoke(this, data);
                WeakReferenceMessenger.Default.Send(new StatusMessage($"Conversion successful."));
            }
            else
            {
                TextChanged?.Invoke(this, Array.Empty<byte>());
                WeakReferenceMessenger.Default.Send(new StatusMessage($"Input is empty.", Brushes.Orange));
            }
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new StatusMessage(ex.Message, Brushes.Red));
        }
    }

    public void UpdateToString(byte[] data)
    {
        isUpdating = true;
        try
        {
            Text = converter.ToString(data);
        }
        catch
        {
            Text = string.Empty;
        }
        finally
        {
            isUpdating = false;
        }
    }
}
