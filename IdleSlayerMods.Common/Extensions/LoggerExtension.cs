using MelonLoader;

namespace IdleSlayerMods.Common.Extensions;

public static class LoggerExtension
{
    public static void Debug(this MelonLogger.Instance logger, string message)
    {
        #if DEBUG
        logger.MsgPastel(ConsoleColor.DarkGray, message);
        #endif
    }
}