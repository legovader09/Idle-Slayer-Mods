using IdleSlayerMods.Common;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using MyPluginInfo = AutoBoost.MyPluginInfo;
using Plugin = AutoBoost.Plugin;

[assembly: MelonInfo(typeof(Plugin), MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION, MyPluginInfo.PLUGIN_AUTHOR)]
[assembly: MelonAdditionalDependencies("IdleSlayerMods.Common")]

namespace AutoBoost;

public class Plugin : MelonMod
{
    internal static Settings Settings;
    internal static ModHelper ModHelperInstance;
    
    public override void OnInitializeMelon()
    {
        ClassInjector.RegisterTypeInIl2Cpp<ModHelper>();
        Settings = new(MyPluginInfo.PLUGIN_GUID);
        LoggerInstance.Msg($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        ModHelper.ModHelperMounted += SetModHelperInstance;
    }

    private static void SetModHelperInstance(ModHelper instance) => ModHelperInstance = instance;
    
    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName != "Game") return;
        ModUtils.RegisterComponent<AutoBoost>();
    }
}