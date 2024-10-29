using BepInEx.Configuration;
using IdleSlayerMods.Common.Config;

namespace RevealMimics;

public sealed class Settings(ConfigFile cfg) : BaseConfig(cfg)
{
    internal ConfigEntry<bool> ShouldRevealMultipliers;

    protected override void SetBindings()
    {
        ShouldRevealMultipliers = Bind("General", "ShouldRevealMultipliers", false,
            "Should reveal multipliers in chest hunt");
    }
}