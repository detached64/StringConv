
using Avalonia.Media;

namespace StringConv.Models.Messages;

internal sealed class StatusMessage(string text = null, IBrush color = null)
{
    public string Text { get; } = text;
    public IBrush Color { get; } = color ?? Brushes.Gray;
}
