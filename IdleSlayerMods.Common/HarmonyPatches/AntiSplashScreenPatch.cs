using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using HarmonyLib;
using IdleSlayerMods.Common.Extensions;
using IdleSlayerMods.Common.Helpers;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader.Logging;
using MelonLoader.Pastel;

namespace IdleSlayerMods.Common.HarmonyPatches;

/// <summary>
/// A Harmony patch class used to modify the behavior of the splash screen functionality in the game.
/// This patch prevents the splash screen from doing mod-related checks by intercepting and altering the relevant logic.
/// </summary>
[HarmonyPatch]
public class AntiSplashScreenPatch
{
    [HarmonyTargetMethod]
    // ReSharper disable once UnusedMember.Local
    private static IEnumerable<MethodBase> TargetMethods()
    {
        // if possible use nameof() or SymbolExtensions.GetMethodInfo() here
        yield return AntiSplashScreenHelper.ProcessMethod(typeof(SplashScreen), nameof(SplashScreen.Start));
        yield return AntiSplashScreenHelper.ProcessMethod(typeof(SplashScreen), nameof(SplashScreen.Update));
        yield return AntiSplashScreenHelper.ProcessMethod(typeof(SplashScreen), "Crush");
        yield return AntiSplashScreenHelper.ProcessMethod(typeof(SplashScreen), nameof(SplashScreen.LoadGameScene));
        yield return AntiSplashScreenHelper.ProcessMethod(typeof(SplashScreen), nameof(SplashScreen.LoadGameScene), methodType: MethodType.Enumerator);

        var delegateLoadGameSceneType = AccessTools.FirstInner(typeof(SplashScreen), t => t.Name.Contains("c")); // Not sure where c is generated from as it's a delegate
        if (delegateLoadGameSceneType == null) yield break;
        var delegateLoadGameSceneMoveNextMethodInfo = AccessTools.FirstMethod(delegateLoadGameSceneType,
            method =>
            {
                if (method.GetParameters().Any(info => info.ParameterType == typeof(string) && info.Name == "path"))
                {
                    return method.ReturnType == typeof(bool);
                }

                return false;
            });

        if (delegateLoadGameSceneMoveNextMethodInfo != null)
        {
            Plugin.Logger.Debug($"Found {delegateLoadGameSceneMoveNextMethodInfo.FullDescription().Pastel(ColorARGB.GreenYellow)}");
            yield return delegateLoadGameSceneMoveNextMethodInfo;
        }
        else
        {
            Plugin.Logger.Error("Failed to find SplashScreen.LoadGameScene::MoveNext delegate");
        }
    }
    
    [HarmonyPrefix]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static void SplashScreenPrefix()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {

    }
}

/// <summary>
/// A Harmony patch class designed to modify the behavior of player inventory functionality within the game.
/// The patch intervenes in the game's inventory management logic to apply custom modifications or enhancements.
/// </summary>
[HarmonyPatch]
public class PlayerInventoryPatch
{
    // ReSharper disable once UnusedMember.Local
    [HarmonyTargetMethod]
    static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AntiSplashScreenHelper.ProcessMethod(typeof(PlayerInventory), nameof(PlayerInventory.Start));
        yield return AntiSplashScreenHelper.ProcessMethod(typeof(PlayerInventory), nameof(PlayerInventory.LoadPrefs));
        yield return AntiSplashScreenHelper.ProcessMethod(typeof(PlayerInventory), nameof(PlayerInventory.Awake));

        //Type delegatePlayerInventoryAwakeType = AccessTools.FirstInner(typeof(PlayerInventory), t => t.Name.Contains("Awake"));
        //if (delegatePlayerInventoryAwakeType != null)
        //{
        //    System.Reflection.MethodInfo delegatePlayerInventoryAwakeMethodInfo = AccessTools.FirstMethod(delegatePlayerInventoryAwakeType,
        //        method => {
        //            foreach (ParameterInfo info in method.GetParameters())
        //            {
        //                if (info.ParameterType == typeof(string) && info.Name == "path")
        //                    return method.ReturnType == typeof(bool);
        //            }
        //            return false;
        //        });
        //    Plugin.Logger.Debug($"Found {delegatePlayerInventoryAwakeMethodInfo.FullDescription()}");
        //    if (delegatePlayerInventoryAwakeMethodInfo != null)
        //        yield return delegatePlayerInventoryAwakeMethodInfo;
        //}
    }
    [HarmonyPrefix]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static void PlayerInventoryPrefix()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {

    }
}

