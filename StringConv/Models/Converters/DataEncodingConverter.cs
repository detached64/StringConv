using StringConv.I18n;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace StringConv.Models.Converters;

internal abstract class DataEncodingConverter : StringConverter
{
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

    public override byte[] FromString(string input)
    {
        string[] parts = input.Split([@"\u"], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        byte[] bytes = new byte[parts.Length * 2];
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Length != 4 || !ushort.TryParse(parts[i], NumberStyles.HexNumber, null, out ushort codePoint))
            {
                throw new InvalidDataException();
            }
            bytes[i * 2] = (byte)(codePoint & 0xFF);
            bytes[i * 2 + 1] = (byte)(codePoint >> 8);
        }
        return bytes;
    }

    public override string ToString(byte[] input)
    {
        if (input.Length % 2 != 0)
        {
            throw new InvalidDataException();
        }
        StringBuilder sb = new();
        for (int i = 0; i < input.Length; i += 2)
        {
            sb.Append($@"\u{(ushort)(input[i] | (input[i + 1] << 8)):X4}");
        }
        return sb.ToString();
    }
}
