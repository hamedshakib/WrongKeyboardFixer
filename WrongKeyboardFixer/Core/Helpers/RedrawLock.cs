using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WrongKeyboardFixer.Core.Helpers;

internal static class RedrawLock
{
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    public static void Suspend(Control c) => SendMessage(c.Handle, 0x000B, IntPtr.Zero, IntPtr.Zero);   // WM_SETREDRAW off
    public static void Resume(Control c) { SendMessage(c.Handle, 0x000B, new IntPtr(1), IntPtr.Zero); c.Refresh(); }
}
