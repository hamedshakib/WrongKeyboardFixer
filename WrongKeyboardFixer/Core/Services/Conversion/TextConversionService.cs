using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WrongKeyboardFixer.Core.Models;

namespace WrongKeyboardFixer.Core.Services.Conversion;

/// <summary>
/// Performs the "copy selected text → convert language → paste back" pipeline
/// using the clipboard, the keyboard simulator and the configured mappings.
/// </summary>
public sealed class TextConversionService
{
    private readonly IClipboardService _clipboard;
    private readonly IKeyboardSimulator _keyboard;
    private readonly AppSettings _settings;
    private readonly IKeyboardConverter _converter;
    private readonly SemaphoreSlim _conversionGate = new(1, 1);

    // Timing constants (milliseconds)
    private const int ClipboardReadDelayMs = 300;
    private const int ClipboardWriteDelayMs = 200;

    public TextConversionService(
        IClipboardService clipboard,
        IKeyboardSimulator keyboard,
        AppSettings settings,
        IKeyboardConverter? converter = null)
    {
        _clipboard = clipboard;
        _keyboard = keyboard;
        _settings = settings;
        _converter = converter ?? new KeyboardConverter();
    }

    /// <summary>
    /// Converts the currently selected text.
    /// Overlapping invocations are ignored so two pipelines never touch the clipboard
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
            Debug.WriteLine("Starting text conversion...");

            _keyboard.SendCtrlC();
            await Task.Delay(ClipboardReadDelayMs);

            string originalText = await _clipboard.GetTextWithRetryAsync();
            if (string.IsNullOrWhiteSpace(originalText))
            {
                _clipboard.RestoreText(previousClipboard);
                return;
            }

            bool toPersian = _converter.ShouldConvertToPersian(originalText);

            string convertedText = toPersian
                ? _converter.ConvertEnglishToPersian(originalText, _settings.EnglishToPersianMap, _settings.WordCorrections)
                : _converter.ConvertPersianToEnglish(originalText, _settings.PersianToEnglishMap);

            if (convertedText == originalText)
            {
                _clipboard.RestoreText(previousClipboard);
                return;
            }

            _clipboard.SetText(convertedText);
            await Task.Delay(ClipboardWriteDelayMs);
            _keyboard.SendCtrlV();
            await Task.Delay(ClipboardWriteDelayMs);

            _clipboard.RestoreText(previousClipboard);
            Debug.WriteLine("Conversion completed successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
            _clipboard.RestoreText(previousClipboard);
        }
    }
}
