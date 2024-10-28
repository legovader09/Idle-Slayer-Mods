using BepInEx.Configuration;
using IdleSlayerMods.Common.Config;
using UnityEngine;

namespace AutoBoost;

public class Settings : BaseConfig
{
    internal ConfigEntry<KeyCode> ToggleKey;
    
    public Settings(ConfigFile cfg) => SetBindings(cfg);

    protected sealed override void SetBindings(ConfigFile cfg)
    {
        base.SetBindings(cfg);
        ToggleKey = Bind("General", "ToggleKey", KeyCode.B,
            "The key bind for toggling auto boosting");
    }
}