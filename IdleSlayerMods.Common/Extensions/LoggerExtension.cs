using MelonLoader;
using MelonLoader.Logging;

namespace IdleSlayerMods.Common.Extensions;

// ReSharper disable once UnusedType.Global
/// <summary>
/// Provides extension methods for logging functionality in conjunction with the MelonLogger system.
/// </summary>
public static class LoggerExtension
{
    /// <summary>
    /// Outputs a debug message to the log if debug mode is enabled in the core mod config file.
    /// </summary>
    /// <param name="logger">The instance of the MelonLogger used for logging.</param>
    /// <param name="message">The message to log as debug information.</param>
    public static void Debug(this MelonLogger.Instance logger, string message)
    {
        if (!ModUtils.DebugMode) return;
        logger.MsgPastel(ConsoleColor.DarkGray, $"[DEBUG] {message}");
    }

    /// <summary>
    /// Outputs a debug-only error message to the log if debug mode is enabled in the core mod config file.
    /// </summary>
    /// <param name="logger">The instance of the MelonLogger used for logging.</param>
    /// <param name="color">The color of the console log text</param>
    /// <param name="message">The message to log as error information.</param>
    public static void Debug(this MelonLogger.Instance logger, ColorARGB color, string message)
    {
        if (!ModUtils.DebugMode) return;
        logger.MsgPastel(color, $"[ERROR] {message}");
    }
}