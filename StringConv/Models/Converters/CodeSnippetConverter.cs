using StringConv.I18n;
using System.Text;

namespace StringConv.Models.Converters;

internal abstract class CodeSnippetConverter : StringConverter
{
    public override string Category => GuiStrings.CodeSnippets;
}

internal sealed class AssemblyByteConverter : CodeSnippetConverter
{
    public override string Name => GuiStrings.AssemblyByteArray;

    public override string ToString(byte[] data)
    {
        StringBuilder sb = new(data.Length * 5);
        sb.Append("array DB ");
        for (int i = 0; i < data.Length; i++)
        {
            sb.Append($"{data[i]:X2}h");
            if (i < data.Length - 1)
            {
                sb.Append(", ");
            }
        }
        return sb.ToString();
    }
}

internal sealed class AssemblyStringConverter : CodeSnippetConverter
{
    public override string Name => GuiStrings.AssemblyString;

    public override string ToString(byte[] data)
    {
        StringBuilder sb = new(data.Length * 4);
        for (int i = 0; i < data.Length; i++)
        {
            sb.Append($"{data[i]:X2}h");
            if (i < data.Length - 1)
            {
                sb.Append(',');
            }
        }
        return sb.ToString();
    }
}

internal sealed class CByteConverter : CodeSnippetConverter
{
    public override string Name => GuiStrings.CByteArray;

    public override string ToString(byte[] data)
    {
        StringBuilder sb = new(data.Length * 6);
        sb.Append("{ ");
        for (int i = 0; i < data.Length; i++)
        {
            sb.Append($"0x{data[i]:X2}");
            if (i < data.Length - 1)
            {
                sb.Append(", ");
            }
        }
        sb.Append(" }");
        return sb.ToString();
    }
}

internal sealed class CStringConverter : CodeSnippetConverter
{
    public override string Name => GuiStrings.CString;

    public override string ToString(byte[] data)
    {
        StringBuilder sb = new(data.Length * 4);
        sb.Append('\"');
        for (int i = 0; i < data.Length; i++)
        {
            sb.Append($"\\x{data[i]:X2}");
        }
        sb.Append('\"');
        return sb.ToString();
    }
}

internal sealed class PascalByteConverter : CodeSnippetConverter
{
    public override string Name => GuiStrings.PascalByteArray;

    public override string ToString(byte[] data)
    {
        StringBuilder sb = new(data.Length * 5);
        sb.Append($"Array [1..{data.Length}] of Byte = ( ");
        for (int i = 0; i < data.Length; i++)
        {
            sb.Append($"${data[i]:X2}");
            if (i < data.Length - 1)
            {
                sb.Append(", ");
            }
        }
        sb.Append(" );");
        return sb.ToString();
    }
}

internal sealed class Python3ByteConverter : CodeSnippetConverter
{
    public override string Name => GuiStrings.Python3ByteString;

    public override string ToString(byte[] data)
    {
        StringBuilder sb = new(data.Length * 4);
        sb.Append("b\"");
        for (int i = 0; i < data.Length; i++)
        {
            sb.Append($"\\x{data[i]:X2}");
        }
        sb.Append('"');
        return sb.ToString();
    }
}
