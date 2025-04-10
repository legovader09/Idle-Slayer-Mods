using IdleSlayerMods.Common.Config;
using MelonLoader;

namespace IdleSlayerMods.Common;

public class Settings(string cfgName) : BaseConfig(cfgName)
{
    public MelonPreferences_Entry<bool> DebugMode;
    
    protected override void SetBindings()
    {
        DebugMode = Bind("IdleSlayerCore","Debug Mode", false, "Enable debug mode");
    }
}