using IdleSlayerMods.Common.Config;
using MelonLoader;
using UnityEngine;

namespace GameSpeedChanger;

internal sealed class ConfigFile(string configName) : BaseConfig(configName)
{
    internal MelonPreferences_Entry<int> DefaultSpeed;
    internal MelonPreferences_Entry<bool> SaveSpeed;
    internal MelonPreferences_Entry<KeyCode> SpeedUpKey;
    internal MelonPreferences_Entry<KeyCode> SpeedDownKey;
    internal MelonPreferences_Entry<KeyCode> ResetKey;

    protected override void SetBindings()
    {
        DefaultSpeed = Bind("Default Speed", 1, "The game speed that is set when the game loads");
        SaveSpeed = Bind("Save Speed on Change", false, "Saves the most recent to Default Speed upon change");
        SpeedUpKey = Bind("Speed Up Key", KeyCode.F4, "The key to increase game speed");
        SpeedDownKey = Bind("Speed Down Key", KeyCode.F3, "The key to decrease game speed");
        ResetKey = Bind("Speed Reset Key", KeyCode.F2, "The key to reset game speed");
    }
}
