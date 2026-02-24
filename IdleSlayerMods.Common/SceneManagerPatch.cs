using HarmonyLib;
using IdleSlayerMods.Common.Extensions;
using UnityEngine.SceneManagement;

namespace IdleSlayerMods.Common;

/// <summary>
/// A Harmony patch for Unity's <see cref="SceneManager"/> class that modifies the behavior of the
/// <see cref="SceneManager.LoadScene(string)"/> method to conditionally block "New Version Required" scene from loading,
/// </summary>
[HarmonyPatch(typeof(SceneManager), nameof(SceneManager.LoadScene), typeof(string))]
public class NewVersionRequiredPatch
{
    /// <summary>
    /// A Harmony patch for Unity's <see cref="SceneManager"/> class that modifies the behavior of the
    /// <see cref="SceneManager.LoadScene(string)"/> method to conditionally block "New Version Required" scene from loading,
    /// </summary>
    /// <param name="sceneName">The name of the scene that is being requested to load.</param>
    /// <returns>
    /// A boolean indicating whether the original scene loading process should proceed.
    /// Returns false to block the scene load, or true to allow it.
    /// </returns>
    public static bool Prefix(string sceneName)
    {
        Plugin.Logger.Debug($"[SceneManagerPatch] Attempting to load scene: {sceneName}");
        if (sceneName != "New Version Required" || !Plugin.Settings.EnableNewVersionRequiredSkip.Value) return true;
        
        Plugin.Logger.Debug("[SceneManagerPatch] Blocking 'New Version Required' scene.");
        return false;
    }
}