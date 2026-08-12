namespace WrongKeyboardFixer.Core.Contracts;

/// <summary>
/// Provides methods for managing system-wide hotkeys.
/// </summary>
public interface IHotkeyService : IDisposable
{
    /// <summary>
    /// Registers a hotkey combination.
    /// </summary>
    bool Register(uint modifiers, Keys key);

    /// <summary>
    /// Unregisters the currently registered hotkey.
    /// </summary>
    void Unregister();

    /// <summary>
    /// Checks if a message is a hotkey message for this instance.
    /// </summary>
    bool HandleHotkeyMessage(ref Message message);
}
