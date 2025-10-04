using IdleSlayerMods.Common;
using MelonLoader;
using MyPluginInfo = PathBuilder.MyPluginInfo;
using Plugin = PathBuilder.Plugin;

[assembly: MelonInfo(typeof(Plugin), MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION, MyPluginInfo.PLUGIN_AUTHOR)]
[assembly: MelonAdditionalDependencies("IdleSlayerMods.Common")]

namespace PathBuilder;

public class Plugin : MelonMod
{
    internal static ConfigFile Config;
    internal static readonly MelonLogger.Instance Logger = Melon<Plugin>.Logger;
    
    public override void OnInitializeMelon()
    {
        Config = new(MyPluginInfo.PLUGIN_GUID);
        Logger.Msg($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName != "Game") return;
        ModUtils.RegisterComponent<PathBuilder>();
    }
}
