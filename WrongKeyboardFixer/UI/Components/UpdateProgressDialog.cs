using System;
using System.Drawing;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;

namespace WrongKeyboardFixer.UI.Components;

/// <summary>
///     A small non-interactive progress dialog used while checking / downloading updates.
///     Owns its window and an <see cref="IProgress{T}" /> reporter that updates it safely.
/// </summary>
public sealed class UpdateProgressDialog : IDisposable
{
    private readonly Form _form;
    private readonly Label _label;
    private readonly ProgressBar _progressBar;

    public UpdateProgressDialog(IWin32Window? owner = null)
    {
        _form = new Form
        {
            Text = Localization.Get("CheckingUpdate"),
            Size = new Size(Constants.Forms.UpdateProgressDialogWidth, Constants.Forms.UpdateProgressDialogHeight),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterScreen,
            MaximizeBox = false,
            MinimizeBox = false,
            RightToLeft = Localization.IsRtl ? RightToLeft.Yes : RightToLeft.No,
            RightToLeftLayout = true,
            ControlBox = false,
            AutoScaleDimensions = new SizeF(96F, 96F),
            AutoScaleMode = AutoScaleMode.Dpi
        };

        _label = new Label
        {
            Text = Localization.Get("CheckingProgress"),
            Dock = DockStyle.Top,
            Height = 30,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Tahoma", 9)
        };

        _progressBar = new ProgressBar
        {
            Dock = DockStyle.Bottom,
            Height = 25,
            Minimum = 0,
            Maximum = 100,
            Value = 0
        };

        _form.Controls.Add(_label);
        _form.Controls.Add(_progressBar);

        Progress = new Progress<(int percent, string message)>(update =>
        {
            _progressBar.Value = Math.Min(update.percent, 100);
            _label.Text = update.message;
        });

        if (owner != null)
            _form.Show(owner);
        else
            _form.Show();
    }

    /// <summary>Reporter that updates the progress bar and message text.</summary>
    public IProgress<(int percent, string message)> Progress { get; }

    public void Dispose()
    {
        _form.Dispose();
    }

    /// <summary>Closes the dialog window.</summary>
    public void Close()
    {
        _form.Close();
    }
}