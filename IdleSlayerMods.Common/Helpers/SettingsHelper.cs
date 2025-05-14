using MelonLoader;

namespace IdleSlayerMods.Common.Helpers;

public static class SettingsHelper
{
    public static void OnToggleChanged(bool state, MelonPreferences_Entry<bool> configEntry, Action<bool> onChanged)
    {
        configEntry.Value = state;
        onChanged?.Invoke(state);
    }
}