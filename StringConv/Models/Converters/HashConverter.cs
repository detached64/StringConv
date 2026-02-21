using StringConv.I18n;
using System;
using System.IO.Hashing;
using System.Security.Cryptography;

namespace StringConv.Models.Converters;

internal abstract class HashConverter : StringConverter
{
    public override string Category => GuiStrings.Hash;
}

internal sealed class CRC32Converter : HashConverter
{
    public override string Name => "CRC32";

    public override string ToString(byte[] data)
    {
        return Convert.ToHexString(Crc32.Hash(data));
    }
}

internal sealed class CRC64Converter : HashConverter
{
    public override string Name => "CRC64";

    public override string ToString(byte[] data)
    {
        return Convert.ToHexString(Crc64.Hash(data));
    }
}

internal sealed class MD5Converter : HashConverter
{
    public override string Name => "MD5";

    public override string ToString(byte[] data)
    {
        return Convert.ToHexString(MD5.HashData(data));
    }
}

internal sealed class SHA1Converter : HashConverter
{
    public override string Name => "SHA1";

    public override string ToString(byte[] data)
    {
        return Convert.ToHexString(SHA1.HashData(data));
    }
}

internal sealed class SHA256Converter : HashConverter
{
    public override string Name => "SHA256";

    public override string ToString(byte[] data)
    {
        return Convert.ToHexString(SHA256.HashData(data));
    }
}

internal sealed class SHA384Converter : HashConverter
{
    public override string Name => "SHA384";

    public override string ToString(byte[] data)
    {
        return Convert.ToHexString(SHA384.HashData(data));
    }
}

internal sealed class SHA512Converter : HashConverter
{
    public override string Name => "SHA512";

    public override string ToString(byte[] data)
    {
        return Convert.ToHexString(SHA512.HashData(data));
    }
}
