using UnityEngine;

namespace AutoBoost;

public class AutoBoost : MonoBehaviour
{
    private BoostButton _boost;
    private bool _isEnabled;

    private void Awake()
    {
        _boost = GetComponentInChildren<BoostButton>();
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.B)) _isEnabled = !_isEnabled;
        
        _boost.holdingBoost = _isEnabled;
    }
}