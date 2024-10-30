using UnityEngine;
using UnityEngine.SceneManagement;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using JetBrains.Annotations;
using UnityEngine.Events;

namespace RevealMimics;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("IdleSlayerMods.Common")]
// ReSharper disable once ClassNeverInstantiated.Global
public class Plugin : BasePlugin
{
    internal new static ManualLogSource Log;
    internal static Settings Settings;

    public override void Load()
    {
        Log = base.Log;
        Settings = new(Config);
        ClassInjector.RegisterTypeInIl2Cpp<MimicRevealer>();
        
        SceneManager.sceneLoaded += (UnityAction<Scene, LoadSceneMode>)OnSceneLoaded;
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Game") return;
        var chestHunt = GameObject.Find("Chest Hunt");
        if (chestHunt) chestHunt.AddComponent<MimicRevealer>();
        SceneManager.sceneLoaded -= (UnityAction<Scene, LoadSceneMode>)OnSceneLoaded;
    }
}