using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using UnityEngine;
using MyPluginInfo = RevealMimics.MyPluginInfo;
using Plugin = RevealMimics.Plugin;

[assembly: MelonInfo(typeof(Plugin), MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION, MyPluginInfo.PLUGIN_AUTHOR)]
[assembly: MelonAdditionalDependencies("IdleSlayerMods.Common")]

namespace RevealMimics;

public class Plugin : MelonMod
{
    internal static Settings Settings;

    public override void OnInitializeMelon()
    {
        Settings = new(MyPluginInfo.PLUGIN_GUID);
        ClassInjector.RegisterTypeInIl2Cpp<ChestRevealer>();
        LoggerInstance.Msg($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName != "Game") return;
        var chestHunt = GameObject.Find("Chest Hunt");
        if (chestHunt) chestHunt.AddComponent<ChestRevealer>();
    }
}