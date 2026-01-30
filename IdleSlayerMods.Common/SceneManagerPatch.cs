using HarmonyLib;
using IdleSlayerMods.Common.Extensions;
using UnityEngine.SceneManagement;

namespace IdleSlayerMods.Common;

[HarmonyPatch(typeof(SceneManager), nameof(SceneManager.LoadScene), typeof(string))]
public class AscensionTreePatch
{
    public static bool Prefix(string sceneName)
    {
        Plugin.Logger.Debug($"[SceneManagerPatch] Attempting to load scene: {sceneName}");
        if (sceneName == "New Version Required" && Plugin.Settings.EnableNewVersionRequiredSkip.Value)
        {
            Plugin.Logger.Debug("[SceneManagerPatch] Blocking 'New Version Required' scene.");
            return false;
        }
        return true;
    }
}