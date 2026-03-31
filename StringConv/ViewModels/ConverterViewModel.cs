using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using StringConv.I18n;
using StringConv.Models.Converters;
using StringConv.Models.Messages;
using System;

namespace StringConv.ViewModels;

internal partial class ConverterViewModel(StringConverter converter) : ViewModelBase
{
    private bool isUpdating;

    [ObservableProperty]
    public partial string Name { get; set; } = converter.Name;

    [ObservableProperty]
    public partial string Text { get; set; }

    public StringConverter Converter => converter;
    public event EventHandler<byte[]> TextChanged;

    partial void OnTextChanged(string value)
    {
        if (isUpdating)
            return;
        try
        {
            if (!string.IsNullOrEmpty(Text))
            {
                byte[] data = converter.FromString(Text);
                TextChanged?.Invoke(this, data);
                WeakReferenceMessenger.Default.Send(new StatusMessage(MsgStrings.ConversionSuccessful));
            }
            else
            {
                TextChanged?.Invoke(this, []);
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

    public void Clear()
    {
        Text = string.Empty;
    }
}
