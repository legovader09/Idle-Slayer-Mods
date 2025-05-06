using System.IO;
using IdleSlayerMods.Common;
using MelonLoader.Utils;
using UnityEngine;

namespace IdleConfig;

public class IdleConfig : MonoBehaviour
{
    private void Start()
    {
        Plugin.Logger.Msg(Path.Combine(MelonEnvironment.ModsDirectory, "IdleConfigIcon.png"));
        var bundle = ModHelper.LoadBundle(Path.Combine(MelonEnvironment.ModsDirectory, "IdleConfig.asset"));
        var texture = ModHelper.LoadTextureFromBundle(bundle, "IdleConfigIcon.png");

        Plugin.ModHelper.AddPanelButton("Idle Config", ClickAction, texture);
    }

    private static void ClickAction()
    {
        Plugin.ModHelper.ShowDialog("Idle Config");
    }
}