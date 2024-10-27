using HarmonyLib;

namespace RevealMimics;

[HarmonyPatch(typeof(ChestHuntManager), "Awake")]
public static class PluginPatch
{
    public static void Postfix(ChestHuntManager instance)
    {
        // Only add the MimicRevealer component once.
        if (instance != null && instance.GetComponent<MimicRevealer>() == null)
        {
            instance.gameObject.AddComponent<MimicRevealer>();
        }
    }
}