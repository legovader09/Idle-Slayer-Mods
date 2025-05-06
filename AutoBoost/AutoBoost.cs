using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace AutoBoost;

public class AutoBoost : MonoBehaviour
{
    private Boost _boost;
    private WindDash _windDash;
    private PlayerMovement _playerMovement;
    private bool _autoBoostEnabled;
    private bool _windDashEnabled;
    
    private void Awake()
    {
        var boost = GameObject.Find("Abilities Manager/Boost");
        var windDash = GameObject.Find("Abilities Manager/Wind Dash");
        _boost = boost.GetComponentInChildren<Boost>();
        _windDash = windDash.GetComponentInChildren<WindDash>();
        _playerMovement = PlayerMovement.instance;
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(Plugin.Settings.ToggleKey.Value))
            ToggleBoost("Auto Boost", ref _autoBoostEnabled, Plugin.Settings.ShowPopup.Value);
        if (Plugin.Settings.EnableWindDash.Value && Input.GetKeyDown(Plugin.Settings.ToggleKeyWindDash.Value))
            ToggleBoost("Auto Wind Dash", ref _windDashEnabled, Plugin.Settings.ShowPopupWindDash.Value);
        if (CanActivateAbility(_boost, _autoBoostEnabled)) ActivateAbility(_boost);
        if (CanActivateWindDash(_windDash, _windDashEnabled)) ActivateAbility(_windDash);
    }

    private static bool CanActivateAbility(Ability ability, bool state)
    {
        return state
           && ability
           && ability.Unlocked()
           && ability.GetCurrentCooldown().Equals(0)
           && GameState.IsRunner();
    }

    private bool CanActivateWindDash(Ability ability, bool state) 
    {
        return CanActivateAbility(ability, state)
           && (!Plugin.Settings.WindDashOnTheGround.Value || _playerMovement.IsGrounded());
    }

    private static void ActivateAbility(Ability ability) 
    {
        ability.Activate();
        ability.currentCd = ability.GetCooldown();
    }
    
    private static void ToggleBoost(string type, ref bool state, bool showPopup)
    {
        state = !state;
        Melon<Plugin>.Logger.Msg($"{type} is: {(state ? "ON" : "OFF")}");
        
        if (showPopup)
            Plugin.ModHelperInstance.ShowNotification(state ? $"{type} activated!" : $"{type} deactivated!", state);
    }
}
