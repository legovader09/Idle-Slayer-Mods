using IdleSlayerMods.Common.Config;
using MelonLoader;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace IdleSlayerMods.Common;

/// <inheritdoc />
public class Settings(string cfgName) : BaseConfig(cfgName)
{
    // Core Mod Settings
    public MelonPreferences_Entry<bool> DebugMode;
    public MelonPreferences_Entry<bool> EnableNewVersionRequiredSkip;
    public MelonPreferences_Entry<bool> ShowModVersionOnTitleScreen;
    
    // AntiSplashScreen Settings
    public MelonPreferences_Entry<bool> CreateBackups;
    public MelonPreferences_Entry<bool> PreventLogout;
    public MelonPreferences_Entry<bool> PreventClose;
    public MelonPreferences_Entry<bool> DebugSplashScreen;
    public MelonPreferences_Entry<string[]> ModStrings;
    
    // SilverRandomBoxFix Settings
    public MelonPreferences_Entry<bool> ApplySilverRandomBoxPatch;

    /// <inheritdoc />
    protected override void SetBindings()
    {
        DebugMode = Bind("IdleSlayerCore","Debug Mode", false, "Enable debug mode. Shows Logger.Debug logs in the console.");
        EnableNewVersionRequiredSkip = Bind("IdleSlayerCore", "Enable New Version Required Skip", true, "Skips the 'New Version Required' screen.");
        ShowModVersionOnTitleScreen = Bind("IdleSlayerCore","Show Mod Version On Title Screen", true, "Display the core mod version on the title screen.");
        ApplySilverRandomBoxPatch = Bind("SilverRandomBoxFix", "Enable Patching", false, "Enables patching of SilverRandomBoxManager to fix the issue with the silver random box button not showing up correctly.");
        CreateBackups = Bind("AntiSplashScreen", "Create Backups", true, "Creates backup saves when the game loads");
        PreventLogout = Bind("AntiSplashScreen", "Force Prevent Logout", false, "Forces to game not to log you out (causes side effects)");
        PreventClose = Bind("AntiSplashScreen", "Force Prevent Close", false, "Forces to game not to quit (causes side effects)");
        DebugSplashScreen = Bind("AntiSplashScreen", "Debug Splash Screen", false, "Shows debug information");
        ModStrings = Bind("AntiSplashScreen", "Mod Files/Directorys Blacklist", (string[])[
            // Default paths that are checked
            "BepInEx",
            "Mods",
            "MelonLoader",
            "dotnet",
            "doorstop_config.ini",
            // Custom entries
            "Melon Loader", // GetDirectories checks for "MelonLoader" and "Melon Loader" filenames
            // Libraries used for loading mods
            "version.dll",
            "winhttp.dll",
            "opengl32.dll",
            // BepInEx specific files
            ".doorstop_version",
            "changelog.txt",
        ], "Files and directories that the game checks for to prevent loading");
    }
}