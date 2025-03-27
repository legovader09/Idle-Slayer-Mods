using IdleSlayerMods.Common;
using MelonLoader;
using MyPluginInfo = IdleConfig.MyPluginInfo;
using Plugin = IdleConfig.Plugin;

[assembly: MelonInfo(typeof(Plugin), MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION, MyPluginInfo.PLUGIN_AUTHOR)]
[assembly: MelonAdditionalDependencies("IdleSlayerMods.Common")]

namespace IdleConfig;

public class Plugin : MelonMod
{
    internal static ModHelper ModHelper;
    internal static readonly MelonLogger.Instance Logger = Melon<Plugin>.Logger;

    public override void OnInitializeMelon()
    {
        ModHelper.ModHelperMounted += helper => ModHelper = helper; 
        Logger.Msg($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName != "Game") return;
        ModUtils.RegisterComponent<IdleConfig>();
    }
}