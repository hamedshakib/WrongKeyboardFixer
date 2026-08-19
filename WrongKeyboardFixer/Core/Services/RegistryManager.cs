using System;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Win32;
using WrongKeyboardFixer.Core.Helpers;

namespace WrongKeyboardFixer.Core.Services;

/// <summary>
///     Thin wrapper around the Windows registry <c>Run</c> key that controls
///     whether the application starts when Windows boots.
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
            // Open the Run key for writing (creates if it doesn't exist)
            using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath);
            if (key == null)
            {
                Debug.WriteLine($"❌ Failed to create/open registry key: {RunKeyPath}");
                return;
            }

            if (enable)
            {
                // Ensure the executable path is properly quoted for registry
                string pathToRun = $"\"{ExecutablePath}\"";
                key.SetValue(AutoStartKeyName, pathToRun);
                Debug.WriteLine($"✅ Set auto-start: {pathToRun}");
            }
            else
            {
                if (key.GetValue(AutoStartKeyName) is not null)
                {
                    key.DeleteValue(AutoStartKeyName);
                    Debug.WriteLine("✅ Removed auto-start entry");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Registry error: {ex.Message}");
            MessageBox.Show(
                Localization.Format("StartupSettingsError", ex.Message),
                Localization.Get("Error"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}