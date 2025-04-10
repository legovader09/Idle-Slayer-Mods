using MelonLoader;

namespace IdleSlayerMods.Common.Extensions;

public static class LoggerExtension
{
    public static void Debug(this MelonLogger.Instance logger, string message)
    {
        if (!ModUtils.DebugMode) return;
        logger.MsgPastel(ConsoleColor.DarkGray, $"[DEBUG] {message}");
    }
}