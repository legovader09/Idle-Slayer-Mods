using IdleSlayerMods.Common.Extensions;
using UnityEngine;

namespace GameSpeedChanger;

public class GameSpeedChanger : MonoBehaviour
{
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
            Time.timeScale = Mathf.Max(1f, Time.timeScale - 1f);
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
}
