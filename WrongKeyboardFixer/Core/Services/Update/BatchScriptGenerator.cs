using System.IO;

namespace WrongKeyboardFixer.Core.Services.Update;

/// <summary>
/// Generates the batch script that waits for the running process to exit,
/// replaces the executable with retry logic, verifies the copy and restarts
/// the application.
/// </summary>
internal static class BatchScriptGenerator
{
    /// <summary>
    /// Builds the full batch script text for replacing the running executable.
    /// </summary>
    public static string Generate(int processId, string newExePath, string currentExePath, string tempDir)
    {
        string logFile = Path.Combine(tempDir, "update.log");
        return $@"@echo off
chcp 65001 >nul
setlocal EnableDelayedExpansion

set ""LOG={logFile}""
echo [%date% %time%] Update script started > ""!LOG!""

:: Wait for the current application to exit (max 30 seconds)
set /a waitCount=0
:waitloop
tasklist /fi ""PID eq {processId}"" 2>nul | find ""{processId}"" >nul
if !errorlevel! equ 0 (
    set /a waitCount+=1
    if !waitCount! geq 30 (
        echo [%date% %time%] ERROR: Timed out waiting for process {processId} to exit >> ""!LOG!""
        goto :error
    )
    ping 127.0.0.1 -n 2 >nul
    goto waitloop
)
echo [%date% %time%] Process {processId} exited. Proceeding with update... >> ""!LOG!""

:: Copy the new executable over the old one with retry (max 10 attempts)
set /a copyAttempt=0
:copyloop
set /a copyAttempt+=1
copy /y ""{newExePath}"" ""{currentExePath}"" >nul 2>&1
if !errorlevel! equ 0 (
    echo [%date% %time%] Copy succeeded on attempt !copyAttempt! >> ""!LOG!""
    goto :copyok
)
if !copyAttempt! geq 10 (
    echo [%date% %time%] ERROR: Copy failed after 10 attempts >> ""!LOG!""
    goto :error
)
echo [%date% %time%] Copy attempt !copyAttempt! failed, retrying... >> ""!LOG!""
ping 127.0.0.1 -n 2 >nul
goto copyloop

:copyok
:: Verify the copy actually worked by checking file size
for %%A in (""{newExePath}"") do set ""newSize=%%~zA""
for %%A in (""{currentExePath}"") do set ""curSize=%%~zA""
if not ""!newSize!""==""!curSize!"" (
    echo [%date% %time%] ERROR: File size mismatch after copy (new=!newSize!, cur=!curSize!) >> ""!LOG!""
    goto :error
)
echo [%date% %time%] File size verified: !curSize! bytes >> ""!LOG!""

:: Start the updated application
echo [%date% %time%] Starting updated application... >> ""!LOG!""
start """" ""{currentExePath}""

:: Wait a moment for the app to start
ping 127.0.0.1 -n 3 >nul

:: Clean up temp directory
rd /s /q ""{tempDir}"" 2>nul

:: Delete this batch file
(goto) 2>nul & del ""%~f0""
exit /b 0

:error
echo [%date% %time%] Update failed. See log for details. >> ""!LOG!""
:: Show error message to user
msg * ""Update failed. Please check the log file: {logFile}"" >nul 2>&1
:: Clean up temp directory
rd /s /q ""{tempDir}"" 2>nul
:: Delete this batch file
(goto) 2>nul & del ""%~f0""
exit /b 1
";
    }
}
