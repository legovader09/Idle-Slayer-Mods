using IdleSlayerMods.Common.Config;
using MelonLoader;
using UnityEngine;

namespace AutoBoost;

internal sealed class Settings(string configName) : BaseConfig(configName)
{
    internal MelonPreferences_Entry<KeyCode> ToggleKey;
    internal MelonPreferences_Entry<KeyCode> ToggleKeyWindDash;
    internal MelonPreferences_Entry<bool> ShowPopup;
    internal MelonPreferences_Entry<bool> EnableWindDash;
    internal MelonPreferences_Entry<bool> ShowPopupWindDash;

    protected override void SetBindings()
    {
        EnableWindDash = Bind("EnableWindDash", false,
            "Enable wind dash boosting");
        ToggleKeyWindDash = Bind("ToggleKeyWindDash", KeyCode.N,
            "The key bind for toggling wind dash boosting");
        ShowPopupWindDash = Bind("ShowPopupWindDash", true,
            "Show a message popup to indicate whether wind dash has been toggled.");
        ToggleKey = Bind("ToggleKey", KeyCode.B,
            "The key bind for toggling auto boosting");
        ShowPopup = Bind("ShowPopup", true,
            "Show a message popup to indicate whether auto boost has been toggled.");
    }
}