using MelonLoader;

namespace IdleSlayerMods.Common.Helpers;

/// <summary>
/// Provides helper methods for managing application settings and triggers related callbacks
/// when configuration settings are modified.
/// </summary>
public static class SettingsHelper
{
    /// <summary>
    /// Updates the state of a configuration entry and invokes a callback when the toggle value changes.
    /// </summary>
    /// <param name="state">The new state of the toggle.</param>
    /// <param name="configEntry">The configuration entry to be updated.</param>
    /// <param name="onChanged">The callback action to be invoked with the new state.</param>
    public static void OnToggleChanged(bool state, MelonPreferences_Entry<bool> configEntry, Action<bool> onChanged)
    {
        configEntry.Value = state;
        onChanged?.Invoke(state);
    }
}