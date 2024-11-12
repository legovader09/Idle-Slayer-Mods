using BepInEx.Configuration;
using IdleSlayerMods.Common.Config;

namespace RevealMimics;

internal sealed class Settings(ConfigFile cfg) : BaseConfig(cfg)
{
    internal ConfigEntry<bool> ShouldRevealMultipliers;
    internal ConfigEntry<bool> ShouldRevealDuplicator;

    protected override void SetBindings()
    {
        ShouldRevealMultipliers = Bind("General", "ShouldRevealMultipliers", false,
            "Should reveal multipliers in chest hunt");
        ShouldRevealDuplicator = Bind("General", "ShouldRevealDuplicator", false,
            "Should reveal duplicator item in chest hunt");
    }
}