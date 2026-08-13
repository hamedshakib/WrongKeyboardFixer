using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;
using WrongKeyboardFixer.Core.Models;
using WrongKeyboardFixer.Core.Services;
using WrongKeyboardFixer.UI.Components;

namespace WrongKeyboardFixer.UI.Forms;

public class KeyboardMappingsForm : ModernForm, ICloseRequestHandler
{
    private readonly AppSettings _settings;

    // کپی محلی نگاشت‌ها: فقط هنگام فشردن «ذخیره» روی تنظیمات اصلی اعمال می‌شوند.
    // (قبلاً ویرایش سلول مستقیماً _settings را تغییر می‌داد و «انصراف» بی‌اثر بود)
    private Dictionary<char, char> _persianToEnglish;
    private Dictionary<char, char> _englishToPersian;

    private MappingGrid? _persianToEnglishGrid;
    private MappingGrid? _englishToPersianGrid;
    private ModernButton? _saveButton;
    private ModernButton? _cancelButton;
    private Label? _persianToEnglishCountLabel;
    private Label? _englishToPersianCountLabel;

    private const int FormWidth = 920;
    private const int FormHeight = 600;

    public KeyboardMappingsForm(AppSettings settings)
    {
        _settings = settings ?? new AppSettings();
        _persianToEnglish = new Dictionary<char, char>(_settings.PersianToEnglishMap ?? new Dictionary<char, char>());
        _englishToPersian = new Dictionary<char, char>(_settings.EnglishToPersianMap ?? new Dictionary<char, char>());

        Localization.SetLanguage(_settings.Language);

        this.SuspendLayout();
        InitializeControls();
        this.ResumeLayout(false);

        LoadMappings();
    }

    private void InitializeControls()
    {
        this.Text = Localization.Get("KeyboardMappingsTitle");
        this.FormBorderStyle = FormBorderStyle.None;
        this.AutoScaleDimensions = new SizeF(96F, 96F);
        this.AutoScaleMode = AutoScaleMode.Dpi;
        this.ClientSize = new Size(FormWidth, FormHeight);
        this.MinimumSize = new Size(FormWidth, FormHeight);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = Theme.BodyFont;
        this.Icon = IconLoader.GetIcon();
        this.RightToLeft = Localization.IsRtl ? RightToLeft.Yes : RightToLeft.No;
        this.RightToLeftLayout = Localization.IsRtl;

        AddTitleBar("KeyboardMappingsTitle", "KeyboardMappingsSubtitle");

        var panel = new RoundedPanel
        {
            CornerRadius = 14,
            BackColor = Theme.Surface,
            Padding = new Padding(24),
            Location = new Point(14, ModernTitleBar.TitleBarHeight + 10),
            Size = new Size(FormWidth - 28, FormHeight - ModernTitleBar.TitleBarHeight - 24)
        };
        this.Controls.Add(panel);

        int left = panel.Padding.Left;
        int right = panel.Width - panel.Padding.Right;
        int innerWidth = right - left;
        const int cardGap = 16;
        int cardWidth = (innerWidth - cardGap) / 2;
        const int cardY = 14;
        const int gridHeight = 306;

        // ── کارت فارسی → انگلیسی ──────────────────────────
        _persianToEnglishCountLabel = Theme.BodyLabel("", Theme.TextSecondary, Theme.SmallFont);
        _persianToEnglishGrid = CreateGrid(MappingGrid.Direction.PersianToEnglish,
            DeletePersianMapping, ResetPersianCharMapping, (k, v) => _persianToEnglish[k] = v);
        panel.Controls.Add(BuildMappingCard(
            left, cardWidth, cardY, gridHeight,
            "PersianToEnglish",
            _persianToEnglishCountLabel,
            _persianToEnglishGrid,
            AddPersianToEnglishButton_Click,
            ResetAllPersianToEnglishButton_Click));

        // ── کارت انگلیسی → فارسی ──────────────────────────
        _englishToPersianCountLabel = Theme.BodyLabel("", Theme.TextSecondary, Theme.SmallFont);
        _englishToPersianGrid = CreateGrid(MappingGrid.Direction.EnglishToPersian,
            DeleteEnglishMapping, ResetEnglishCharMapping, (k, v) => _englishToPersian[k] = v);
        panel.Controls.Add(BuildMappingCard(
            left + cardWidth + cardGap, cardWidth, cardY, gridHeight,
            "EnglishToPersian",
            _englishToPersianCountLabel,
            _englishToPersianGrid,
            AddEnglishToPersianButton_Click,
            ResetAllEnglishToPersianButton_Click));

        // ── فوتر ──────────────────────────────────────────
        int footerY = panel.Height - panel.Padding.Bottom - 42;

        _cancelButton = new ModernButton
        {
            Text = Localization.Get("Cancel"),
            ButtonVariant = ModernButton.Variant.Secondary,
            Location = new Point(right - 112, footerY),
            Size = new Size(112, 38),
            Anchor = AnchorStyles.Right | AnchorStyles.Bottom
        };
        _cancelButton.Click += CancelButton_Click;
        panel.Controls.Add(_cancelButton);

        _saveButton = new ModernButton
        {
            Text = Localization.Get("Save"),
            ButtonVariant = ModernButton.Variant.Primary,
            Location = new Point(right - 112 - 124, footerY),
            Size = new Size(112, 38),
            Anchor = AnchorStyles.Right | AnchorStyles.Bottom
        };
        _saveButton.Click += SaveButton_Click;
        panel.Controls.Add(_saveButton);

        var footerDivider = new RoundedPanel
        {
            Height = 1,
            Width = innerWidth,
            Location = new Point(left, footerY - 16),
            CornerRadius = 0,
            BackColor = Theme.Border,
            BorderWidth = 0,
            Padding = new Padding(0),
            TabStop = false,
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };
        panel.Controls.Add(footerDivider);
        this.ActiveControl = null;
    }

