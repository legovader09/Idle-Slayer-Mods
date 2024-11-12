using UnityEngine;
using UnityEngine.UI;

namespace RevealMimics;

public class ChestRevealer : MonoBehaviour
{
    private ChestHuntManager _chestHuntManager;
    private bool _chestUpdateCompleted;

    private void Awake()
    {
        _chestHuntManager = gameObject.GetComponentInChildren<ChestHuntManager>();
    }

    private void Update()
    {
#if DEBUG
        if (Input.GetKeyDown(KeyCode.P))
        {
            _chestHuntManager.StartEvent();
        }
#endif
        
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
                var @object = chest.chestObject;
                if (!@object) continue;
                
                @object.GetComponent<Image>().color = chest.type switch
                {
                    ChestType.Mimic => new(255, 0, 0),
                    ChestType.Multiplier when Plugin.Settings.ShouldRevealMultipliers.Value => new(255, 255, 0),
                    ChestType.DuplicateNextPick when Plugin.Settings.ShouldRevealDuplicator.Value => new(0, 255, 0),
                    _ => @object.GetComponent<Image>().color
                };
            }

            _chestUpdateCompleted = true;
            Plugin.Log.LogInfo("Chests reveal complete");
        }
    }
}