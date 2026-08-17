using static WrongKeyboardFixer.Platforms.Windows.Win32Methods;

namespace WrongKeyboardFixer.Platforms.Windows;

/// <summary>
/// Simulates Ctrl+C / Ctrl+V keyboard input via Win32 SendInput API.
/// </summary>
public sealed class WindowsKeyboardSimulator : Core.Services.IKeyboardSimulator
{
    private static readonly int InputSize = Marshal.SizeOf<INPUT>();

    public void SendCtrlC()
    {
        ReleaseModifierKeys();
        Thread.Sleep(30);
        SendKeyCombination(VK_CONTROL, VK_C);
    }

    public void SendCtrlV()
    {
        ReleaseModifierKeys();
        Thread.Sleep(30);
        SendKeyCombination(VK_CONTROL, VK_V);
    }

    private static void ReleaseModifierKeys()
    {
        Span<INPUT> keyUps = stackalloc INPUT[2];
        keyUps[0] = KeyboardInput(VK_MENU, KEYEVENTF_KEYUP);
        keyUps[1] = KeyboardInput(VK_CONTROL, KEYEVENTF_KEYUP);
        SendInput(2, keyUps, InputSize);
    }

    private static void SendKeyCombination(byte modifier, byte key)
    {
        Span<INPUT> inputs = stackalloc INPUT[4];
        inputs[0] = KeyboardInput(modifier, 0);
        inputs[1] = KeyboardInput(key, 0);
        inputs[2] = KeyboardInput(key, KEYEVENTF_KEYUP);
        inputs[3] = KeyboardInput(modifier, KEYEVENTF_KEYUP);
        SendInput(4, inputs, InputSize);
    }

    private static INPUT KeyboardInput(byte virtualKey, uint flags)
    {
        return new INPUT
        {
            Type = INPUT_KEYBOARD,
            Union = new INPUTUNION
            {
                Ki = new KEYBDINPUT { Vk = virtualKey, Flags = flags }
            }
        };
    }
}
