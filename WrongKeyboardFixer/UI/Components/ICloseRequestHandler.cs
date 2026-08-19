namespace WrongKeyboardFixer.UI.Components;

/// <summary>
///     Implemented by forms that own a <see cref="ModernTitleBar" /> so the close
///     button can trigger a custom shutdown flow.
/// </summary>
public interface ICloseRequestHandler
{
    void RequestClose();
}