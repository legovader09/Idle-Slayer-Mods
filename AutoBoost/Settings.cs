using BepInEx.Configuration;
using IdleSlayerMods.Common.Config;
using UnityEngine;

namespace AutoBoost;

public sealed class Settings(ConfigFile cfg) : BaseConfig(cfg)
{
    internal ConfigEntry<KeyCode> ToggleKey;
    internal ConfigEntry<bool> ShowPopup;

    protected override void SetBindings()
    {
        ToggleKey = Bind("General", "ToggleKey", KeyCode.B,
            "The key bind for toggling auto boosting");
        ShowPopup = Bind("General", "ShowPopup", true,
            "Show a message popup to indicate whether auto boost has been toggled.");
    }
}