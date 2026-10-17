using HarmonyLib;
using IdleConfig.UI;
using Il2Cpp;

namespace IdleConfig.Patches;

/// <summary>
/// The IdleConfig window is a clone of the game's Options popup with its content replaced.
/// PopupOptions.ReloadValues would try to populate the original options controls, which no longer exist
/// in the clone, so it is skipped for our instance (same approach as CustomSettingsMod).
/// </summary>
[HarmonyPatch(typeof(PopupOptions), "ReloadValues")]
internal static class PopupOptionsReloadValuesPatch
{
    // ReSharper disable once InconsistentNaming
    private static bool Prefix(PopupOptions __instance) => __instance == null || __instance.name != ConfigWindow.PopupName;
}
