using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace AutoBoost;

public class AutoBoost : MonoBehaviour
{
    private Boost _boost;
    private WindDash _windDash;
    private bool _autoBoostEnabled;
    private bool _windDashEnabled;
    
    private void Awake()
    {
        var boost = GameObject.Find("Abilities Manager/Boost");
        var windDash = GameObject.Find("Abilities Manager/Wind Dash");
        _boost = boost.GetComponentInChildren<Boost>();
        _windDash = windDash.GetComponentInChildren<WindDash>();
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(Plugin.Settings.ToggleKey.Value))
            ToggleBoost("Auto Boost", ref _autoBoostEnabled, Plugin.Settings.ShowPopup.Value);
        if (Plugin.Settings.EnableWindDash.Value && Input.GetKeyDown(Plugin.Settings.ToggleKeyWindDash.Value))
            ToggleBoost("Auto Wind Dash", ref _windDashEnabled, Plugin.Settings.ShowPopupWindDash.Value);
        if (CanActivateAbility(_boost, _autoBoostEnabled)) ActivateAbility(_boost, "Auto Boost");
        if (CanActivateAbility(_windDash, _windDashEnabled)) ActivateAbility(_windDash, "Auto Wind Dash");
    }

    private static bool CanActivateAbility(Ability ability, bool state) 
    {
        return state
           && ability
           && ability.Unlocked()
           && ability.currentCd.Equals(0)
           && GameState.IsRunner();
    }

    private static void ActivateAbility(Ability ability, string type) 
    {
        ability.Activate();
        ability.currentCd = ability.cd;
    }
    
    private static void ToggleBoost(string type, ref bool state, bool showPopup)
    {
        state = !state;
        Melon<Plugin>.Logger.Msg($"{type} is: {(state ? "ON" : "OFF")}");
        
        if (showPopup)
            Plugin.ModHelperInstance.ShowNotification(state ? $"{type} activated!" : $"{type} deactivated!", state);
    }
}