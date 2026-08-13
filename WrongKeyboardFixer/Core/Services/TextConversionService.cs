using System;
using System.Diagnostics;
using System.Threading;
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
    private readonly SemaphoreSlim _conversionGate = new(1, 1);

    // Timing constants (milliseconds)
    private const int ClipboardReadDelayMs = 300;
    private const int ClipboardWriteDelayMs = 200;

    public TextConversionService(ClipboardManager clipboard, AppSettings settings)
    {
        _clipboard = clipboard;
        _settings = settings;
    }

    /// <summary>
    /// Converts the currently selected text.
    /// Overlapping invocations (e.g. a second hotkey press while a conversion is
    /// still in flight) are ignored so two pipelines never touch the clipboard
    /// at the same time.
    /// </summary>
    public async Task ConvertSelectedTextAsync()
    {
        if (!await _conversionGate.WaitAsync(0))
            return;

        try
        {
            await ConvertCoreAsync();
        }
        finally
        {
            _conversionGate.Release();
        }
    }

    private async Task ConvertCoreAsync()
    {
        string previousClipboard = _clipboard.GetText();

        try
        {
            Debug.WriteLine("🔄 Starting text conversion...");

            KeyboardSimulator.SendCtrlC();
            await Task.Delay(ClipboardReadDelayMs);

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

            // Nothing changed: leave the selection untouched and give back the
            // clipboard instead of simulating a pointless Ctrl+V.
            if (convertedText == originalText)
            {
                _clipboard.RestoreText(previousClipboard);
                return;
            }

            _clipboard.SetText(convertedText);
            await Task.Delay(ClipboardWriteDelayMs);
            KeyboardSimulator.SendCtrlV();
            await Task.Delay(ClipboardWriteDelayMs);

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
