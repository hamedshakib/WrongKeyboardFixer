using System;
using System.Threading;
using System.Threading.Tasks;
using WrongKeyboardFixer.Core.Helpers;
using WrongKeyboardFixer.Core.Models;

namespace WrongKeyboardFixer.Core.Services.Conversion;

/// <summary>
///     Performs the "copy selected text → convert language → paste back" pipeline
///     using the clipboard, the keyboard simulator and the configured mappings.
///     Thread-safe through semaphore-based gate to prevent overlapping conversions.
/// </summary>
public sealed class TextConversionService : IDisposable
{
    private readonly IClipboardManager _clipboard;
    private readonly SemaphoreSlim _conversionGate = new(1, 1);
    private readonly IKeyboardConverter _converter;
    private readonly ILogger _logger;
    private readonly AppSettings _settings;

    public TextConversionService(
        ClipboardManager clipboard,
        AppSettings settings,
        IKeyboardConverter? converter = null,
        ILogger? logger = null)
    {
        _clipboard = clipboard;
        _settings = settings;
        _converter = converter ?? new KeyboardConverter();
        _logger = logger ?? new ConsoleLogger();
    }

    /// <summary>
    ///     Disposes the semaphore used for conversion gating.
    /// </summary>
    public void Dispose()
    {
        _conversionGate.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Converts the currently selected text.
    ///     Overlapping invocations (e.g. a second hotkey press while a conversion is
    ///     still in flight) are ignored so two pipelines never touch the clipboard
    ///     at the same time.
    /// </summary>
    public async Task ConvertSelectedTextAsync()
    {
        if (!await _conversionGate.WaitAsync(0))
        {
            _logger.Debug("Conversion already in progress, skipping concurrent invocation");
            return;
        }

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
        _logger.Info("🔄 Starting text conversion...");

        try
        {
            KeyboardSimulator.SendCtrlC();
            await Task.Delay(AppConfiguration.Conversion.ClipboardReadDelayMs);

            string originalText = await _clipboard.GetTextWithRetryAsync();
            if (string.IsNullOrWhiteSpace(originalText))
            {
                _logger.Debug("No text retrieved from clipboard, restoring previous content");
                _clipboard.RestoreText(previousClipboard);
                return;
            }

            bool toPersian = _converter.ShouldConvertToPersian(originalText);
            _logger.Debug($"Text detection result: converting to {(toPersian ? "Persian" : "English")}");

            string convertedText = toPersian
                ? _converter.ConvertEnglishToPersian(originalText, _settings.EnglishToPersianMap, _settings.WordCorrections)
                : _converter.ConvertPersianToEnglish(originalText, _settings.PersianToEnglishMap);

            // Nothing changed: leave the selection untouched and give back the
            // clipboard instead of simulating a pointless Ctrl+V.
            if (convertedText == originalText)
            {
                _logger.Info("Text unchanged after conversion, skipping paste operation");
                _clipboard.RestoreText(previousClipboard);
                return;
            }

            _clipboard.SetText(convertedText);
            await Task.Delay(AppConfiguration.Conversion.ClipboardWriteDelayMs);
            KeyboardSimulator.SendCtrlV();
            await Task.Delay(AppConfiguration.Conversion.ClipboardWriteDelayMs);

            _clipboard.RestoreText(previousClipboard);
            _logger.Info("✅ Conversion completed successfully");
        }
        catch (Exception ex)
        {
            _logger.Error($"❌ Error during conversion: {ex.Message}", ex);
            _clipboard.RestoreText(previousClipboard);
        }
    }
}