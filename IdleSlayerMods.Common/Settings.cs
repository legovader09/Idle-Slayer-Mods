using IdleSlayerMods.Common.Config;
using MelonLoader;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace IdleSlayerMods.Common;

/// <inheritdoc />
public class Settings(string cfgName) : BaseConfig(cfgName)
{
    public MelonPreferences_Entry<bool> DebugMode;
    public MelonPreferences_Entry<bool> EnableNewVersionRequiredSkip;
    public MelonPreferences_Entry<bool> ShowModVersionOnTitleScreen;

    /// <inheritdoc />
    protected override void SetBindings()
    {
        DebugMode = Bind("IdleSlayerCore","Debug Mode", false, "Enable debug mode. Shows Logger.Debug logs in the console.");
        EnableNewVersionRequiredSkip = Bind("IdleSlayerCore", "Enable New Version Required Skip", true, "Skips the 'New Version Required' screen.");
        ShowModVersionOnTitleScreen = Bind("IdleSlayerCore","Show Mod Version On Title Screen", true, "Display the core mod version on the title screen.");
    }
}