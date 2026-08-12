using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;
using WrongKeyboardFixer.Core.Model;

namespace WrongKeyboardFixer.Core.Services;

/// <summary>
/// Single source of truth for the hotkey combinations offered in the settings UI.
/// Keeps the display order and the underlying values together so the form no
/// longer needs duplicated switch/if chains.
/// </summary>
public static class HotkeyOptions
{
    private static readonly (string LocalizationKey, uint Modifier)[] Modifiers =
    {
        ("HotkeyModifierCtrlAlt", (uint)(HotkeyModifiers.Control | HotkeyModifiers.Alt)),
        ("HotkeyModifierCtrlShift", (uint)(HotkeyModifiers.Control | HotkeyModifiers.Shift)),
        ("HotkeyModifierAltShift", (uint)(HotkeyModifiers.Alt | HotkeyModifiers.Shift)),
        ("HotkeyModifierCtrl", (uint)HotkeyModifiers.Control),
        ("HotkeyModifierAlt", (uint)HotkeyModifiers.Alt),
        ("HotkeyModifierShift", (uint)HotkeyModifiers.Shift)
    };

    private static readonly (string LocalizationKey, Keys Key)[] KeysOptions =
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

    public static int ModifierCount => Modifiers.Length;
    public static int KeyCount => KeysOptions.Length;

    public static string ModifierText(int index) => Localization.Get(Modifiers[index].LocalizationKey);
    public static uint ModifierValue(int index) => Modifiers[index].Modifier;

    public static string KeyText(int index) => Localization.Get(KeysOptions[index].LocalizationKey);
    public static Keys KeyValue(int index) => KeysOptions[index].Key;

    /// <summary>Returns the combo index matching a modifier value (defaults to 0 = Ctrl+Alt).</summary>
    public static int IndexOfModifier(uint modifier)
    {
        for (int i = 0; i < Modifiers.Length; i++)
            if (Modifiers[i].Modifier == modifier) return i;
        return 0;
    }

    /// <summary>Returns the combo index matching a key (defaults to 0 = Add).</summary>
    public static int IndexOfKey(Keys key)
    {
        for (int i = 0; i < KeysOptions.Length; i++)
            if (KeysOptions[i].Key == key) return i;
        return 0;
    }
}