using BepInEx.Configuration;
using IdleSlayerMods.Common.Config;
using UnityEngine;

namespace AutoBoost;

internal sealed class Settings(ConfigFile cfg) : BaseConfig(cfg)
{
    internal ConfigEntry<KeyCode> ToggleKey;
    internal ConfigEntry<KeyCode> ToggleKeyWindDash;
    internal ConfigEntry<bool> ShowPopup;
    internal ConfigEntry<bool> EnableWindDash;
    internal ConfigEntry<bool> ShowPopupWindDash;

    protected override void SetBindings()
    {
        EnableWindDash = Bind("Wind Dash", "EnableWindDash", false,
            "Enable wind dash boosting");
        ToggleKeyWindDash = Bind("Wind Dash", "ToggleKeyWindDash", KeyCode.N,
            "The key bind for toggling wind dash boosting");
        ShowPopupWindDash = Bind("Wind Dash", "ShowPopupWindDash", true,
            "Show a message popup to indicate whether wind dash has been toggled.");
        ToggleKey = Bind("Auto Boost", "ToggleKey", KeyCode.B,
            "The key bind for toggling auto boosting");
        ShowPopup = Bind("Auto Boost", "ShowPopup", true,
            "Show a message popup to indicate whether auto boost has been toggled.");
    }
}