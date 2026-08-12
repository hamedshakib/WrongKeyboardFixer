using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace WrongKeyboardFixer.Core.Services;

/// <summary>
/// Simulates keyboard input (Ctrl+C / Ctrl+V) via the modern <c>SendInput</c>
/// API instead of the deprecated <c>keybd_event</c>.
/// </summary>
public static partial class KeyboardSimulator
{
    private const uint InputKeyboard = 1;
    private const uint KeyEventFKeyUp = 0x0002;

    private const byte VkControl = 0x11;
    private const byte VkC = 0x43;
    private const byte VkV = 0x56;
    private const byte VkMenu = 0x12; // Alt

    private const int KeyReleaseDelayMs = 30;

    [LibraryImport("user32.dll")]
    private static partial uint SendInput(uint nInputs, ReadOnlySpan<Input> pInputs, int cbSize);

    /// <summary>
    /// Sends a Ctrl+C keyboard combination to copy the current selection.
    /// </summary>
    public static void SendCtrlC()
    {
        ReleaseModifierKeys();
        Thread.Sleep(KeyReleaseDelayMs);
        SendKeyCombination(VkControl, VkC);
    }

    /// <summary>
    /// Sends a Ctrl+V keyboard combination to paste clipboard contents.
    /// </summary>
    public static void SendCtrlV()
    {
        ReleaseModifierKeys();
        Thread.Sleep(KeyReleaseDelayMs);
        SendKeyCombination(VkControl, VkV);
    }

    /// <summary>
    /// Releases any held Alt/Ctrl modifier keys before sending our combination,
    /// preventing stuck modifiers.
    /// </summary>
    private static void ReleaseModifierKeys()
    {
        SendInput(2, CreateKeyUpInputs(VkMenu, VkControl), InputSize);
    }

    /// <summary>
    /// Sends a modifier + key press/release sequence via SendInput.
    /// </summary>
    private static void SendKeyCombination(byte modifier, byte key)
    {
        Span<Input> inputs = stackalloc Input[4];

        inputs[0] = KeyboardInput(modifier, 0);               // Press modifier
        inputs[1] = KeyboardInput(key, 0);                    // Press key
        inputs[2] = KeyboardInput(key, KeyEventFKeyUp);       // Release key
        inputs[3] = KeyboardInput(modifier, KeyEventFKeyUp);  // Release modifier

        SendInput(4, inputs, InputSize);
    }

    /// <summary>
    /// Creates an array of key-up INPUT records for the given virtual keys.
    /// </summary>
    private static Input[] CreateKeyUpInputs(params byte[] virtualKeys)
    {
        var inputs = new Input[virtualKeys.Length];
        for (int i = 0; i < virtualKeys.Length; i++)
            inputs[i] = KeyboardInput(virtualKeys[i], KeyEventFKeyUp);
        return inputs;
    }

    private static Input KeyboardInput(byte virtualKey, uint flags)
    {
        return new Input
        {
            Type = InputKeyboard,
            Union = new InputUnion
            {
                Ki = new KeybdInput { Vk = virtualKey, Flags = flags }
            }
        };
    }

    /// <summary>
    /// Cached size of <see cref="Input"/> for the <c>SendInput</c> cbSize parameter.
    /// </summary>
    private static readonly int InputSize = Marshal.SizeOf<Input>();

    /// <summary>
    /// Mirrors the native Win32 <c>INPUT</c> structure (type + union).
    /// The union must be declared explicitly so the struct is laid out exactly
    /// like the native type; otherwise <c>SendInput</c> fails because <c>cbSize</c>
    /// (verified via <see cref="Marshal.SizeOf{T}"/>) does not match.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public uint Type;
        public InputUnion Union;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public MouseInput Mi;
        [FieldOffset(0)] public KeybdInput Ki;
        [FieldOffset(0)] public HardwareInput Hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseInput
    {
        public int Dx;
        public int Dy;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public UIntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeybdInput
    {
        public ushort Vk;
        public ushort Scan;
        public uint Flags;
        public uint Time;
        public UIntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HardwareInput
    {
        public uint Msg;
        public ushort ParamL;
        public ushort ParamH;
    }
}
