namespace WrongKeyboardFixer.UI.Components;

/// <summary>
///     Tracks the current mouse-hover state across a <see cref="MappingGrid" />'s
///     rows/columns so cell painting can render hover/press highlights cheaply
///     without re-computing hit tests.
/// </summary>
internal sealed class HoverState
{
    public int Column = -1;
    public bool Pressed;
    public int Row = -1;

    public void Reset()
    {
        Row = Column = -1;
        Pressed = false;
    }
}