using UnityEngine;

namespace GameSpeedChanger;

public class GameSpeedChanger : MonoBehaviour
{
    public void Start()
    {
        Time.timeScale = Plugin.Config.DefaultSpeed.Value;
    }

    public void Update()
    {
        if (Input.GetKeyDown(Plugin.Config.SpeedUpKey.Value))
        {
            Time.timeScale = Math.Min(16f, Time.timeScale * 2f);
            Plugin.Logger.Msg($"Speed increased to {Time.timeScale}");
        }

        if (Input.GetKeyDown(Plugin.Config.SpeedDownKey.Value))
        {
            Time.timeScale = Mathf.Max(1f, Time.timeScale - 1f);
            Plugin.Logger.Msg($"Speed decreased to {Time.timeScale}");
        }

        if (Input.GetKeyDown(Plugin.Config.ResetKey.Value))
        {
            Time.timeScale = 1f;
            Plugin.Logger.Msg($"Speed reset to {Time.timeScale}");
        }
    }
}
