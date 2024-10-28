using UnityEngine;
using UnityEngine.UI;

namespace RevealMimics;

public class MimicRevealer : MonoBehaviour
{
    private ChestHuntManager _chestHuntManager;
    private bool _chestUpdateCompleted;

    private void Awake()
    {
        _chestHuntManager = gameObject.GetComponentInChildren<ChestHuntManager>();
    }

    private void Update()
    {
        if (!GameState.IsChestHunt())
        {
            _chestUpdateCompleted = false;
        }
        else if (!_chestUpdateCompleted)
        {
            if (!_chestHuntManager.IsVisible() || _chestHuntManager.chests.Count == 0) return;
            Plugin.Log.LogInfo("Iterating through chests");
            
            foreach (var chest in _chestHuntManager.chests)
            {
                if (!chest.chestObject) continue;
                if (chest.type == ChestType.Mimic)
                {
                    chest.chestObject.GetComponent<Image>().color = new (255, 0, 0);
                }

                if (chest.type == ChestType.Multiplier && Plugin.Settings.ShouldRevealMultipliers.Value)
                {
                    chest.chestObject.GetComponent<Image>().color = new (255, 255, 0);
                }
            }

            _chestUpdateCompleted = true;
            Plugin.Log.LogInfo("Chests reveal complete");
        }
    }
}