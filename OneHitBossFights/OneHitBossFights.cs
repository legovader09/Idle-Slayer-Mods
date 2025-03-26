using Il2Cpp;
using UnityEngine;

namespace OneHitBossFights;

public class OneHitBossFights : MonoBehaviour
{
    private BossMapController _bossMapController;

    private void Awake()
    {
        _bossMapController = GameObject.Find("Boss Map Controller").GetComponent<BossMapController>();
    }

    private void Update()
    {
        if (_bossMapController.currentBossHealth <= 1) return;
        Plugin.Logger.Msg("Setting boss health to 1");
        _bossMapController.currentBossHealth = 1;
    }
}