    private static MappingGrid CreateGrid(
        MappingGrid.Direction direction,
        Action<char> deleteHandler,
        Action<char> resetHandler,
        Action<char, char> commitHandler)
    {
        var grid = new MappingGrid(direction);
        grid.DeleteRequested += deleteHandler;
        grid.ResetRequested += resetHandler;
        grid.ValueCommitted += commitHandler;
        return grid;
    }

    private static int BuildCardHeight(int gridHeight) => 14 + 26 + 6 + 38 + 10 + gridHeight + 14;

    /// <summary>
    /// Builds one mapping card (title, count label, add/reset buttons and the mapping grid).
    /// Shared by the Persian→English and English→Persian cards to avoid duplicated layout code.
    /// </summary>
    private RoundedPanel BuildMappingCard(
        int left, int cardWidth, int cardY, int gridHeight,
        string titleKey,
        Label countLabel,
        MappingGrid grid,
        EventHandler addClick,
        EventHandler resetClick)
    {
        var card = new RoundedPanel
        {
            CornerRadius = 12,
            BackColor = Theme.SurfaceAlt,
            BorderColor = Theme.Border,
            BorderWidth = 1,
            Padding = new Padding(14),
            Location = new Point(left, cardY),
            Size = new Size(cardWidth, BuildCardHeight(gridHeight))
        };

        card.Controls.Add(Theme.SectionLabel(Localization.Get(titleKey))
            .Then(l => l.Location = new Point(14, 12)));

        countLabel.Location = new Point(cardWidth - 14 - 130, 14);
        countLabel.Size = new Size(130, 22);
        countLabel.TextAlign = ContentAlignment.MiddleRight;
        countLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        card.Controls.Add(countLabel);

        var addButton = new ModernButton
        {
            Text = "+ " + Localization.Get("AddNewMapping"),
            ButtonVariant = ModernButton.Variant.Primary,
            Location = new Point(14, 46),
            Size = new Size(160, 38)
        };
        addButton.Click += addClick;
        card.Controls.Add(addButton);

        var resetButton = new ModernButton
        {
            Text = Localization.Get("ResetAll"),
            ButtonVariant = ModernButton.Variant.Ghost,
            Location = new Point(14 + 160 + 10, 46),
            Size = new Size(170, 38)
        };
        resetButton.Click += resetClick;
        card.Controls.Add(resetButton);

        grid.Location = new Point(14, 94);
        grid.Size = new Size(cardWidth - 28, gridHeight);
        card.Controls.Add(grid);

        return card;
    }

