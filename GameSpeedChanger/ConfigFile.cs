using IdleSlayerMods.Common.Config;
using MelonLoader;
using UnityEngine;

namespace GameSpeedChanger;

internal sealed class ConfigFile(string configName) : BaseConfig(configName)
{
    internal MelonPreferences_Entry<int> DefaultSpeed;
    internal MelonPreferences_Entry<KeyCode> SpeedUpKey;
    internal MelonPreferences_Entry<KeyCode> SpeedDownKey;
    internal MelonPreferences_Entry<KeyCode> ResetKey;

    protected override void SetBindings()
    {
        DefaultSpeed = Bind("Game Speed Changer", 1,
            "The default game speed that is set when the game loads");
        SpeedUpKey = Bind("Speed Up Key", KeyCode.Equals, "The key to increase game speed");
        SpeedDownKey = Bind("Speed Down Key", KeyCode.Minus, "The key to decrease game speed");
        ResetKey = Bind("Speed Down Key", KeyCode.Alpha0, "The key to reset game speed");
    }
}
