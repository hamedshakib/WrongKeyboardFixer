using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;

namespace WrongKeyboardFixer.Core.Models;

/// <summary>
///     Single source of truth for the hotkey combinations offered in the settings UI.
///     Keeps the display order and the underlying values together so the form no
///     longer needs duplicated switch/if chains.
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

    private static readonly (string LocalizationKey, Keys Key)[] KeyOptions =
    {
        ("HotkeyKeyAdd", Keys.Add),
        ("HotkeyKeySubtract", Keys.Subtract),
        ("HotkeyKeyMultiply", Keys.Multiply),
        ("HotkeyKeyF1", Keys.F1),
        ("HotkeyKeyF2", Keys.F2),
        ("HotkeyKeyF3", Keys.F3),
        ("HotkeyKeyF4", Keys.F4),
        ("HotkeyKeyF5", Keys.F5),
        ("HotkeyKeyF6", Keys.F6),
        ("HotkeyKeyF7", Keys.F7),
        ("HotkeyKeyF8", Keys.F8),
        ("HotkeyKeyF9", Keys.F9),
        ("HotkeyKeyF10", Keys.F10),
        ("HotkeyKeyF11", Keys.F11),
        ("HotkeyKeyF12", Keys.F12),
        ("HotkeyKeyInsert", Keys.Insert),
        ("HotkeyKeyHome", Keys.Home),
        ("HotkeyKeyPageUp", Keys.PageUp),
        ("HotkeyKeyPageDown", Keys.PageDown),
        ("HotkeyKeyEnd", Keys.End),
        ("HotkeyKeyDelete", Keys.Delete),
        ("HotkeyKeySpace", Keys.Space)
    };

    public static int ModifierCount => ModifierOptions.Length;
    public static int KeyCount => KeyOptions.Length;

    public static string ModifierText(int index)
    {
        return Localization.Get(ModifierOptions[index].LocalizationKey);
    }

    public static uint ModifierValue(int index)
    {
        return ModifierOptions[index].Modifier;
    }

    public static string KeyText(int index)
    {
        return Localization.Get(KeyOptions[index].LocalizationKey);
    }

    public static Keys KeyValue(int index)
    {
        return KeyOptions[index].Key;
    }

    /// <summary>Returns the combo index matching a modifier value (defaults to 0 = Ctrl+Alt).</summary>
    public static int IndexOfModifier(uint modifier)
    {
        for (int i = 0; i < ModifierOptions.Length; i++)
            if (ModifierOptions[i].Modifier == modifier)
                return i;
        return 0;
    }

    /// <summary>Returns the combo index matching a key (defaults to 0 = Add).</summary>
    public static int IndexOfKey(Keys key)
    {
        for (int i = 0; i < KeyOptions.Length; i++)
            if (KeyOptions[i].Key == key)
                return i;
        return 0;
    }
}