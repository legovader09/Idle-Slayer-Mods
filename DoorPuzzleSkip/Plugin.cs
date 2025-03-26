using IdleSlayerMods.Common;
using MelonLoader;
using MyPluginInfo = DoorPuzzleSkip.MyPluginInfo;
using Plugin = DoorPuzzleSkip.Plugin;

[assembly: MelonInfo(typeof(Plugin), MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION, MyPluginInfo.PLUGIN_AUTHOR)]
[assembly: MelonAdditionalDependencies("IdleSlayerMods.Common")]

namespace DoorPuzzleSkip;

public class Plugin : MelonMod
{
    internal static readonly MelonLogger.Instance Logger = Melon<Plugin>.Logger;
    
    public override void OnInitializeMelon()
    {
        Logger.Msg($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName != "Game") return;
        ModUtils.RegisterComponent<DoorPuzzleSkip>();
    }
}