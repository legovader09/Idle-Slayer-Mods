using UnityEngine;
using Input = UnityEngine.Input;
using KeyCode = UnityEngine.KeyCode;

namespace BonusStageCompleter;

public class BonusStageCompleter : MonoBehaviour
{
    private MapController _mapController;
    private BonusMapController _bonusController;
    private bool _isInBonusGame;
    
    private void Awake()
    {
        _mapController = GameObject.Find("Map").GetComponent<MapController>();
        _bonusController = GameObject.Find("Bonus Map Controller").GetComponent<BonusMapController>();
    }

    private void LateUpdate()
    {
#if DEBUG
        if (Input.GetKeyDown(KeyCode.P))
        {
            _mapController.ChangeMap(_mapController.CurrentBonusMap());
        }
#endif

        _isInBonusGame = _mapController.selectedMap.name.Contains("bonus");
        
        // only do logic in bonus stages
        if (!_isInBonusGame || !_bonusController.showCurrentTime) return;
        
        // determine whether the collected spheres variable has already been set so we don't overwrite it.
        var pickedUp = _bonusController.bonusSpheresPickedUp;
        var total = (int)_bonusController.currentSection.requiredSpheres;
        
        if (pickedUp == total - 1 || pickedUp == total) return;
        _bonusController.bonusSpheresPickedUp = total - 1;
        Plugin.Log.LogInfo($"Set spheres picked up to: {_bonusController.bonusSpheresPickedUp}");
    }
}