/// <summary>
/// A Harmony patch class intended to modify and intercept the behavior of various methods involving method callers in the game.
/// This class allows custom logic to hook into or replace existing functionality, enabling deeper customization or enhancements.
/// </summary>
[HarmonyPatch]
public class CallerPatches
{
    // ReSharper disable once UnusedMember.Local
    [HarmonyTargetMethod]
    private static IEnumerable<MethodBase> TargetMethods()
    {
        var appQuitMethodBase = AccessTools.Method(typeof(UnityEngine.Application), nameof(UnityEngine.Application.Quit));
        var authLogoutMethod = AccessTools.Method(typeof(Authentication), nameof(Authentication.Logout));
        foreach (var method in appQuitMethodBase.GetMethodsCallers())
        {
            Plugin.Logger.Debug($"Found Caller {method.FullDescription().Pastel(ColorARGB.GreenYellow)}->{appQuitMethodBase.GetShortDescription().Pastel(ColorARGB.GreenYellow)}");
            //Plugin.Logger.DebugPastel(System.Drawing.Color.Orange, $"Found Caller of Application.Quit: {method.FullDescription().Pastel(ColorARGB.GreenYellow)}");
            yield return method;
        }

        foreach (var method in authLogoutMethod.GetMethodsCallers())
        {
            Plugin.Logger.Debug($"Found Caller {method.FullDescription().Pastel(ColorARGB.GreenYellow)}->{authLogoutMethod.GetShortDescription().Pastel(ColorARGB.GreenYellow)}");
            yield return method;
        }
    }
    [HarmonyPrefix]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static void HookPrefix()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {

    }
}

