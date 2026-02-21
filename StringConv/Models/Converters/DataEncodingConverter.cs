using StringConv.I18n;
using System;
using System.Text;
using System.Text.Json;

namespace StringConv.Models.Converters;

internal abstract class DataEncodingConverter : StringConverter
{
    public override string Category => GuiStrings.DataEncoding;
    public override bool CanConvert => true;
}

internal sealed class Base64Converter : DataEncodingConverter
{
    public override string Name => "Base64";

    public override byte[] FromString(string input)
    {
        return Convert.FromBase64String(input);
    }

    public override string ToString(byte[] input)
    {
        return Convert.ToBase64String(input);
    }
}

internal sealed class HexStringConverter : DataEncodingConverter
{
    public override string Name => GuiStrings.HexString;

    public override byte[] FromString(string value)
    {
        value = RemoveWhitespaces(value);
        return value.Length % 2 != 0
            ? throw new FormatException(MsgStrings.HexStringMustHaveEvenLength)
            : Convert.FromHexString(value);
    }

    public override string ToString(byte[] data)
    {
        return Convert.ToHexString(data);
    }

    private static string RemoveWhitespaces(string input)
    {
        StringBuilder sb = new();
        foreach (char c in input)
        {
            if (!char.IsWhiteSpace(c))
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
}

internal sealed class UnicodeStringConverter : DataEncodingConverter
{
    public override string Name => GuiStrings.UnicodeString;
    public override string[] Dependencies => [$"{GuiStrings.CharacterEncoding}/1200"];   // UTF-16LE

    public override byte[] FromString(string input)
    {
        string result = JsonSerializer.Deserialize<string>($"\"{input}\"");
        return Encoding.Unicode.GetBytes(result);
    }

    public override string ToString(byte[] input)
    {
        string result = Encoding.Unicode.GetString(input);
        return JsonSerializer.Serialize(result).Trim('"');
    }
}
