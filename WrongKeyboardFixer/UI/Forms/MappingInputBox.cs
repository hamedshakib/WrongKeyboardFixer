using System.Drawing;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;
using WrongKeyboardFixer.UI.Components;

namespace WrongKeyboardFixer.UI.Forms;

/// <summary>
///     Single dialog for capturing two characters (replaces the previous
///     two sequential InputBox calls).
/// </summary>
public static class MappingInputBox
{
    public static (string first, string second)? Show(string title, string prompt1, string prompt2)
    {
        using var form = new Form
        {
            Text = title,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            StartPosition = FormStartPosition.CenterParent,
            Size = new Size(Constants.UI.MappingInputBoxWidth, Constants.UI.MappingInputBoxHeight),
            BackColor = Theme.Surface,
            Font = Theme.BodyFont,
            RightToLeft = Localization.IsRtl ? RightToLeft.Yes : RightToLeft.No,
            RightToLeftLayout = true,
            AutoScaleDimensions = new SizeF(96F, 96F),
            AutoScaleMode = AutoScaleMode.Dpi
        };

        var firstLabel = new Label
        {
            Text = prompt1,
            AutoSize = true,
            Location = new Point(Constants.UI.MappingInputBoxLabelX, Constants.UI.MappingInputBoxLabelY),
            ForeColor = Theme.TextPrimary
        };
        form.Controls.Add(firstLabel);

        var firstTextBox = new TextBox
        {
            Location = new Point(Constants.UI.MappingInputBoxLabelX, Constants.UI.MappingInputBoxFirstTextY),
            Size = new Size(Constants.UI.MappingInputBoxTextBoxWidth, Constants.UI.MappingInputBoxTextBoxHeight),
            MaxLength = 1,
            TextAlign = HorizontalAlignment.Center,
            Font = Theme.TitleFont,
            BorderStyle = BorderStyle.FixedSingle,
            ForeColor = Theme.TextPrimary,
            BackColor = Theme.SurfaceAlt
        };
        form.Controls.Add(firstTextBox);

        var secondLabel = new Label
        {
            Text = prompt2,
            AutoSize = true,
            Location = new Point(Constants.UI.MappingInputBoxLabelX, Constants.UI.MappingInputBoxSecondLabelY),
            ForeColor = Theme.TextPrimary
        };
        form.Controls.Add(secondLabel);

        var secondTextBox = new TextBox
        {
            Location = new Point(Constants.UI.MappingInputBoxLabelX, Constants.UI.MappingInputBoxSecondTextY),
            Size = new Size(Constants.UI.MappingInputBoxTextBoxWidth, Constants.UI.MappingInputBoxTextBoxHeight),
            MaxLength = 1,
            TextAlign = HorizontalAlignment.Center,
            Font = Theme.TitleFont,
            BorderStyle = BorderStyle.FixedSingle,
            ForeColor = Theme.TextPrimary,
            BackColor = Theme.SurfaceAlt
        };
        form.Controls.Add(secondTextBox);

        var okButton = new ModernButton
        {
            Text = Localization.Get("OK"),
            ButtonVariant = ModernButton.Variant.Primary,
            DialogResult = DialogResult.OK,
            Location = new Point(Constants.UI.MappingInputBoxOkButtonX, Constants.UI.MappingInputBoxButtonY),
            Size = new Size(Constants.UI.MappingInputBoxButtonWidth, Constants.UI.StdButtonHeight)
        };
        form.Controls.Add(okButton);

        var cancelButton = new ModernButton
        {
            Text = Localization.Get("Cancel"),
            ButtonVariant = ModernButton.Variant.Secondary,
            DialogResult = DialogResult.Cancel,
            Location = new Point(Constants.UI.MappingInputBoxCancelButtonX, Constants.UI.MappingInputBoxButtonY),
            Size = new Size(Constants.UI.MappingInputBoxButtonWidth, Constants.UI.StdButtonHeight)
        };
        form.Controls.Add(cancelButton);

        form.Icon = IconLoader.GetIcon();
        form.Shown += (_, _) =>
        {
            firstTextBox.Focus();
            firstTextBox.SelectAll();
        };
        form.AcceptButton = okButton;
        form.CancelButton = cancelButton;

        return form.ShowDialog() == DialogResult.OK ? (firstTextBox.Text, secondTextBox.Text) : null;
    }
}