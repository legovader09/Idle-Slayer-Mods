using IdleSlayerMods.Common.Extensions;
using Il2Cpp;
using UnityEngine;

namespace GameSpeedChanger;

public class GameSpeedChanger : MonoBehaviour
{
    private MapController _mapController;
    private PlayerInventory _playerInventory;
    private double _specialRandomBoxChanceBackup;


    private void Awake()
    {
        _mapController = MapController.instance;
        _playerInventory = PlayerInventory.instance;
        _specialRandomBoxChanceBackup = _playerInventory.specialRandomBoxChance;
        if (_specialRandomBoxChanceBackup < 1f)
            _specialRandomBoxChanceBackup = 8f;
        ResetSpecialBoxLastUsedTimer();
    }
    
    public void Start()
    {
        Time.timeScale = Plugin.Config.DefaultSpeed.Value;
    }

    public void LateUpdate()
    {
        if (Input.GetKeyDown(Plugin.Config.SpeedUpKey.Value))
        {
            Time.timeScale = Math.Min(16f, Time.timeScale * 2f);
            Plugin.Logger.Msg($"Speed increased to {Time.timeScale}");
            Plugin.ModHelperInstance.ShowNotification($"Speed increased to: {(int)Time.timeScale}x", false);
            SaveGameSpeed();
        }

        if (Input.GetKeyDown(Plugin.Config.SpeedDownKey.Value))
        {
            Time.timeScale = Mathf.Max(1f, Time.timeScale / 2f);
            Plugin.Logger.Msg($"Speed decreased to {Time.timeScale}");
            Plugin.ModHelperInstance.ShowNotification($"Speed decreased to: {(int)Time.timeScale}x", false);
            SaveGameSpeed();
        }

        if (Input.GetKeyDown(Plugin.Config.ResetKey.Value))
        {
            Time.timeScale = 1f;
            Plugin.Logger.Msg($"Speed reset to {Time.timeScale}");
            Plugin.ModHelperInstance.ShowNotification($"Speed reset to: {(int)Time.timeScale}x", false);
            SaveGameSpeed();
        }
    }

    private static void SaveGameSpeed()
    {
        if (!Plugin.Config.SaveSpeed.Value) return;
        Plugin.Config.DefaultSpeed.Value = (int)Time.timeScale;
        Plugin.Config.DefaultSpeed.SaveEntry(true);
    }
    
    private void ResetSpecialBoxLastUsedTimer()
    {
        var currentUTCTime = Il2CppSystem.DateTime.UtcNow;
        var currentUTCUnixTimeStamp = TimeManager.GetUnixTimeStampFromDate(currentUTCTime);
        if (_mapController.specialRandomBoxLastUsed > currentUTCUnixTimeStamp)
        {
            Plugin.Logger.Debug($"Current UTC Time: {currentUTCTime}");
            Plugin.Logger.Debug($"Purple box last used: {TimeManager.GetDateTime(_mapController.specialRandomBoxLastUsed)}");
            _mapController.specialRandomBoxLastUsed = currentUTCUnixTimeStamp - 300;
            _playerInventory.specialRandomBoxChance = _specialRandomBoxChanceBackup;
            Plugin.Logger.Debug("normal chance time in future");
        }
        else
        {
            _playerInventory.specialRandomBoxChance = _specialRandomBoxChanceBackup;
            Plugin.Logger.Debug("normal chance");
        }
    }
}
