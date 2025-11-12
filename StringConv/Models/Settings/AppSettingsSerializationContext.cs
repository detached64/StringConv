using System.Text.Json.Serialization;

namespace StringConv.Models.Settings;

[JsonSerializable(typeof(AppSettings))]
internal partial class AppSettingsSerializationContext : JsonSerializerContext;
