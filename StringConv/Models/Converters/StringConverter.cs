using System;
using System.Linq;

namespace StringConv.Models.Converters;

internal abstract class StringConverter
{
    /// <summary>
    /// Name of the converter.
    /// </summary>
    public abstract string Name { get; }
    /// <summary>
    /// Category of the converter.
    /// </summary>
    public abstract string Category { get; }
    /// <summary>
    /// Unique identifier for the converter.
    /// </summary>
    public virtual string Id => $"{Category}/{Name}";
    /// <summary>
    /// Whether the converter can convert from string to byte array.
    /// </summary>
    public virtual bool CanConvert { get; }
    /// <summary>
    /// List of converter Ids that the converter depends on.
    /// </summary>
    /// <remarks>All dependencies will be pinned when the converter is pinned.</remarks>
    public virtual string[] Dependencies => [];
    public virtual string Description => $@"Name: {Name}
Category: {Category}
Id: {Id}
Can Convert: {CanConvert}
Dependencies: {(Dependencies.Length > 0 ? string.Join(", ", Dependencies) : "None")}";

    public virtual byte[] FromString(string value)
    {
        throw new NotImplementedException();
    }

    public virtual string ToString(byte[] data)
    {
        throw new NotImplementedException();
    }
}
