using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace WrongKeyboardFixer;

public static class KeyboardSimulator
{
    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    private const uint KEYEVENTF_KEYDOWN = 0x0000;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const byte VK_CONTROL = 0x11;
    private const byte VK_C = 0x43;
    private const byte VK_V = 0x56;
    private const byte VK_MENU = 0x12; // Alt
    private const int KeyReleaseDelayMs = 30;

    public static void SendCtrlC()
    {
        ReleaseModifierKeys();
        Thread.Sleep(KeyReleaseDelayMs);

        SendKeyCombination(VK_CONTROL, VK_C);
    }

    public static void SendCtrlV()
    {
        ReleaseModifierKeys();
        Thread.Sleep(KeyReleaseDelayMs);

        SendKeyCombination(VK_CONTROL, VK_V);
    }

    private static void ReleaseModifierKeys()
    {
        keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    }

    private static void SendKeyCombination(byte modifier, byte key)
    {
        // Press modifier
        keybd_event(modifier, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
        // Press key
        keybd_event(key, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
        // Release key
        keybd_event(key, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        // Release modifier
        keybd_event(modifier, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    }
}