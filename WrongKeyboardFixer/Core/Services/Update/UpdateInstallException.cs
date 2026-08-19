using System;

namespace WrongKeyboardFixer.Core.Services.Update;

/// <summary>
///     Thrown when an update cannot be downloaded, verified or installed.
///     Carries a fully localized message; the UI layer decides how to present it.
/// </summary>
internal sealed class UpdateInstallException : Exception
{
    public UpdateInstallException(string message, bool isWarning = false)
        : base(message)
    {
        IsWarning = isWarning;
    }

    /// <summary>True when the problem is informational (e.g. the downloaded build is not newer).</summary>
    public bool IsWarning { get; }
}