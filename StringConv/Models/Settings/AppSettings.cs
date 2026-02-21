using System.Collections.Generic;

namespace StringConv.Models.Settings;

internal sealed class AppSettings
{
    public List<string> PinnedConverterIds { get; set; } = [];
}
