using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WrongKeyboardFixer.Core.Model;

namespace WrongKeyboardFixer.Core.Services;

/// <summary>
/// Performs the "copy selected text → convert language → paste back" pipeline
/// using the clipboard, the keyboard simulator and the configured mappings.
/// </summary>
public sealed class TextConversionService
{
    private readonly ClipboardManager _clipboard;
    private readonly AppSettings _settings;

    public TextConversionService(ClipboardManager clipboard, AppSettings settings)
    {
        _clipboard = clipboard;
        _settings = settings;
    }

    public async Task ConvertSelectedTextAsync()
    {
        string previousClipboard = _clipboard.GetText();

        try
        {
            Debug.WriteLine("🔄 Starting text conversion...");

            KeyboardSimulator.SendCtrlC();
            await Task.Delay(300);

            string originalText = await _clipboard.GetTextWithRetryAsync();
            if (string.IsNullOrWhiteSpace(originalText))
            {
                _clipboard.RestoreText(previousClipboard);
                return;
            }

            bool toPersian = KeyboardConverter.ShouldConvertToPersian(originalText);

            string convertedText = toPersian
                ? KeyboardConverter.ConvertEnglishToPersian(originalText, _settings.EnglishToPersianMap)
                : KeyboardConverter.ConvertPersianToEnglish(originalText, _settings.PersianToEnglishMap);

            _clipboard.SetText(convertedText);
            await Task.Delay(200);
            KeyboardSimulator.SendCtrlV();
            await Task.Delay(200);

            _clipboard.RestoreText(previousClipboard);
            Debug.WriteLine("✅ Conversion completed successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Error: {ex.Message}");
            _clipboard.RestoreText(previousClipboard);
        }
    }
}
