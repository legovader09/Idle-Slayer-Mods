using MelonLoader;

namespace IdleSlayerMods.Common.Extensions;

// ReSharper disable once UnusedType.Global
public static class LoggerExtension
{
    public static void Debug(this MelonLogger.Instance logger, string message)
    {
        if (!ModUtils.DebugMode) return;
        logger.MsgPastel(ConsoleColor.DarkGray, $"[DEBUG] {message}");
    }
}