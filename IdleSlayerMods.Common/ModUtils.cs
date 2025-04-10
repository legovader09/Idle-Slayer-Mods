using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IdleSlayerMods.Common;

public static class ModUtils
{
    public static bool DebugMode => Plugin.Settings.DebugMode.Value;
    
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static GameObject RegisterComponent<T>() where T : Component
    {
        ClassInjector.RegisterTypeInIl2Cpp<T>();
        var obj = new GameObject(typeof(T).Name);
        var component = obj.AddComponent<T>();
        
        if (component == null)
        {
            Melon<Plugin>.Logger.Error($"Failed to add component {typeof(T).Name}");
            Object.Destroy(obj);
            return null;
        }
        
        Object.DontDestroyOnLoad(obj);
        return obj;
    }
}