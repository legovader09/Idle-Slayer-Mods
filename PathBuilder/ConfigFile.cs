using IdleSlayerMods.Common.Config;
using MelonLoader;

namespace PathBuilder;

internal sealed class ConfigFile(string configName) : BaseConfig(configName)
{
    internal MelonPreferences_Entry<bool> MySetting;

    protected override void SetBindings()
    {
        MySetting = Bind("My Setting", false,
            "Example setting of a boolean value");
    }
}
