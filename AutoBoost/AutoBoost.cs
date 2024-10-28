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
        if (Input.GetKeyDown(KeyCode.B)) ToggleBoost();
        if (!_isEnabled || !_boost) return;
        if (!_boost.Unlocked() || _boost.currentCd != 0) return;
        Plugin.Log.LogDebug("Boost activated");
        _boost.Activate();
        _boost.currentCd = _boost.cd;
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    private void ToggleBoost()
    {
        _isEnabled = !_isEnabled;
        Plugin.Log.LogInfo($"AutoBoost is: {(_isEnabled ? "ON" : "OFF")}");
    }
}