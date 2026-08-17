using WrongKeyboardFixer.Core.Helpers;

namespace WrongKeyboardFixer.Core.Models;

/// <summary>
/// Single source of truth for the hotkey combinations offered in the settings UI.
/// </summary>
public static class HotkeyOptions
{
    private static readonly (string LocalizationKey, uint Modifier)[] ModifierOptions =
    {
        ("HotkeyModifierCtrlAlt", (uint)(HotkeyModifiers.Control | HotkeyModifiers.Alt)),
        ("HotkeyModifierCtrlShift", (uint)(HotkeyModifiers.Control | HotkeyModifiers.Shift)),
        ("HotkeyModifierAltShift", (uint)(HotkeyModifiers.Alt | HotkeyModifiers.Shift)),
        ("HotkeyModifierCtrl", (uint)HotkeyModifiers.Control),
        ("HotkeyModifierAlt", (uint)HotkeyModifiers.Alt),
        ("HotkeyModifierShift", (uint)HotkeyModifiers.Shift)
    };

    private static readonly (string LocalizationKey, int Key)[] KeyOptions =
    {
        ("HotkeyKeyAdd", VirtualKeys.Add),
        ("HotkeyKeySubtract", VirtualKeys.Subtract),
        ("HotkeyKeyMultiply", VirtualKeys.Multiply),
        ("HotkeyKeyF1", VirtualKeys.F1),
        ("HotkeyKeyF2", VirtualKeys.F2),
        ("HotkeyKeyF3", VirtualKeys.F3),
        ("HotkeyKeyF4", VirtualKeys.F4),
        ("HotkeyKeyF5", VirtualKeys.F5),
        ("HotkeyKeyF6", VirtualKeys.F6),
        ("HotkeyKeyF7", VirtualKeys.F7),
        ("HotkeyKeyF8", VirtualKeys.F8),
        ("HotkeyKeyF9", VirtualKeys.F9),
        ("HotkeyKeyF10", VirtualKeys.F10),
        ("HotkeyKeyF11", VirtualKeys.F11),
        ("HotkeyKeyF12", VirtualKeys.F12),
        ("HotkeyKeyInsert", VirtualKeys.Insert),
        ("HotkeyKeyHome", VirtualKeys.Home),
        ("HotkeyKeyPageUp", VirtualKeys.PageUp),
        ("HotkeyKeyPageDown", VirtualKeys.PageDown),
        ("HotkeyKeyEnd", VirtualKeys.End),
        ("HotkeyKeyDelete", VirtualKeys.Delete),
        ("HotkeyKeySpace", VirtualKeys.Space)
    };

    public static int ModifierCount => ModifierOptions.Length;
    public static int KeyCount => KeyOptions.Length;

    public static string ModifierText(int index) => Localization.Get(ModifierOptions[index].LocalizationKey);
    public static uint ModifierValue(int index) => ModifierOptions[index].Modifier;

    public static string KeyText(int index) => Localization.Get(KeyOptions[index].LocalizationKey);
    public static int KeyValue(int index) => KeyOptions[index].Key;

    public static int IndexOfModifier(uint modifier)
    {
        for (int i = 0; i < ModifierOptions.Length; i++)
            if (ModifierOptions[i].Modifier == modifier) return i;
        return 0;
    }

    public static int IndexOfKey(int key)
    {
        for (int i = 0; i < KeyOptions.Length; i++)
            if (KeyOptions[i].Key == key) return i;
        return 0;
    }
}
