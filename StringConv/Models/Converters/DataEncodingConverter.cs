using StringConv.I18n;
using System;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

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
        if (string.IsNullOrEmpty(input))
            return input;

        if (input.Length <= 512)
        {
            Span<char> buffer = stackalloc char[input.Length];
            int index = 0;
            foreach (char c in input)
            {
                if (!char.IsWhiteSpace(c))
                    buffer[index++] = c;
            }
            return index == input.Length ? input : new string(buffer[..index]);
        }

        int nonWhitespaceCount = 0;
        foreach (char c in input)
        {
            if (!char.IsWhiteSpace(c))
                nonWhitespaceCount++;
        }

        if (nonWhitespaceCount == input.Length)
            return input;
        if (nonWhitespaceCount == 0)
            return string.Empty;

        return string.Create(nonWhitespaceCount, input, (span, str) =>
        {
            int index = 0;
            foreach (char c in str)
            {
                if (!char.IsWhiteSpace(c))
                    span[index++] = c;
            }
        });
    }
}

internal sealed class UnicodeEscapeSequenceConverter : DataEncodingConverter
{
    public override string Name => GuiStrings.UnicodeEscapeSequence;
    public override string[] Dependencies => [$"{GuiStrings.CharacterEncoding}/1200"];   // UTF-16LE

    public override byte[] FromString(string input)
    {
        string result = Regex.Unescape(input);
        return Encoding.Unicode.GetBytes(result);
    }

    public override string ToString(byte[] input)
    {
        string result = Encoding.Unicode.GetString(input);
        return JsonSerializer.Serialize(result, ConverterSerializerContext.Default.String).Trim('"');
    }
}

internal sealed class UrlEncodingConverter : DataEncodingConverter
{
    public override string Name => GuiStrings.UrlEncoding;
    public override string[] Dependencies => [$"{GuiStrings.CharacterEncoding}/65001"];   // UTF-8

    public override byte[] FromString(string input)
    {
        string result = Uri.UnescapeDataString(input);
        return Encoding.UTF8.GetBytes(result);
    }

    public override string ToString(byte[] input)
    {
        string result = Encoding.UTF8.GetString(input);
        return Uri.EscapeDataString(result);
    }
}
