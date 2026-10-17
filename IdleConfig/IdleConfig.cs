using System;
using IdleConfig.UI;
using UnityEngine;

namespace IdleConfig;

/// <summary>
/// Lives in the Game scene. Adds the "Idle Config" info panel button and owns the settings window.
/// </summary>
public class IdleConfig : MonoBehaviour
{
    private const string ButtonText = "Idle Config";

    private ConfigWindow _window;
    private bool _setupDone;

    private void Start() => TrySetup();

    private void Update()
    {
        // ModHelper is mounted by IdleSlayerMods.Common in the same scene load; retry until it exists.
        if (!_setupDone) TrySetup();
        _window?.Tick();
        IconLoader.Tick();
    }

    private void OnDestroy()
    {
        _window?.Dispose();
        _window = null;
    }

    private void TrySetup()
    {
        if (_setupDone || !Plugin.ModHelper) return;
        _setupDone = true;

        try
        {
            _window = ConfigWindow.Create();
            var button = Plugin.ModHelper.CreatePanelButton(ButtonText, OnButtonClicked, IconLoader.GetIcon());
            if (!button) Plugin.Logger.Warning("Could not add the Idle Config button to the info panel.");
            else IconLoader.Watch(button);
        }
        catch (Exception ex)
        {
            Plugin.Logger.Error($"IdleConfig setup failed: {ex}");
        }
    }

    private void OnButtonClicked()
    {
        _window ??= ConfigWindow.Create();
        _window?.Toggle();
    }
}
