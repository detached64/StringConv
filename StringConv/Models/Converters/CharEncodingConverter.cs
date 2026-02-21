using System.Text;
using StringConv.I18n;

namespace StringConv.Models.Converters;

internal sealed class CharEncodingConverter(Encoding encoding) : StringConverter
{
    public override string Name => encoding.EncodingName;
    public override string Category => GuiStrings.CharacterEncoding;
    public override string Id => $"{Category}/{encoding.CodePage}";
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
