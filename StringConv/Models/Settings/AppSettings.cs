using System.Collections.Generic;

namespace StringConv.Models.Settings;

internal sealed class AppSettings
{
    public List<int> PinnedConverterIndices { get; set; } = [];
}