/// <summary>
/// A Harmony patch class designed to bypass or prevent checks for the presence of mod-related directories or files.
/// This patch intercepts system-level file and directory operations, including existence checks, directory enumeration,
/// and application shutdown requests, to prevent actions that may restrict modding functionality.
/// </summary>
[HarmonyPatch]
public class ModCheckPatch
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private enum ModCheckType
    {
        None,
        DirectoryExists,
        FileExists,
        InternalEnumeratePaths,
        GetFileName,
        Quit,
        Logout
    }
    private static string GetFancyCallstack(System.Diagnostics.StackFrame[] frames, bool formatManaged = true)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Callstack:");
        foreach (var frame in frames)
        {
            var method = frame.GetMethod();
            var methodName = method?.Name ?? "Unknown Method";
            var declTypeName = method?.DeclaringType?.Name;
            if (methodName.Equals("il2cpp_runtime_invoke"))
            {
                sb.AppendLine($"\t[UNKNOWN Il2CPP CALL]");
                continue;
            }
            if (formatManaged && methodName.Contains("(il2cpp -> managed) "))
            {
                methodName = methodName.Replace("(il2cpp -> managed) ", string.Empty);
            }
            sb.AppendLine($"\tat {((declTypeName != null) ? declTypeName + "." : string.Empty)}{methodName}");
        }
        return sb.ToString();
    }
    private static bool IsMethodACheck(MethodBase methodBase)
    {
        return methodBase.Name.Contains("Il2Cpp.SplashScreen")
            || methodBase.Name.Contains("Il2Cpp.PlayerInventory");
    }
    private static bool IsPathChecked(string path)
    {
        return Plugin.Settings.ModStrings.Value.Any(modString => path.Contains(modString, StringComparison.OrdinalIgnoreCase));
    }

    // ReSharper disable once UnusedMember.Local
    private static void GetIl2CppStackTraceFrames()
    {
        var sb = new StringBuilder();
        var stackSize = Il2CppInterop.Runtime.IL2CPP.il2cpp_current_thread_get_stack_depth();
        sb.AppendLine($"StackTrace({stackSize}): ");
        for (var i = 0; i < stackSize; i++)
        {
            var frame = IntPtr.Zero;
            Il2CppInterop.Runtime.IL2CPP.il2cpp_current_thread_get_frame_at(i, frame);
            sb.AppendLine($"Frame {i}: {frame.ToString("X")}");
        }
        Plugin.Logger.Debug(sb.ToString());
    }
    
    /// <summary>
    /// Checks if the callstack contains "Il2Cpp.SplashScreen" then checks if the path contains any of the mod strings.
    /// </summary>
    // ReSharper disable once UnusedParameter.Local
    private static bool IsModCheck(ModCheckType checkType, string path = null, bool stackCheck = false, bool printDebug = false)
    {
        var modInPath = false;

        var stackTrace = new System.Diagnostics.StackTrace(0);
        var stackFrames = stackTrace.GetFrames();

        var checkInStack = stackFrames.Any(frame => IsMethodACheck(frame.GetMethod()));

        if (path != null && checkInStack)
        {
            modInPath = IsPathChecked(path);
        }
        var IsCheck = modInPath || (stackCheck && checkInStack);
        if (IsCheck)
        {
            Plugin.Logger.Debug(ColorARGB.Orange, $"{stackFrames[1].GetMethod()?.Name}({path}) {GetFancyCallstack(stackFrames)}"); // Should be orange
        }
        else if (Plugin.Settings.DebugSplashScreen.Value || printDebug)
        {
            Plugin.Logger.Debug($"{stackFrames[0].GetMethod()?.Name}({path}) {GetFancyCallstack(stackFrames)}");
        }
        return IsCheck;
    }

    /// <summary>
    /// Prevents the game from checking for mod directories
    /// </summary>
    /// <code>
    /// if (Directory.Exists(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "BepInEx"))) 
    ///     || Directory.Exists(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Mods"))) 
    ///     || Directory.Exists(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "MelonLoader"))) 
    ///     || Directory.Exists(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "dotnet"))) 
    ///     || File.Exists(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "doorstop_config.ini"))))
    ///	{
    ///		Application.Quit();
    ///	}
    /// </code>
    [HarmonyPrefix, HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(Il2CppSystem.IO.Directory), nameof(Il2CppSystem.IO.Directory.Exists))]
    public static bool Il2CppSystem_IO_Directory_Exists(ref bool __runOriginal, bool __result, string path)
    {
        if (!Plugin.Settings.EnableAntiSplashScreen.Value) return true;
        //Plugin.Logger.Debug($"Il2CppSystem.IO.Directory.Exists({path})");
        if (!IsModCheck(ModCheckType.DirectoryExists, path)) return __runOriginal;
        
        __runOriginal = false;
        // ReSharper disable once RedundantAssignment
        __result = false;
        return __runOriginal;
    }

    [HarmonyPrefix, HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(Il2CppSystem.IO.File), nameof(Il2CppSystem.IO.File.Exists))]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static bool Il2CppSystem_IO_File_Exists(ref bool __runOriginal, bool __result, string path)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        if (!Plugin.Settings.EnableAntiSplashScreen.Value) return true;
        //Plugin.Logger.Debug($"Il2CppSystem.IO.File.Exists({path})");
        if (!IsModCheck(ModCheckType.FileExists, path)) return __runOriginal;
        
        __runOriginal = false;
        // ReSharper disable once RedundantAssignment
        __result = false;
        return __runOriginal;
    }

    [HarmonyPostfix, HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(Il2CppSystem.IO.Directory), nameof(Il2CppSystem.IO.Directory.GetDirectories), typeof(string), typeof(string), typeof(Il2CppSystem.IO.SearchOption))] // Il2CppSystem.IO.EnumerationOptions Il2CppSystem.IO.SearchOption
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static void Il2CppSystem_IO_Directory_GetDirectories_Postfix(ref Il2CppStringArray __result, string path, string searchPattern)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        if (!Plugin.Settings.EnableAntiSplashScreen.Value) return;
        if (!IsModCheck(ModCheckType.InternalEnumeratePaths, path, true, true)) return;
        
        string[] filteredResult = [];
        foreach (var dir in __result)
        {
            if (!IsPathChecked(dir))
            {
                filteredResult.AddItem(dir);
            }
        }
        __result = filteredResult;
    }

    [HarmonyPrefix, HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(UnityEngine.Application), nameof(UnityEngine.Application.Quit), [])]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static bool UnityEngine_Application_Quit(ref bool __runOriginal)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        if (!Plugin.Settings.EnableAntiSplashScreen.Value) return true;
        if (IsModCheck(ModCheckType.Quit, null, true, true))
        {
            Plugin.Logger.Warning("Mod check attempted close the game");
            __runOriginal = false;
        }
        else
        {
            Plugin.Logger.Warning("Game will quit");
        }

        if (!Plugin.Settings.PreventClose.Value) return __runOriginal;
        __runOriginal = false;
        
        Plugin.Logger.Warning("Prevented Game Quit");
        return __runOriginal;
    }

    [HarmonyPrefix, HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(Authentication), nameof(Authentication.Logout))]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static bool Authentication_Logout(ref bool __runOriginal)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        if (!Plugin.Settings.EnableAntiSplashScreen.Value) return true;
        if (IsModCheck(ModCheckType.Logout, null, true, true))
        {
            Plugin.Logger.Warning("Mod check attempted to logout user");
            __runOriginal = false;
        }
        else
        {
            Plugin.Logger.Warning("Game will logout");
        }

        if (!Plugin.Settings.PreventLogout.Value) return __runOriginal;
        __runOriginal = false;
        
        Plugin.Logger.Warning("Prevented Game logout");
        return __runOriginal;
    }
}