    // ── بارگذاری: با SuspendLayout و بدون انتخاب اضافی ──
    private void LoadMappings()
    {
        if (_persianToEnglishGrid is { } p2eGrid)
        {
            p2eGrid.Load(_persianToEnglish);
            _persianToEnglishCountLabel!.Text = Localization.Format("MappingsCount", _persianToEnglish.Count);
        }

        if (_englishToPersianGrid is { } e2pGrid)
        {
            e2pGrid.Load(_englishToPersian);
            _englishToPersianCountLabel!.Text = Localization.Format("MappingsCount", _englishToPersian.Count);
        }
    }

    // ── عملیات نگاشت → فقط کپی محلی ──
    private void AddPersianToEnglishButton_Click(object? sender, EventArgs e) => AddPersianToEnglishMapping();
    private void AddEnglishToPersianButton_Click(object? sender, EventArgs e) => AddEnglishToPersianMapping();

    private void ResetAllPersianToEnglishButton_Click(object? sender, EventArgs e)
    {
        if (ConfirmReset() != DialogResult.Yes) return;
        _persianToEnglish = MappingDefaults.GetDefaultPersianToEnglishMap();
        LoadMappings();
    }

    private void ResetAllEnglishToPersianButton_Click(object? sender, EventArgs e)
    {
        if (ConfirmReset() != DialogResult.Yes) return;
        _englishToPersian = MappingDefaults.GetDefaultEnglishToPersianMap();
        LoadMappings();
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        // فقط حالا تغییرات روی تنظیمات اصلی اعمال می‌شود
        _settings.PersianToEnglishMap = new Dictionary<char, char>(_persianToEnglish);
        _settings.EnglishToPersianMap = new Dictionary<char, char>(_englishToPersian);
        SettingsManager.Save(_settings);
        DialogResult = DialogResult.OK;
        Close();
    }

    private void CancelButton_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    public void RequestClose() => CancelButton_Click(this, EventArgs.Empty);

    // ── افزودن نگاشت: فقط یک دیالوگ (به‌جای دو InputBox پشت‌سرهم) ──
    private void AddPersianToEnglishMapping()
    {
        var input = MappingInputBox.Show(
            Localization.Get("AddMapping"),
            Localization.Get("EnterPersianChar"),
            Localization.Get("EnterEnglishChar"));
        if (input is null) return;

        string persian = input.Value.first.Trim();
        string english = input.Value.second.Trim();

        if (persian.Length != 1) { ShowError("InvalidPersianChar"); return; }
        if (english.Length != 1 || english[0] < 32 || english[0] > 126) { ShowError("InvalidEnglishChar"); return; }

        char p = persian[0], en = english[0];
        _englishToPersian.Remove(en);
        _persianToEnglish[p] = en;
        LoadMappings();
    }

    private void AddEnglishToPersianMapping()
    {
        var input = MappingInputBox.Show(
            Localization.Get("AddMapping"),
            Localization.Get("EnterEnglishChar"),
            Localization.Get("EnterPersianChar"));
        if (input is null) return;

        string english = input.Value.first.Trim();
        string persian = input.Value.second.Trim();

        if (english.Length != 1 || english[0] < 32 || english[0] > 126) { ShowError("InvalidEnglishChar"); return; }
        if (persian.Length != 1) { ShowError("InvalidPersianChar"); return; }

        char en = english[0], p = persian[0];
        _persianToEnglish.Remove(p);
        _englishToPersian[en] = p;
        LoadMappings();
    }

    private void DeletePersianMapping(char persianChar)
    {
        if (_persianToEnglish.Remove(persianChar)) LoadMappings();
    }

    private void DeleteEnglishMapping(char englishChar)
    {
        if (_englishToPersian.Remove(englishChar)) LoadMappings();
    }

    private void ResetPersianCharMapping(char persianChar)
    {
        if (_persianToEnglish.Remove(persianChar)) LoadMappings();
    }

    private void ResetEnglishCharMapping(char englishChar)
    {
        if (_englishToPersian.Remove(englishChar)) LoadMappings();
    }

    private static DialogResult ConfirmReset() => MessageBox.Show(
        Localization.Get("ResetAllConfirm"),
        Localization.Get("Attention"),
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question);

    private static void ShowError(string key) => MessageBox.Show(
        Localization.Get(key),
        Localization.Get("Error"),
        MessageBoxButtons.OK,
        MessageBoxIcon.Error);
}
