using System.Windows.Forms;

namespace WrongKeyboardFixer.Core.UI;

public class ModernDataGridView : DataGridView
{
    public ModernDataGridView()
    {
        // دسترسی مستقیم بدون Reflection (کاملاً سازگار با AOT)
        this.DoubleBuffered = true;
    }
}
