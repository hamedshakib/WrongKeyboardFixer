using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WrongKeyboardFixer.UI.Components;

/// <summary>
///     تضمین می‌کند از هر فرم فقط یک نمونه باز باشد (singleton-per-form).
///     اگر نمونه‌ای از همان فرم هنوز باز است، به‌جای ساخت نمونهٔ جدید همان را جلو می‌آورد.
///     کاملاً سازگار با Native AOT — بدون هرگونه reflection (فقط <see cref="Type" /> و typeof).
/// </summary>
public static class FormSingleton
{
    private static readonly Dictionary<Type, Form> Open = new();

    /// <summary>
    ///     باز کردن فرم به‌صورت مودال؛ فقط در صورتی که نمونهٔ بازِ همنوعی وجود نداشته باشد.
    /// </summary>
    public static DialogResult ShowDialog<T>(Func<T> factory, IWin32Window? owner = null) where T : Form
    {
        if (TryGetOpen<T>() is { } existing)
        {
            // نمونهٔ باز وجود دارد → به‌جای باز کردن دوباره، همان را جلو می‌آوریم.
            if (existing.WindowState == FormWindowState.Minimized)
                existing.WindowState = FormWindowState.Normal;
            existing.Activate();
            return existing.DialogResult;
        }

        using var form = factory();
        Open[typeof(T)] = form;
        try
        {
            return owner != null ? form.ShowDialog(owner) : form.ShowDialog();
        }
        finally
        {
            Open.Remove(typeof(T));
        }
    }

    /// <summary>
    ///     باز کردن فرم به‌صورت غیرمودال؛ فقط در صورتی که نمونهٔ بازِ همنوعی وجود نداشته باشد.
    /// </summary>
    public static void Show<T>(Func<T> factory) where T : Form
    {
        if (TryGetOpen<T>() is { } existing)
        {
            if (existing.WindowState == FormWindowState.Minimized)
                existing.WindowState = FormWindowState.Normal;
            existing.Activate();
            return;
        }

        var form = factory();
        Open[typeof(T)] = form;
        form.FormClosed += (_, _) => Open.Remove(typeof(T));
        form.Show();
    }

    private static T? TryGetOpen<T>() where T : Form
    {
        if (Open.TryGetValue(typeof(T), out var form) && form != null && !form.IsDisposed)
            return (T)form;

        // اگر فرم بسته/نابود شده بود، ورودیِ کهنه را پاک می‌کنیم.
        Open.Remove(typeof(T));
        return null;
    }
}