using System;
using System.Collections.Generic;

namespace StringConv.Models.Converters;

internal static partial class StringConverterProvider
{
    private static readonly Lazy<List<StringConverter>> _converters = new(InitConverters);
    private static List<StringConverter> InitConverters() => [.. LoadConvertersGenerated()];
    public static List<StringConverter> Converters => _converters.Value;
}
