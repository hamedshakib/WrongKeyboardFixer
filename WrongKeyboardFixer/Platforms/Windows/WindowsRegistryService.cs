using Microsoft.Win32;
using System.Diagnostics;

namespace WrongKeyboardFixer.Platforms.Windows;

/// <summary>
/// Manages the Windows auto-start registry entry (HKCU\Software\Microsoft\Windows\CurrentVersion\Run).
/// </summary>
public sealed class WindowsRegistryService
{
    private const string AutoStartKeyName = "WrongKeyboardFixer";
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public void SetAutoStart(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath);
            if (key == null)
            {
                Debug.WriteLine($"Failed to create/open registry key: {RunKeyPath}");
                return;
            }

            if (enable)
            {
                string exePath = Environment.ProcessPath ?? "";
                string pathToRun = $"\"{exePath}\"";
                key.SetValue(AutoStartKeyName, pathToRun);
            }
            else
            {
                if (key.GetValue(AutoStartKeyName) is not null)
                {
                    key.DeleteValue(AutoStartKeyName);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Registry error: {ex.Message}");
        }
    }
}
