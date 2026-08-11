using System;
using System.Windows.Forms;

namespace WrongKeyboardFixer.Core.UI;

public partial class KeyboardMappingsForm : Form
{
    public KeyboardMappingsForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Keyboard Mappings";
        this.Size = new System.Drawing.Size(400, 300);
        this.StartPosition = FormStartPosition.CenterParent;

        var label = new Label
        {
            Text = "Keyboard Mappings editor coming soon...",
            Location = new System.Drawing.Point(20, 20),
            AutoSize = true
        };
        Controls.Add(label);

        var okButton = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Location = new System.Drawing.Point(160, 230),
            Size = new System.Drawing.Size(80, 30)
        };
        okButton.Click += (s, e) => Close();
        Controls.Add(okButton);
    }
}