using StringConv.Models.Converters;
using System.Collections.Generic;

namespace StringConv.ViewModels;

internal sealed class ConverterCategoryViewModel
{
    public string Name { get; init; }
    public IEnumerable<StringConverter> Converters { get; init; }
}
