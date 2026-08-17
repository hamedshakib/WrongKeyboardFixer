namespace WrongKeyboardFixer.Core.Services;

/// <summary>
/// Abstraction for clipboard operations.
/// </summary>
public interface IClipboardService
{
    string GetText();
    Task<string> GetTextWithRetryAsync();
    void SetText(string text);
    void RestoreText(string text);
}

/// <summary>
/// Abstraction for keyboard simulation (Ctrl+C / Ctrl+V).
/// </summary>
public interface IKeyboardSimulator
{
    void SendCtrlC();
    void SendCtrlV();
}
