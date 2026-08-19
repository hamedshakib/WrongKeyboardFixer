using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WrongKeyboardFixer.UI.Components;

public class ModernTextBox : UserControl
{
    private readonly Color _borderColor = Theme.Border;
    private readonly int _cornerRadius = 8;
    private readonly Color _focusBorderColor = Color.FromArgb(99, 102, 241); // رنگ فوتر/آکست مدرن
    private readonly TextBox _textBox;
    private bool _isFocused;
    private bool _isPlaceholderActive = true;
    private string _placeholderText = string.Empty;

    public ModernTextBox()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw, true);

        Size = new Size(220, 36);
        BackColor = Theme.Surface;

        _textBox = new TextBox
        {
            BorderStyle = BorderStyle.None,
            BackColor = Theme.Surface,
            ForeColor = Theme.TextPrimary,
            Font = Theme.BodyFont
        };

        _textBox.Enter += (s, e) =>
        {
            _isFocused = true;
            if (_isPlaceholderActive)
            {
                _textBox.Text = string.Empty;
                _textBox.ForeColor = Theme.TextPrimary;
                _isPlaceholderActive = false;
            }

            Invalidate();
        };

        _textBox.Leave += (s, e) =>
        {
            _isFocused = false;
            if (string.IsNullOrEmpty(_textBox.Text))
                SetPlaceholderState();
            Invalidate();
        };

        _textBox.TextChanged += (s, e) =>
        {
            if (!_isPlaceholderActive)
                TextChanged?.Invoke(this, e);
        };

        _textBox.KeyDown += (s, e) => { OnKeyDown(e); };
        _textBox.KeyPress += (s, e) => { OnKeyPress(e); };

        Controls.Add(_textBox);
        UpdateTextBoxBounds();
        SetPlaceholderState();
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public override string Text
    {
        get => _isPlaceholderActive ? string.Empty : _textBox.Text;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                SetPlaceholderState();
            }
            else
            {
                _textBox.Text = value;
                _isPlaceholderActive = false;
                _textBox.ForeColor = Theme.TextPrimary;
            }
        }
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [DefaultValue(false)]
    public bool UseSystemPasswordChar
    {
        get => _textBox.UseSystemPasswordChar;
        set => _textBox.UseSystemPasswordChar = value;
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [DefaultValue('\0')]
    public char PasswordChar
    {
        get => _textBox.PasswordChar;
        set => _textBox.PasswordChar = value;
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [DefaultValue("")]
    public string PlaceholderText
    {
        get => _placeholderText;
        set
        {
            _placeholderText = value;
            if (_isPlaceholderActive)
                _textBox.Text = _placeholderText;
        }
    }

    public override Font Font
    {
        get => base.Font;
        set
        {
            base.Font = value;
            if (_textBox != null) _textBox.Font = value;
        }
    }

    public new event EventHandler? TextChanged;

    public void Clear()
    {
        _textBox.Clear();
    }

    private void SetPlaceholderState()
    {
        if (!string.IsNullOrEmpty(_placeholderText))
        {
            _isPlaceholderActive = true;
            _textBox.Text = _placeholderText;
            _textBox.ForeColor = Theme.TextSecondary;
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        _textBox.Focus();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // رسم پس‌زمینه گرد کامپوننت
        using var path = GetRoundedRectanglePath(ClientRectangle, _cornerRadius);
        using var brush = new SolidBrush(BackColor);
        g.FillPath(brush, path);

        // رسم حدور (Border) با تغییر رنگ در حالت فوکوس
        var borderColor = _isFocused ? _focusBorderColor : _borderColor;
        using var pen = new Pen(borderColor, _isFocused ? 1.5f : 1f);

        var rect = ClientRectangle;
        rect.Width -= 1;
        rect.Height -= 1;
        using var borderPath = GetRoundedRectanglePath(rect, _cornerRadius);
        g.DrawPath(pen, borderPath);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateTextBoxBounds();
    }

    private void UpdateTextBoxBounds()
    {
        if (_textBox != null)
        {
            // تنظیم موقعیت متن برای قرارگیری دقیق در وسط به صورت عمودی
            int top = (Height - _textBox.Height) / 2;
            _textBox.Location = new Point(12, Math.Max(2, top));
            _textBox.Width = Width - 24;
        }
    }

    public static GraphicsPath GetRoundedRectanglePath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        int diameter = radius * 2;
        var arcRect = new Rectangle(rect.X, rect.Y, diameter, diameter);

        // گوشه بالا چپ
        path.AddArc(arcRect, 180, 90);
        // گوشه بالا راست
        arcRect.X = rect.Right - diameter;
        path.AddArc(arcRect, 270, 90);
        // گوشه پایین راست
        arcRect.Y = rect.Bottom - diameter;
        path.AddArc(arcRect, 0, 90);
        // گوشه پایین چپ
        arcRect.X = rect.X;
        path.AddArc(arcRect, 90, 90);

        path.CloseFigure();
        return path;
    }
}