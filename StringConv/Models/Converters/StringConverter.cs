using System;

namespace StringConv.Models.Converters;

internal abstract class StringConverter
{
    public abstract string Name { get; }
    public abstract string Category { get; }
    public virtual string Id => $"{Category}/{Name}";
    public virtual bool CanConvert { get; }

    public virtual byte[] FromString(string value)
    {
        throw new NotImplementedException();
    }

    public virtual string ToString(byte[] data)
    {
        throw new NotImplementedException();
    }
}
