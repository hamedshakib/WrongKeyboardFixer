namespace WrongKeyboardFixer.Core.Services;

/// <summary>
/// Interface for keyboard simulation operations.
/// Defines the contract for simulating keyboard input via SendInput API.
/// Note: Implementation is static methods - use KeyboardSimulator directly.
/// This interface is kept for documentation purposes.
/// </summary>
public interface IKeyboardSimulator
{
    /// <summary>
    /// Sends a Ctrl+C keyboard combination to copy the current selection.
    /// </summary>
    void SendCtrlC();

    /// <summary>
    /// Sends a Ctrl+V keyboard combination to paste clipboard contents.
    /// </summary>
    void SendCtrlV();
}

/// <summary>
/// Static keyboard simulator implementation.
/// Provides direct access to keyboard simulation methods.
/// </summary>
public static class KeyboardSimulatorAdapter
{
    /// <summary>
    /// Sends a Ctrl+C keyboard combination to copy the current selection.
    /// </summary>
    public static void SendCtrlC() => KeyboardSimulator.SendCtrlC();

    /// <summary>
    /// Sends a Ctrl+V keyboard combination to paste clipboard contents.
    /// </summary>
    public static void SendCtrlV() => KeyboardSimulator.SendCtrlV();
}