using IdleSlayerMods.Common.Extensions;
using IdleSlayerMods.Common.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace IdleConfig.UI;

/// <summary>
/// Loads the menu button icon from the PNG embedded in IdleConfig.dll (see IdleConfig.csproj) and
/// verifies that the button keeps it.
/// The sprite is cached and flagged DontUnloadUnusedAsset, so it is decoded once per session.
/// </summary>
internal static class IconLoader
{
    private const string ResourceName = "IdleConfig.Assets.IdleConfigIcon.png";

    // How long to keep checking that nothing replaces the sprite after the button is created.
    private const float WatchSeconds = 5f;

    private static Sprite _icon;
    private static bool _failed;

    private static Image _watchedImage;
    private static float _watchUntil;
    private static bool _overwriteReported;

    internal static Sprite GetIcon()
    {
        if (_icon) return _icon;
        if (_failed) return null;

        Plugin.Logger.Debug($"Loading menu icon '{ResourceName}'...");
        _icon = SpriteHelper.LoadSpriteFromEmbeddedResource(typeof(IconLoader).Assembly, ResourceName);
        if (!_icon)
        {
            _failed = true;
            Plugin.Logger.Debug("Menu icon could not be loaded (see [SpriteHelper] lines above for the failing stage).");
        }
        return _icon;
    }

    /// <summary>
    /// Starts watching the button's icon Image, so a later overwrite (by the template's own scripts,
    /// an animator, or another mod) is logged and corrected rather than silently losing the icon.
    /// </summary>
    internal static void Watch(GameObject button)
    {
        if (!button || !_icon) return;

        _watchedImage = button.transform.Find("Background/Content")?.GetChild(0)?.GetComponent<Image>();
        if (!_watchedImage)
        {
            Plugin.Logger.Warning("Menu icon: could not find the button's icon Image to verify.");
            return;
        }

        _watchUntil = Time.unscaledTime + WatchSeconds;
        _overwriteReported = false;
    }

    /// <summary>Called every frame by the IdleConfig behaviour while a watch is active.</summary>
    internal static void Tick()
    {
        if (!_watchedImage) return;

        if (_watchedImage.sprite != _icon || !_watchedImage.enabled)
        {
            if (!_overwriteReported)
            {
                var replacement = _watchedImage.sprite ? _watchedImage.sprite.name : "null";
                Plugin.Logger.Warning($"Menu icon was overwritten after assignment (sprite now '{replacement}', " +
                                      $"enabled={_watchedImage.enabled}); reapplying.");
                _overwriteReported = true;
            }
            _watchedImage.sprite = _icon;
            _watchedImage.enabled = true;
        }

        if (Time.unscaledTime < _watchUntil) return;

        Plugin.Logger.Debug(_overwriteReported
            ? "Menu icon: reapplied after an overwrite; check the warning above for what replaced it."
            : $"Menu icon: verified on the button for {WatchSeconds:0}s, not overwritten.");
        _watchedImage = null;
    }
}
