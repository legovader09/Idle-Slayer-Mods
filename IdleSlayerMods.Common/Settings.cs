using IdleSlayerMods.Common.Config;
using MelonLoader;

namespace IdleSlayerMods.Common;

public class Settings(string cfgName) : BaseConfig(cfgName)
{
    public MelonPreferences_Entry<bool> DebugMode;
    public MelonPreferences_Entry<bool> EnableNewVersionRequiredSkip { get; set; }
    
    protected override void SetBindings()
    {
        DebugMode = Bind("IdleSlayerCore","Debug Mode", false, "Enable debug mode");
        EnableNewVersionRequiredSkip = Bind("IdleSlayerCore", "Enable New Version Required Skip", true, "Enable New Version Required Skip");
    }
}