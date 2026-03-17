using HarmonyLib;
using IdleSlayerMods.Common;
using IdleSlayerMods.Common.Data;
using IdleSlayerMods.Common.Extensions;
using Il2Cpp;
using MelonLoader;
using MelonLoader.Logging;
using MelonLoader.Pastel;

[assembly: MelonInfo(typeof(Plugin), MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION, MyPluginInfo.PLUGIN_AUTHOR)]
[assembly: MelonGame(MyPluginInfo.GAME_DEVELOPER, MyPluginInfo.GAME_NAME)]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]
[assembly: MelonIncompatibleAssemblies("AntiSpashScreen", "AntiSplashScreen", "SilverRandomBoxFix", "NoSyncGameLoad")]
[assembly: MelonPriority(100)]
[assembly: MelonColor(1, 1, 255, 100)]
[assembly: MelonAuthorColor(1, 1, 155, 70)]

namespace IdleSlayerMods.Common;

/// <inheritdoc />
public class Plugin : MelonMod
{
    internal static readonly Settings Settings = new(MyPluginInfo.PLUGIN_GUID);
    internal static readonly MelonLogger.Instance Logger = Melon<Plugin>.Logger;

    /// <inheritdoc />
    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        foreach (var method in HarmonyInstance.GetPatchedMethods())
        {
            Logger.Debug($"Patched {method.FullDescription().Pastel(ColorARGB.GreenYellow)}");
        }
        
        InitializeAntiSplashScreen();
    }

    /// <inheritdoc />
    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName == "Title Screen")
        {
            ModUtils.RegisterComponent<TitleChanger>(false);
        }
        
        if (sceneName != "Game") return;
        ModUtils.RegisterComponent<ModHelper>();
        InitializeSilverRandomBoxFix();
    }
    
#if DEBUG
    /// <inheritdoc />
    public override void OnLateInitializeMelon()
    {
        Logger.Debug($"---Dumping(LoadGameSceneMethod)---");
        //DumpLoadGameSceneMethod();
        Logger.Debug($"---Dumping(Calls)---");
        ModUtils.PrintCalls();
    }
#endif
    
    private static void InitializeAntiSplashScreen()
    {
        Logger.Msg($"---Started ({AntiSplashScreenPluginInfo.PLUGIN_GUID} v{AntiSplashScreenPluginInfo.PLUGIN_VERSION})---");
        if (!Settings.CreateBackups.Value) return;
        Logger.Msg($"---Creating Backup Saves---");
        ModUtils.CreateGameBackup();
        Logger.Msg($"---Backups Created---");
    }

    private static void InitializeSilverRandomBoxFix()
    {
        var lastTimeUsed = SilverRandomBoxManager.instance.lastTimeUsed;
        Logger.Msg($"\"Silver Random Box DateTime\" is set to \"{lastTimeUsed}\"({TimeManager.GetDateTime(lastTimeUsed).ToLocalTime()}) ");
        if (!(lastTimeUsed > TimeManager.lastWorkingTime)) return;
        
        var lastTime = TimeManager.lastWorkingTime - 30 * 60;
        SilverRandomBoxManager.instance.lastTimeUsed = lastTime;
        // ReSharper disable once SpecifyACultureInStringConversionExplicitly
        SaveManager.SetString("Silver Random Box DateTime", lastTime.ToString());
        Logger.Warning($"Silver Boxes were shadowbanned fixed DateTime to \"{lastTime}\"({TimeManager.GetDateTime(lastTime).ToLocalTime()})");
    }
}