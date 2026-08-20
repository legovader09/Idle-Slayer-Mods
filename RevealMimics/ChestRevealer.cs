using Il2Cpp;
using MelonLoader;
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
        if (Input.GetKeyDown(KeyCode.O))
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
            Melon<Plugin>.Logger.Msg("Iterating through chests");
            
            foreach (var chest in _chestHuntManager.chests)
            {
                var @object = chest.chestObject;
                if (!@object) continue;
                
                @object.GetComponent<Image>().color = chest.type switch
                {
                    ChestType.Mimic => new(1f, 0f, 0f),
                    ChestType.Multiplier when Plugin.Settings.ShouldRevealMultipliers.Value => new(1f, 1f, 0f),
                    ChestType.DuplicateNextPick when Plugin.Settings.ShouldRevealDuplicator.Value => new(0f, 1f, 0f),
                    ChestType.ArmoryChest when Plugin.Settings.ShouldRevealArmoryChest.Value => new(0f, 0.5f, 1f),
                    ChestType.MultiplierIncreaser when Plugin.Settings.ShouldRevealMultiplierIncreaser.Value => new(1f, 0.65f, 0f),
                    _ => @object.GetComponent<Image>().color
                };
            }

            _chestUpdateCompleted = true;
            Melon<Plugin>.Logger.Msg("Chests reveal complete");
        }
    }
}