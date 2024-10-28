using BepInEx.Configuration;
using IdleSlayerMods.Common.Config;

namespace RevealMimics;

public class Settings : BaseConfig
{
    internal ConfigEntry<bool> ShouldRevealMultipliers;
    
    public Settings(ConfigFile cfg) => SetBindings(cfg);

    protected sealed override void SetBindings(ConfigFile cfg)
    {
        base.SetBindings(cfg);
        ShouldRevealMultipliers = Bind("General", "ShouldRevealMultipliers", false,
            "Should reveal multipliers in chest hunt");
    }
}