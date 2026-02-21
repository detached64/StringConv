using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StringConv.Models.Settings;

[JsonSerializable(typeof(AppSettings))]
[JsonSerializable(typeof(HashSet<string>))]
internal partial class AppSettingsSerializationContext : JsonSerializerContext;
