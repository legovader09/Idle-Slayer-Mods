using IdleSlayerMods.Common.Config;
using MelonLoader;

namespace RestoreLastMap;

internal sealed class ConfigFile(string configName) : BaseConfig(configName)
{
    internal MelonPreferences_Entry<string> LastMap;
    internal MelonPreferences_Entry<bool> InstantTransfer;

    protected override void SetBindings()
    {
        LastMap = Bind("LastMap", "",
            "The last map name the player was on before exiting the game. (do not modify)");
        InstantTransfer = Bind("InstantTransfer", false,
            "Travel to the last map instantly instead of spawning the portal in. (recommended off for a better experience)");
    }
}
