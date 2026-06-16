using HarmonyLib;
using IdleSlayerMods.Common.Extensions;
using Il2Cpp;
using UnityEngine;

namespace GameSpeedChanger;

public class GameSpeedChanger : MonoBehaviour
{
    private MapController _mapController;
    private PlayerInventory _playerInventory;
    private double _specialRandomBoxChanceBackup;
    private static bool _shouldRunReset;

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
        var multiplier = Plugin.Config.CustomMultiplier.Value;
        var noLimits = Plugin.Config.RemoveLimiter.Value;
        if (Input.GetKeyDown(Plugin.Config.SpeedUpKey.Value))
        {
            Time.timeScale = noLimits ? Time.timeScale * multiplier : Math.Min(16f, Time.timeScale * multiplier);
            Plugin.Logger.Msg($"Speed increased to {Time.timeScale}");
            Plugin.ModHelperInstance.ShowNotification($"Speed increased to: {Time.timeScale:N2}x", false);
            SaveGameSpeed();
        }

        if (Input.GetKeyDown(Plugin.Config.SpeedDownKey.Value))
        {
            Time.timeScale = noLimits ? Time.timeScale / multiplier : Mathf.Max(1f, Time.timeScale / multiplier);
            Plugin.Logger.Msg($"Speed decreased to {Time.timeScale}");
            Plugin.ModHelperInstance.ShowNotification($"Speed decreased to: {Time.timeScale:N2}x", false);
            SaveGameSpeed();
        }

        if (Input.GetKeyDown(Plugin.Config.ResetKey.Value))
        {
            Time.timeScale = 1f;
            Plugin.Logger.Msg($"Speed reset to {Time.timeScale}");
            Plugin.ModHelperInstance.ShowNotification($"Speed reset to: {Time.timeScale:N2}x", false);
            SaveGameSpeed();
        }

        if (!_shouldRunReset) return;
        _shouldRunReset = false;
        ResetSpecialBoxLastUsedTimer();
    }

    private static void SaveGameSpeed()
    {
        if (!Plugin.Config.SaveSpeed.Value) return;
        Plugin.Config.DefaultSpeed.Value = (int)Time.timeScale;
        Plugin.Config.DefaultSpeed.SaveEntry(true);
    }

    public void ResetSpecialBoxLastUsedTimer()
    {
        var currentUTCTime = Il2CppSystem.DateTime.UtcNow;
        var currentUTCUnixTimeStamp = TimeManager.GetUnixTimeStampFromDate(currentUTCTime);
        if (_mapController.specialRandomBoxLastUsed > currentUTCUnixTimeStamp)
        {
            Plugin.Logger.Debug($"Current UTC Time: {currentUTCTime}");
            Plugin.Logger.Debug($"Purple box last used: {TimeManager.GetDateTime(_mapController.specialRandomBoxLastUsed)}");
            _mapController.specialRandomBoxLastUsed = currentUTCUnixTimeStamp - 300;
            Plugin.Logger.Debug("Normal chance time is in the future");
        }

        if (Plugin.NoSpecialBoxesModeDetected) return;
        _playerInventory.specialRandomBoxChance = _specialRandomBoxChanceBackup;
        Plugin.Logger.Debug("Purple box chance changed");
    }

    private static int specialBoxes;
    private static int normalBoxes;
    private static void LogBoxes()
    {
        var logContent = $"Special Boxes: {specialBoxes}\nNormal Boxes: {normalBoxes}\nTime elapsed: {Time.time}";
        Plugin.Logger.Debug(logContent);
    }

    [HarmonyPatch(typeof(RandomBox), "OnObjectSpawn")]
    public class Patch_RandomBox_OnObjectSpawn
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            _shouldRunReset = true;
            normalBoxes++;
            LogBoxes();
        }
    }

    [HarmonyPatch(typeof(SpecialRandomBox), "OnObjectSpawn")]
    public class Patch_SpecialRandomBox_OnObjectSpawn
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            _shouldRunReset = true;
            specialBoxes++;
            LogBoxes();
        }
    }
}
