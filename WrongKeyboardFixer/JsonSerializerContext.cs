using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Model;

namespace WrongKeyboardFixer;

/// <summary>
/// Source-generated JSON serialization context for Native AOT compatibility.
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(AppSettings))]
[JsonSerializable(typeof(Keys))] // ← بسیار مهم برای AOT
[JsonSerializable(typeof(HotkeyModifiers))]
[JsonSerializable(typeof(Dictionary<char, char>))]
internal partial class AppSettingsJsonContext : JsonSerializerContext
{
}
