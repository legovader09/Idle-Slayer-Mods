using UnityEngine;
using UnityEngine.SceneManagement;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using UnityEngine.Events;

namespace RevealMimics;

[BepInPlugin(PluginData.PLUGIN_GUID, PluginData.PLUGIN_NAME, PluginData.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    internal new static ManualLogSource Log;

    public override void Load()
    {
        Log = base.Log;
        ClassInjector.RegisterTypeInIl2Cpp<MimicRevealer>();
        
        SceneManager.sceneLoaded += (UnityAction<Scene, LoadSceneMode>)OnSceneLoaded;
        Log.LogInfo($"Plugin {PluginData.PLUGIN_GUID} is loaded!");
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Game") return;
        var chestHunt = GameObject.Find("Chest Hunt");
        if (chestHunt) chestHunt.AddComponent<MimicRevealer>();
        SceneManager.sceneLoaded -= (UnityAction<Scene, LoadSceneMode>)OnSceneLoaded;
    }
}