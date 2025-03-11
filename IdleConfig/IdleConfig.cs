using System.IO;
using BepInEx;
using IdleSlayerMods.Common;
using TMPro;
using UnityEngine;

namespace IdleConfig;

public class IdleConfig : MonoBehaviour
{
    private void Start()
    {
        Plugin.Log.LogInfo(Path.Combine(Paths.PluginPath, "IdleConfig", "IdleConfigIcon.png"));
        var texture = ModHelper.LoadTextureFromFile(Path.Combine(Paths.PluginPath, "IdleConfig", "IdleConfigIcon.png"));

        Plugin.ModHelper.AddPanelButton("Idle Config", ClickAction, texture);
    }

    private static void ClickAction()
    {
        Plugin.ModHelper.ShowDialog("Idle Config");
    }
}