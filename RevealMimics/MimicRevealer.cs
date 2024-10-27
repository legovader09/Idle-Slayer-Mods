using UnityEngine;
using UnityEngine.UI;

namespace RevealMimics;

public class MimicRevealer : MonoBehaviour
{
    private ChestHuntManager _chestHuntManager;

    private void Awake()
    {
        _chestHuntManager = gameObject.GetComponentInChildren<ChestHuntManager>();
    }

    private void Update()
    {
        if (!_chestHuntManager.IsVisible() || _chestHuntManager.chests.Count == 0) return;
        
        foreach (var chest in _chestHuntManager.chests)
        {
            if (chest.type == ChestType.Mimic && chest.chestObject)
            {
                chest.chestObject.GetComponent<Image>().color = new (255, 0, 0);
            }
        }
    }
}