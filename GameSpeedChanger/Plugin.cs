using IdleSlayerMods.Common;
using MelonLoader;
using MyPluginInfo = GameSpeedChanger.MyPluginInfo;
using Plugin = GameSpeedChanger.Plugin;

[assembly: MelonInfo(typeof(Plugin), MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION, MyPluginInfo.PLUGIN_AUTHOR)]
[assembly: MelonAdditionalDependencies("IdleSlayerMods.Common")]

namespace GameSpeedChanger;

public class Plugin : MelonMod
{
    internal static ConfigFile Config;
    internal static readonly MelonLogger.Instance Logger = Melon<Plugin>.Logger;
    internal static ModHelper ModHelperInstance;
    
    public override void OnInitializeMelon()
    {
        ModHelper.ModHelperMounted += SetModHelperInstance;
        Config = new(MyPluginInfo.PLUGIN_GUID);
        Logger.Msg($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
    
    private static void SetModHelperInstance(ModHelper instance) => ModHelperInstance = instance;

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName != "Game") return;
        ModUtils.RegisterComponent<GameSpeedChanger>();
    }
}
