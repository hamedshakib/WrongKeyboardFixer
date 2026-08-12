using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WrongKeyboardFixer.Core.Contracts;
using WrongKeyboardFixer.Core.Model;

namespace WrongKeyboardFixer.Core.Services;

/// <summary>
/// Performs the "copy selected text → convert language → paste back" pipeline
/// using the clipboard, the keyboard simulator and the configured mappings.
/// </summary>
public sealed class TextConversionService
{
    private readonly IClipboardService _clipboard;
    private readonly AppSettings _settings;
    private readonly ITextConverter _converter;
    private readonly IKeyboardSimulator _keyboardSimulator;

    public TextConversionService(
        IClipboardService clipboard,
        AppSettings settings,
        ITextConverter? converter = null,
        IKeyboardSimulator? keyboardSimulator = null)
    {
        _clipboard = clipboard ?? throw new ArgumentNullException(nameof(clipboard));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _converter = converter ?? new KeyboardConverter();
        _keyboardSimulator = keyboardSimulator ?? new KeyboardSimulator();
    }

    /// <summary>
    /// Converts the currently selected text by copying, converting, and pasting back.
    /// </summary>
    public async Task ConvertSelectedTextAsync()
    {
        string previousClipboard = _clipboard.GetText();

        try
        {
            Debug.WriteLine("🔄 Starting text conversion...");

            _keyboardSimulator.SendCtrlC();
            await Task.Delay(300);

            string originalText = await _clipboard.GetTextWithRetryAsync();
            if (string.IsNullOrWhiteSpace(originalText))
            {
                _clipboard.RestoreText(previousClipboard);
                return;
            }

            bool toPersian = _converter.ShouldConvertToPersian(originalText);

            string convertedText = toPersian
                ? _converter.ConvertEnglishToPersian(originalText, _settings.EnglishToPersianMap)
                : _converter.ConvertPersianToEnglish(originalText, _settings.PersianToEnglishMap);

            _clipboard.SetText(convertedText);
            await Task.Delay(200);
            _keyboardSimulator.SendCtrlV();
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
