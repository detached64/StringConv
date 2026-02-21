using System.Collections.Generic;

namespace StringConv.Models.Settings;

internal sealed class AppSettings
{
    public HashSet<string> PinnedConverterIds { get; set; } = [];
}
