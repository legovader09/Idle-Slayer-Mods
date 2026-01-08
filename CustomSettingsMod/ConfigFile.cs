using IdleSlayerMods.Common.Config;
using MelonLoader;
using UnityEngine;

namespace CustomSettingsMod;

internal sealed class ConfigFile(string configName) : BaseConfig(configName)
{
    internal MelonPreferences_Entry<KeyCode> ModMenuKey;
    internal MelonPreferences_Entry<bool> TestBool;
    internal MelonPreferences_Entry<KeyCode> TestKeyCode;

    protected override void SetBindings()
    {
        ModMenuKey = Bind("mod menu", KeyCode.F1, "");
        TestBool = Bind("test bool", true, "");
        TestKeyCode = Bind("test key code", KeyCode.V, "");
    }
}