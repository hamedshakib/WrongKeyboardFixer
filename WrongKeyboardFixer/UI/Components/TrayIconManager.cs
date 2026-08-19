using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;

namespace WrongKeyboardFixer.UI.Components;

/// <summary>
///     Owns the system tray icon and its context menu. Exposes the user's
///     actions (open settings / exit) as events; the menu is rebuilt whenever
///     the active language changes.
/// </summary>
public sealed class TrayIconManager : IDisposable
{
    private readonly NotifyIcon _trayIcon;
    private ContextMenuStrip? _trayMenu;

    public TrayIconManager()
    {
        _trayIcon = new NotifyIcon
        {
            Icon = IconLoader.GetIcon(),
            Visible = true,
            Text = Localization.Get("TrayText")
        };

        _trayIcon.DoubleClick += (_, _) => SettingsRequested?.Invoke(this, EventArgs.Empty);
        RebuildMenu();
    }

    public void Dispose()
    {
        _trayMenu?.Dispose();
        _trayIcon.Dispose();
    }

    public event EventHandler? SettingsRequested;
    public event EventHandler? ExitRequested;

    /// <summary>
    ///     Recreates the context menu using the current language.
    /// </summary>
    public void RebuildMenu()
    {
        _trayMenu?.Dispose();
        _trayMenu = BuildTrayMenu();
        _trayIcon.ContextMenuStrip = _trayMenu;
        _trayIcon.Text = Localization.Get("TrayText");
    }

    private ContextMenuStrip BuildTrayMenu()
    {
        var menu = new ContextMenuStrip
        {
            Renderer = Theme.CreateMenuRenderer(),
            Font = Theme.BodyFont,
            BackColor = Theme.Surface,
            ForeColor = Theme.TextPrimary,
            ShowImageMargin = true
        };

        var settingsItem = new ToolStripMenuItem(Localization.Get("TraySettings"));
        settingsItem.Image = CreateEmojiIcon("⚙️");
        settingsItem.Click += (_, _) => SettingsRequested?.Invoke(this, EventArgs.Empty);
        menu.Items.Add(settingsItem);

        menu.Items.Add("-"); // جداکننده

        var exitItem = new ToolStripMenuItem(Localization.Get("TrayExit"));
        exitItem.Image = CreateEmojiIcon("⏻");
        exitItem.Click += (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty);
        menu.Items.Add(exitItem);

        return menu;
    }

    /// <summary>Renders an emoji to an image for use as a menu item icon.</summary>
    private static Bitmap CreateEmojiIcon(string emoji)
    {
        var bitmap = new Bitmap(16, 16);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

        using var font = new Font("Segoe UI Emoji", 11f, FontStyle.Regular, GraphicsUnit.Pixel);
        using var brush = new SolidBrush(Color.Black);

        var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        var rect = new RectangleF(0, 0, 16, 16);
        graphics.DrawString(emoji, font, brush, rect, format);

        return bitmap;
    }
}