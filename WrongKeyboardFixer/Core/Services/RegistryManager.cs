using System;
using System.Windows.Forms;
using Microsoft.Win32;
using WrongKeyboardFixer.Core.Helpers;

namespace WrongKeyboardFixer.Core.Services;

/// <summary>
/// Thin wrapper around the Windows registry <c>Run</c> key that controls
/// whether the application starts when Windows boots.
/// </summary>
internal sealed class RegistryManager
{
    private const string AutoStartKeyName = "WrongKeyboardFixer";
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    private static readonly string ExecutablePath = Application.ExecutablePath;

    public void SetAutoStart(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
            if (key == null)
                return;

            if (enable)
                key.SetValue(AutoStartKeyName, $"\"{ExecutablePath}\"");
            else if (key.GetValue(AutoStartKeyName) is not null)
                key.DeleteValue(AutoStartKeyName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                Localization.Format("StartupSettingsError", ex.Message),
                Localization.Get("Error"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}
