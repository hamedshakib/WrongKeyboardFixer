using System.Diagnostics;

namespace WrongKeyboardFixer;

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
            throw new InvalidOperationException("Failed to set clipboard text", ex);
        }
    }

    public async Task<string> GetTextWithRetryAsync()
    {
        for (int attempt = 0; attempt < MaxRetryAttempts; attempt++)
        {
            await Task.Delay(RetryDelayMs);

            string text = GetText();
            if (!string.IsNullOrWhiteSpace(text))
                return text;
        }

        return string.Empty;
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