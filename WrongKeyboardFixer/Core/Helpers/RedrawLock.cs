using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WrongKeyboardFixer.Core.Helpers;

internal static partial class RedrawLock
{
    private const int WmSetRedraw = 0x000B;

    // اضافه کردن EntryPoint = "SendMessageW" برای مشخص کردن نام دقیق در DLL
    [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    private static partial int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    public static void Suspend(Control c) =>
        SendMessage(c.Handle, WmSetRedraw, IntPtr.Zero, IntPtr.Zero);   // WM_SETREDRAW off

    public static void Resume(Control c)
    {
        SendMessage(c.Handle, WmSetRedraw, new IntPtr(1), IntPtr.Zero);
        c.Refresh();
    }
}

