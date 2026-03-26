using System.Text.RegularExpressions;
using MelonLoader;

namespace IdleSlayerMods.Common.Extensions;

/// <summary>
/// Provides extension methods for working with configuration entries in the IdleSlayerMods.Common.Extensions namespace.
/// </summary>
// ReSharper disable once UnusedType.Global
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

    /// <summary>
    /// Corrects improperly formatted string arrays in the configuration file.
    /// This method processes the configuration file, identifies string arrays that are improperly formatted,
    /// and reformats them into the correct syntax for proper parsing and interpretation.
    /// </summary>
    public static void FixStringArrays(string configPath)
    {
        var raw = File.ReadAllText(configPath);

        raw = Regex.Replace(raw,
            """
            =\s*"\[\s*(.*?)\s*\]"
            """,
            match =>
            {
                var inner = match.Groups[1].Value;
                var items = inner.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => $"\"{s.Trim().Trim('"')}\"");
                return $"= [ {string.Join(", ", items)} ]";
            });

        File.WriteAllText(configPath, raw);
    }
}