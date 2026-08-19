using System;

namespace WrongKeyboardFixer.Core.Helpers;

/// <summary>
///     Simple logging interface for the application.
///     Provides methods for different log levels.
/// </summary>
public interface ILogger
{
    /// <summary>Logs an error message.</summary>
    void Error(string message);

    /// <summary>Logs an error with exception details.</summary>
    void Error(string message, Exception exception);

    /// <summary>Logs a warning message.</summary>
    void Warning(string message);

    /// <summary>Logs a warning with exception details.</summary>
    void Warning(string message, Exception exception);

    /// <summary>Logs an informational message.</summary>
    void Info(string message);

    /// <summary>Logs a debug message (only in debug builds).</summary>
    void Debug(string message);
}