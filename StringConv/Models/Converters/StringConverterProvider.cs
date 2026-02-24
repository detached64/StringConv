using System;
using System.Collections.Generic;
using System.Linq;

namespace StringConv.Models.Converters;

internal static partial class StringConverterProvider
{
    private static readonly Lazy<List<StringConverter>> _converters = new(InitConverters);
    private static readonly Lazy<Dictionary<string, StringConverter>> _converterMap = new(() => _converters.Value.ToDictionary(c => c.Id));
    private static List<StringConverter> InitConverters() => [.. LoadConvertersGenerated()];
    public static List<StringConverter> Converters => _converters.Value;

    public static StringConverter Find(string id)
    {
        return _converterMap.Value.GetValueOrDefault(id);
    }
}
