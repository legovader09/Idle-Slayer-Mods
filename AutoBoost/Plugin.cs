using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using IdleSlayerMods.Common;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace AutoBoost;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("IdleSlayerMods.Common")]
// ReSharper disable once ClassNeverInstantiated.Global
public class Plugin : BasePlugin
{
    internal new static ManualLogSource Log;
    internal static Settings Settings;
    internal static ModHelper ModHelperInstance;

    public override void Load()
    {
        Log = base.Log;
        Settings = new(Config);
        ClassInjector.RegisterTypeInIl2Cpp<AutoBoost>();

        ModHelper.ModHelperMounted += SetModHelperInstance;
        SceneManager.sceneLoaded += (UnityAction<Scene, LoadSceneMode>)OnSceneLoaded;
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private static void SetModHelperInstance(ModHelper instance) => ModHelperInstance = instance;

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Game") return;

        var boost = GameObject.Find("Boost");
        if (boost) boost.AddComponent<AutoBoost>();

        SceneManager.sceneLoaded -= (UnityAction<Scene, LoadSceneMode>)OnSceneLoaded;
    }
}