namespace WrongKeyboardFixer.Core.Contracts;

/// <summary>
/// Provides methods for simulating keyboard input.
/// </summary>
public interface IKeyboardSimulator
{
    /// <summary>
    /// Simulates pressing Ctrl+C to copy selected text.
    /// </summary>
    void SendCtrlC();

    /// <summary>
    /// Simulates pressing Ctrl+V to paste text.
    /// </summary>
    void SendCtrlV();
}
