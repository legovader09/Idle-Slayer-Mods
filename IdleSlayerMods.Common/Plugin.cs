using IdleSlayerMods.Common;
using IdleSlayerMods.Common.Extensions;
using MelonLoader;

[assembly: MelonInfo(typeof(Plugin), MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION, MyPluginInfo.PLUGIN_AUTHOR)]
[assembly: MelonPriority(100)]

namespace IdleSlayerMods.Common;

public class Plugin : MelonMod
{
    internal static readonly Settings Settings = new(MyPluginInfo.PLUGIN_GUID);
    
    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        LoggerInstance.Debug("Debug message");
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName != "Game") return;
        ModUtils.RegisterComponent<ModHelper>();
    }
}