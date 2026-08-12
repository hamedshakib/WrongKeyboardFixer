using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace WrongKeyboardFixer.Core.UI;

// در حالت Native AOT سازنده‌های هدرسل‌ها (که DataGridView آن‌ها را به‌صورت تنبل با
// Activator.CreateInstance می‌سازد، مثل ReleaseUiaProvider هنگام بسته شدن فرم)
// حذف می‌شوند؛ این‌جا صریحاً روت می‌شوند تا Reflection در زمان اجرا کار کند.
public class ModernDataGridView : DataGridView
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(DataGridViewRowHeaderCell))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(DataGridViewColumnHeaderCell))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(DataGridViewTopLeftHeaderCell))]
    public ModernDataGridView()
    {
        // nsjvsd lsjrdl fn,k Reflection (;hlghQ shc'hv fh AOT)
        // دسترسی مستقیم بدون Reflection (کاملاً سازگار با AOT)
        this.DoubleBuffered = true;
    }
}
