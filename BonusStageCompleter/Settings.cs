using IdleSlayerMods.Common.Config;
using MelonLoader;
using UnityEngine;

namespace BonusStageCompleter;

internal sealed class Settings(string configName) : BaseConfig(configName)
{
    internal MelonPreferences_Entry<KeyCode> ToggleKey;
    internal MelonPreferences_Entry<bool> EnableNotSkipAtSpiritBoost;
    internal MelonPreferences_Entry<bool> ShowPopUpNotSkipAtSpiritBoost;

    protected override void SetBindings()
    {
        EnableNotSkipAtSpiritBoost = Bind("Not Skip At Spirit Boost", "EnableNotSkipAtSpiritBoost", false,
            "Enable not skip at spirit boost appeared bonus stage");
        ToggleKey = Bind("Not Skip At Spirit Boost", "ToggleKey", KeyCode.J,
            "The key bind for toggling 'not skip at spirit boost appeared boosting'");
        ShowPopUpNotSkipAtSpiritBoost = Bind("Not Skip At Spirit Boost", "ShowPopUpNotSkipAtSpiritBoost", true,
            "Show a message popup to indicate whether 'not skip at spirit boost has been toggled'.");
    }
}