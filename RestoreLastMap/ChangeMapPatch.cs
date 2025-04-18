using HarmonyLib;
using IdleSlayerMods.Common.Extensions;
using Il2Cpp;

namespace RestoreLastMap;

[HarmonyPatch(typeof(MapController), nameof(MapController.ChangeMap))]
public class ChangeMapPatch
{
    public static event Action<Map> OnChangeMap;
    
    // ReSharper disable once UnusedParameter.Global
    // ReSharper disable once InconsistentNaming
    public static void Postfix(MapController __instance, Map newMap)
    {
        Plugin.Logger.Debug($"Last visited map set to: {newMap.name}");
        OnChangeMap?.Invoke(newMap);
    }
}