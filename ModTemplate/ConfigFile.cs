using IdleSlayerMods.Common.Config;
using MelonLoader;

namespace ModTemplate;

internal sealed class ConfigFile(string configName) : BaseConfig(configName)
{
    internal MelonPreferences_Entry<bool> MySetting;

    protected override void SetBindings()
    {
        MySetting = Bind("$safeprojectname", "My Setting", false,
            "Example setting of a boolean value");
    }
}
