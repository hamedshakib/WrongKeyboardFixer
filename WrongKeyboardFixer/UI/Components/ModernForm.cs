using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;

namespace WrongKeyboardFixer.UI.Components;

/// <summary>
/// Base class for the app's borderless dialogs. Provides the common window
/// styles (clip children/siblings) and a typed title bar so subclasses
/// don't repeat the boilerplate.
/// </summary>
public class ModernForm : Form
{
    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.Style |= 0x02000000;   // WS_CLIPCHILDREN → پس‌زمینهٔ فرم زیر بچه‌ها repaint نمی‌شود
            cp.Style |= 0x04000000;   // WS_CLIPSIBLINGS → کنترل‌ها روی هم overwrite نمی‌کنند
            return cp;
        }
    }

    /// <summary>
    /// Creates and adds a <see cref="ModernTitleBar"/> at the top of the form.
    /// </summary>
    protected ModernTitleBar AddTitleBar(string titleKey, string subtitleKey)
    {
        var titleBar = new ModernTitleBar
        {
            Dock = DockStyle.Top,
            Text = Localization.Get(titleKey),
            Subtitle = Localization.Get(subtitleKey)
        };
        Controls.Add(titleBar);
        return titleBar;
    }
}