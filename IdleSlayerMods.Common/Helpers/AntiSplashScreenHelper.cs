using HarmonyLib;
using Il2CppInterop.Runtime.XrefScans;
using MelonLoader.Logging;
using MelonLoader.Pastel;
using System.Reflection;
using Il2CppInterop.Common.XrefScans;

namespace IdleSlayerMods.Common.Helpers;

internal static class AntiSplashScreenHelper
{
    public static MethodInfo ProcessMethod(Type type, string name, Type[] parameters = null, Type[] generics = null, MethodType methodType = MethodType.Normal)
    {
        MethodInfo method;
        switch (methodType)
        {
            case MethodType.Normal:
                method = AccessTools.Method(type, name, parameters, generics);
                break;
            case MethodType.Enumerator:
                var enumClassType = AccessTools.FirstInner(type, t => t.Name.Contains(name));
                method = AccessTools.FirstMethod(enumClassType, methodInfo => methodInfo.Name.Contains("MoveNext"));
                break;
            default:
                Plugin.Logger.Error($"ProcessMethod: Unsupported MethodType: {methodType}");
                return null;
        }
        if (method == null)
        {
            Plugin.Logger.Error($"Failed to find {name.Pastel(ColorARGB.GreenYellow)}");
        }
        return method;
    }
    public static IEnumerable<MethodBase> GetMethodsCallers(this MethodBase method)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        return XrefScanner.UsedBy(method)
            .Where(instance => instance.Type == XrefType.Method && instance.TryResolve() != null)
            .Select(instance => instance.TryResolve());
    }

    // Will return true if contains any of the strings in any of types methods
    public static bool GetTypeMethodsContainString(Type type, string[] strings)
    {
        // Get's the method of type's methods that contains any of the strings in the strings array
        //typeof(Helper).GetMethods()
        //    .First(mi => XrefScanner.XrefScan(mi)
        //        .Any(instance => instance.Type == XrefType.Global && instance.ReadAsObject() != null && strings.Contains(instance.ReadAsObject().ToString())));
        
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        return type.GetMethods()
            .First(mi => XrefScanner.XrefScan(mi)
                .Any(instance => instance.Type == XrefType.Global && instance.ReadAsObject() != null && strings.Contains(instance.ReadAsObject()?.ToString()))) != null;
    }

    public static bool FindStrings(MethodBase method, string[] strings)
    {
        var instances = XrefScanner.XrefScan(method);
        foreach (var instance in instances)
        {
            if (instance.Type != XrefType.Global || instance.ReadAsObject() == null) continue;
            var value = instance.ReadAsObject()?.ToString();
            if (strings.Contains(value))
            {
                return true;
            }
        }
        return false;
    }
    public static string GetShortDescription(this MethodBase method)
    {
        return method.DeclaringType is { Namespace: not null } 
            ? $"{method.DeclaringType.Namespace}.{method.DeclaringType.Name}.{method.Name}" 
            : method.Name;
    }
}