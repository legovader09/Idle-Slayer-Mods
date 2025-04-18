using MelonLoader;

namespace IdleSlayerMods.Common.Extensions;

public static class BaseConfigExtensions
{
    /// <summary>
    /// Saves a config entry to file.
    /// </summary>
    /// <param name="entry">The MelonPreferences entry object</param>
    /// <param name="showSaveMessage">Show the preferences saved console log</param>
    public static void SaveEntry<T>(this MelonPreferences_Entry<T> entry, bool showSaveMessage = false)
    {
        entry.Category.SaveToFile(showSaveMessage);
    }
}