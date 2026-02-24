using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IdleSlayerMods.Common;

/// <summary>
/// Useful functions for modding.
/// </summary>
public static class ModUtils
{
    /// <summary>
    /// Enable or disable debug mode, which determines whether Logger.Debug() show in the console. 
    /// </summary>
    public static bool DebugMode => Plugin.Settings.DebugMode.Value;
    
    // ReSharper disable once UnusedMethodReturnValue.Global
    /// <summary>
    /// Registers the unity component to the scene
    /// </summary>
    /// <typeparam name="T">GameObject</typeparam>
    /// <returns>The registered GameObject</returns>
    public static GameObject RegisterComponent<T>() where T : Component
    {
        ClassInjector.RegisterTypeInIl2Cpp<T>();
        var obj = new GameObject(typeof(T).Name);
        var component = obj.AddComponent<T>();
        
        if (component == null)
        {
            Plugin.Logger.Error($"Failed to add component {typeof(T).Name}");
            Object.Destroy(obj);
            return null;
        }
        
        Object.DontDestroyOnLoad(obj);
        return obj;
    }
    
    // ReSharper disable once UnusedMethodReturnValue.Global
    /// <summary>
    /// Registers the unity component to the scene
    /// </summary>
    /// <param name="dontDestroy">Should the GameObject be destroyed when changing scenes?</param>
    /// <typeparam name="T">GameObject</typeparam>
    /// <returns>The registered GameObject</returns>
    public static GameObject RegisterComponent<T>(bool dontDestroy) where T : Component
    {
        ClassInjector.RegisterTypeInIl2Cpp<T>();
        var obj = new GameObject(typeof(T).Name);
        var component = obj.AddComponent<T>();
        
        if (component == null)
        {
            Plugin.Logger.Error($"Failed to add component {typeof(T).Name}");
            Object.Destroy(obj);
            return null;
        }

        if (dontDestroy)
        {
            Object.DontDestroyOnLoad(obj);
        }
        return obj;
    }
}