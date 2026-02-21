using System.Text.Json.Serialization;

namespace StringConv.Models.Converters;

[JsonSerializable(typeof(string))]
internal partial class ConverterSerializerContext : JsonSerializerContext;
