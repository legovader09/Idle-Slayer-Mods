// ReSharper disable RedundantUsingDirective
using System.Reflection;
using System.Text;
using HarmonyLib;
using IdleSlayerMods.Common.Extensions;
using IdleSlayerMods.Common.Helpers;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.XrefScans;
using MelonLoader.Logging;
using MelonLoader.Pastel;
using MelonLoader.Utils;
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
    
    /// <summary>
    /// Creates a backup of the save files in the "Backup Saves" directory within the MelonLoader UserData directory.
    /// </summary>
    /// <remarks>
    /// Add time so we get unique copies
    /// </remarks>
    internal static void CreateGameBackup()
    {
        const string saveFileName = "savefile.sav";
        const string backupSaveFileName = "backup.sav";
        try
        {
            var savePath = Path.Combine(MelonEnvironment.UserDataDirectory, "Backup Saves");
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            /*
                 XOR({'option':'UTF8','string':'If you manage to get this string you are allowed to hack the game all you want'},'Standard',false)
                 JSON_Beautify('  ',false,true)
            */
            // FileBasedPrefs.WriteToSaveFile(UnityEngine.JsonUtility.ToJson(FileBasedPrefs.GetSaveFile(0)), var_rbx);
            // System.IO.StreamWriter
            var decryptedSavePath = Path.Combine(savePath, "decrypted_" + Application.version + "_" + saveFileName);
            if (File.Exists(decryptedSavePath))
            {
                File.Delete(decryptedSavePath);
            }
            var saveWriter = new StreamWriter(decryptedSavePath, false);
            saveWriter.WriteLine(JsonUtility.ToJson(FileBasedPrefs.GetSaveFile()));
            saveWriter.Close();

            File.Copy(FileBasedPrefs.GetSaveFilePath(), Path.Combine(savePath, Application.version + "_" + saveFileName), true);
            Plugin.Logger.Debug($"Backed up {saveFileName.Pastel(ColorARGB.GreenYellow)} to {savePath.Pastel(ColorARGB.GreenYellow)}");

            File.Copy(FileBasedPrefs.GetBackupSaveFilePath(), Path.Combine(savePath, Application.version + "_" + backupSaveFileName), true);
            Plugin.Logger.Debug($"Backed up {backupSaveFileName.Pastel(ColorARGB.GreenYellow)} to {savePath.Pastel(ColorARGB.GreenYellow)}");
        }
        catch (Exception ex)
        {
            Plugin.Logger.Error($"Failed to create backup: {ex.Message}");
        }
    }
    
#if DEBUG
    private static void PrintMethodCalls(MethodInfo method)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"\"{method.GetShortDescription().Pastel(ColorARGB.GreenYellow)}\" Called by:");
        //Plugin.Logger.Debug($"\"{Helper.GetMethodShortDescription(method).Pastel(ColorARGB.GreenYellow)}\" Calls:");
        method.GetMethodsCallers()
                    .ToList()
                    .ForEach(methodBase => sb.AppendLine($"\tCaller: \"{methodBase.GetShortDescription().Pastel(ColorARGB.GreenYellow)}\""));
        Plugin.Logger.Debug(sb.ToString());
    }
    
    // ReSharper disable once UnusedMember.Local
    internal static void PrintCalls()
    {
        PrintMethodCalls(AccessTools.Method(typeof(Application), nameof(Application.Quit)));
        PrintMethodCalls(AccessTools.Method(typeof(Authentication), nameof(Authentication.Logout)));
        PrintMethodCalls(AccessTools.Method(typeof(Il2CppSystem.IO.Directory), nameof(Il2CppSystem.IO.Directory.Exists)));
        PrintMethodCalls(AccessTools.Method(typeof(Il2CppSystem.IO.File), nameof(Il2CppSystem.IO.File.Exists)));

        //var playerInventoryAwakeXref = XrefScanner.XrefScan(AccessTools.Method(typeof(PlayerInventory), nameof(PlayerInventory.Awake)));
        //foreach (var instance in playerInventoryAwakeXref)
        //{
        //    if (instance.Type == XrefType.Global)
        //    {
        //        Plugin.Logger.Debug($"Found string: {instance.Pointer}");
        //    }
        //}
    }

    // ReSharper disable once UnusedMember.Local
    private static void DumpLoadGameSceneMethod()
    {
        var appQuitMethodInfo = AccessTools.Method(typeof(Application), nameof(Application.Quit));
        var appPathMethodInfo = AccessTools.DeclaredPropertyGetter(typeof(Application), nameof(Application.dataPath));
        var loadGameSceneMethodInfo = AccessTools.Method(typeof(SplashScreen), nameof(SplashScreen.LoadGameScene));
        var loadGameSceneMoveNextMethodInfo = AccessTools.EnumeratorMoveNext(loadGameSceneMethodInfo);

        if (appPathMethodInfo == null)
        {
            Plugin.Logger.Error("Failed to find UnityEngine.Application::dataPath method");
        }

        if (loadGameSceneMethodInfo == null || loadGameSceneMoveNextMethodInfo == null)
        {
            Plugin.Logger.Error("Failed to find LoadGameScene method or LoadGameScene enumerator");
            return;
        }

        Plugin.Logger.Warning($"UnityEngine.Application.Quit(Getter): {appQuitMethodInfo.FullDescription()}");
        Plugin.Logger.Warning($"UnityEngine.Application.dataPath: {appPathMethodInfo.FullDescription()}");
        Plugin.Logger.Warning($"LoadGameScene: {loadGameSceneMethodInfo.FullDescription()}");
        Plugin.Logger.Warning($"LoadGameScene EnumeratorMoveNext: {loadGameSceneMoveNextMethodInfo.FullDescription()}");

        var instances = Il2CppInterop.Common.XrefScans.XrefScanner.XrefScan(loadGameSceneMethodInfo);
        foreach (var instance in instances)
        {
            if (instance.Type == Il2CppInterop.Common.XrefScans.XrefType.Global)
            {
                var obj = instance.ReadAsObject();
                if (obj == null) continue;
                var objType = obj.GetIl2CppType();
                Plugin.Logger.Debug($"Object({instance.Pointer.ToString("X")}): {objType.FullName}, {obj.ToString()}");
            }
            else
            {
                var calledMethod = instance.TryResolve();
                Plugin.Logger.Debug($"Method({instance.Pointer.ToString("X")}): {calledMethod.Name}");
                switch (calledMethod.MetadataToken)
                {
                    case var _ when calledMethod.MetadataToken == loadGameSceneMethodInfo.MetadataToken:
                        Plugin.Logger.Warning($"Found SplashScreen.LoadGameScene method");
                        break;
                    case var _ when calledMethod.MetadataToken == loadGameSceneMoveNextMethodInfo.MetadataToken:
                        Plugin.Logger.Warning($"Found SplashScreen.LoadGameScene enumerator");
                        break;
                    case var _ when calledMethod.MetadataToken == appPathMethodInfo?.MetadataToken:
                        Plugin.Logger.Warning($"Found UnityEngine.Application::dataPath method");
                        break;
                    case var _ when calledMethod.MetadataToken == appQuitMethodInfo?.MetadataToken:
                        Plugin.Logger.Warning($"Found UnityEngine.Application::Quit method");
                        break;
                }
            }
        }
    }
#endif
}