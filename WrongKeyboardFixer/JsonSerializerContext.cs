using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WrongKeyboardFixer;

/// <summary>
/// Source-generated JSON serialization context for Native AOT compatibility.
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(WrongKeyboardFixer.Core.Model.AppSettings))]
[JsonSerializable(typeof(Dictionary<char, char>))]
internal partial class AppSettingsJsonContext : JsonSerializerContext
{
}