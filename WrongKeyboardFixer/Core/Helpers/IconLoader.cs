using System;
using System.Drawing;
using System.Reflection;

namespace WrongKeyboardFixer.Core.Helpers;

/// <summary>
/// Loads the application icon directly from an embedded manifest resource.
/// This avoids using ResourceManager for custom types, which is not AOT-compatible.
/// </summary>
public static class IconLoader
{
    private const string ResourceName = "WrongKeyboardFixer.ico";
    private static Icon? _icon;

    public static Icon GetIcon()
    {
        if (_icon != null)
            return _icon;

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(ResourceName);
        _icon = stream != null ? new Icon(stream) : SystemIcons.Application;
        return _icon;
    }
}