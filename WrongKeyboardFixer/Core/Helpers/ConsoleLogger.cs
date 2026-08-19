using System;

namespace WrongKeyboardFixer.Core.Helpers;

/// <summary>
///     Simple console logger implementation.
///     Logs to Debug output in debug builds, no-op in release.
/// </summary>
public class ConsoleLogger : ILogger
{
    /// <summary>Logs an error message.</summary>
    public void Error(string message)
    {
        System.Diagnostics.Debug.WriteLine($"[ERROR] {message}");
    }

    /// <summary>Logs an error with exception details.</summary>
    public void Error(string message, Exception exception)
    {
        System.Diagnostics.Debug.WriteLine($"[ERROR] {message}");
        System.Diagnostics.Debug.WriteLine($"[ERROR] Exception: {exception.GetType().Name}: {exception.Message}");
        System.Diagnostics.Debug.WriteLine($"[ERROR] StackTrace: {exception.StackTrace}");
    }

    /// <summary>Logs a warning message.</summary>
    public void Warning(string message)
    {
        System.Diagnostics.Debug.WriteLine($"[WARN] {message}");
    }

    /// <summary>Logs a warning with exception details.</summary>
    public void Warning(string message, Exception exception)
    {
        System.Diagnostics.Debug.WriteLine($"[WARN] {message}");
        System.Diagnostics.Debug.WriteLine($"[WARN] Exception: {exception.GetType().Name}: {exception.Message}");
    }

    /// <summary>Logs an informational message.</summary>
    public void Info(string message)
    {
        System.Diagnostics.Debug.WriteLine($"[INFO] {message}");
    }

    /// <summary>Logs a debug message (only in debug builds).</summary>
    public void Debug(string message)
    {
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"[DEBUG] {message}");
#endif
    }
}