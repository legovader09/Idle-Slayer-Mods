using UnityEngine;

namespace IdleSlayerMods.Common;

public class AchievementHelper
{
    public static Achievement CreateAchievement(AchievementType type, string achievementName, string description, Sprite? icon = null, bool grantsProgressionPoint = false)
    {
        var achievement = ScriptableObject.CreateInstance<Achievement>();
        achievement.id = $"{type}_{achievementName}";
        achievement.achievementType = type;
        achievement.description = description;
        achievement.excludeFromSteam = true;
        achievement.name = achievementName;
        achievement.icon = icon;
        achievement.grantsProgressionPoint = grantsProgressionPoint;
        achievement.localizedDescription = description;
        achievement.localizedName = achievementName;
        achievement.unlocked = false;
        return achievement;
    }
}