using System.Text;

namespace StringConv.Models.Converters;

internal sealed class CharEncodingConverter(Encoding encoding) : StringConverter
{
    public override string Name => encoding.EncodingName;
    public override bool CanConvert => true;

    public override byte[] FromString(string value)
    {
        return encoding.GetBytes(value);
    }

    public override string ToString(byte[] data)
    {
        return encoding.GetString(data);
    }
}
