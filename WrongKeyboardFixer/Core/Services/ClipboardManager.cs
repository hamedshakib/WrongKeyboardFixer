using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;

namespace WrongKeyboardFixer.Core.Services;

public class ClipboardManager
{
    private const int MaxRetryAttempts = 8;
    private const int RetryDelayMs = 150;

    public string GetText()
    {
        try
        {
            return Clipboard.GetText(TextDataFormat.UnicodeText) ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public void SetText(string text)
    {
        try
        {
            Clipboard.SetText(text);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(Localization.Get("ClipboardSetTextError"), ex);
        }
    }

    public async Task<string> GetTextWithRetryAsync()
    {
        // Read immediately first: the caller already waits after sending Ctrl+C,
        // so sleeping before the first attempt only adds needless latency.
        string text = GetText();
        for (int attempt = 0; string.IsNullOrWhiteSpace(text) && attempt < MaxRetryAttempts; attempt++)
        {
            await Task.Delay(RetryDelayMs);
            text = GetText();
        }

        return text;
    }

    public void RestoreText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            ClearClipboard();
            return;
        }

        try
        {
            Clipboard.SetText(text);
            Debug.WriteLine("✅ Clipboard restored");
        }
        catch
        {
            // Silently fail
        }
    }

    private void ClearClipboard()
    {
        try
        {
            Clipboard.Clear();
            var emptyData = new DataObject();
            emptyData.SetData(DataFormats.Text, string.Empty);
            Clipboard.SetDataObject(emptyData, true);
            Debug.WriteLine("🧹 Clipboard cleared");
        }
        catch
        {
            // Silently fail
        }
    }
}
