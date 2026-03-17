using HarmonyLib;
using Il2Cpp;

namespace IdleSlayerMods.Common.HarmonyPatches;

/// <summary>
/// A Harmony patch class for modifying the behavior of the CheckShowButton method in the SilverRandomBoxManager class.
/// This patch prevents the original method from executing and adds custom logic to control the visibility of the silver random box button.
/// </summary>
[HarmonyPatch]
public class SilverRandomBoxManagerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SilverRandomBoxManager), nameof(SilverRandomBoxManager.CheckShowButton))]

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static void SilverRandomBoxManager_CheckShowButton(SilverRandomBoxManager __instance, ref bool __runOriginal)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        if (!Plugin.Settings.ApplySilverRandomBoxPatch.Value) return;
        
        var silverBoxButton = __instance.silverRandomBoxButton;

        __runOriginal = false;
        if (PlayerInventory.instance != null && Upgrades.list.RandomBox.bought && GameState.IsRunner())
        {
            //if (TimeManager.lastWorkingTime > __instance.lastTimeUsed + (__instance.minimumMinutesDifference * 60))
            //{
            //    return;
            //} 

            silverBoxButton.SetActive(true);
        }
        silverBoxButton.SetActive(false);
    }

}