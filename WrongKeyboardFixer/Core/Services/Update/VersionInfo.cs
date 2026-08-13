using System;
using System.Diagnostics;
using System.Reflection;

namespace WrongKeyboardFixer.Core.Services.Update;

/// <summary>
/// Reads the running assembly's version and parses release tag names.
/// The current version is resolved once via reflection and cached afterwards.
/// </summary>
internal static class VersionInfo
{
    private static Version? _currentVersion;

    public static Version Current => _currentVersion ??= ReadCurrentVersion();

    public static string CurrentString => Current.ToString();

    /// <summary>
    /// Parses a GitHub tag name (e.g. "v1.3.0") into a <see cref="Version"/>.
    /// Falls back to 1.0.0 when the tag is not a valid version.
    /// </summary>
    public static Version ParseTag(string tagName)
    {
        string versionString = tagName.TrimStart('v');
        if (Version.TryParse(versionString, out var version))
            return version;
        return new Version(1, 0, 0);
    }

    /// <summary>
    /// Reads the file version of an executable, falling back to its product version.
    /// </summary>
    public static Version? FromFile(string filePath)
    {
        try
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(filePath);
            if (versionInfo.FileVersion != null && Version.TryParse(versionInfo.FileVersion, out var version))
                return version;
            if (versionInfo.ProductVersion != null && Version.TryParse(versionInfo.ProductVersion, out var productVersion))
                return productVersion;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Failed to read file version: {ex.Message}");
        }
        return null;
    }

    private static Version ReadCurrentVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (versionAttribute != null && Version.TryParse(versionAttribute.InformationalVersion, out var version))
            return version;
        return assembly.GetName().Version ?? new Version(1, 0, 0);
    }
}
