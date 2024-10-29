using UnityEngine;

namespace AutoBoost;

public class AutoBoost : MonoBehaviour
{
    private Boost _boost;
    private bool _isEnabled;
    
    private void Awake()
    {
        _boost = GetComponentInChildren<Boost>();
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(Plugin.Settings.ToggleKey.Value)) ToggleBoost();
        if (CanActivateBoost()) ActivateBoost();
    }

    private bool CanActivateBoost() 
    {
        return _isEnabled
               && _boost
               && _boost.Unlocked()
               && GameState.IsRunner()
               && _boost.currentCd.Equals(0);
    }

    private void ActivateBoost() 
    {
        Plugin.Log.LogDebug("Boost activated");
        _boost.Activate();
        _boost.currentCd = _boost.cd;
    }
    
    private void ToggleBoost()
    {
        _isEnabled = !_isEnabled;
        Plugin.Log.LogInfo($"AutoBoost is: {(_isEnabled ? "ON" : "OFF")}");
        
        if (Plugin.Settings.ShowPopup.Value)
            Plugin.ModHelperInstance.ShowNotification(_isEnabled ? "Auto Boost activated!" : "Auto Boost deactivated!", _isEnabled);
    }
}