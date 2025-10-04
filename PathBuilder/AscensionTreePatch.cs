using HarmonyLib;
using IdleSlayerMods.Common.Extensions;
using Il2Cpp;
using UnityEngine;

namespace PathBuilder;

[HarmonyPatch(typeof(SkillTreeManager), nameof(SkillTreeManager.BuyAscensionSkill))]
public class AscensionTreePatch
{
    public static bool Prefix(SkillTreeManager __instance, string id, bool showNotification)
    {
        showNotification = false;
        Plugin.Logger.Debug($"BuyAscensionSkill called: {id}");
        var skill = PlayerInventory.instance.ascensionSkills.FirstOrDefault((skill) => skill.id == id);
        if (!skill) return false;
        Plugin.Logger.Debug(skill.skillObjectComponent.connectionLines.Count.ToString());
        foreach (var req in skill.skillObjectComponent.connectionLines)
        {
            Plugin.Logger.Debug(req.gameObject.name);
            req.image.color = new(0, 255, 255);
            req.image.m_Color = new(0, 255, 255);
        }

        return true;
    }
}