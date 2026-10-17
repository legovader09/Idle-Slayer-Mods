using System.Reflection;
using System.Runtime.CompilerServices;
using IdleSlayerMods.Common.Config;
using IdleSlayerMods.Common.Constants;
using IdleSlayerMods.Common.Extensions;
using IdleSlayerMods.Common.Helpers;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using File = Il2CppSystem.IO.File;
using Path = Il2CppSystem.IO.Path;

namespace IdleSlayerMods.Common;

/// <inheritdoc />
public class ModHelper : MonoBehaviour
{
    /// <summary>
    /// Event that occurs once the ModHelper has been added to the game scene and is ready to use.
    /// </summary>
    // ReSharper disable once EventNeverSubscribedTo.Global
    public static event Action<ModHelper> ModHelperMounted;
    private AchievementManager _achievementManager;
    private NotificationText _notificationText;
    private Popup _popup;
    private Transform _infoPanelButtonsContainer;
    private GameObject _templateButton;
    private bool anyModSettings;

    private void OnModHelperMounted()
    {
        ModHelperMounted?.Invoke(this);
        Plugin.Logger.Debug("ModHelper mounted successfully");
    }

    private void Awake()
    {
        OnModHelperMounted();
        _notificationText = GameObject.Find(PathConstants.NotificationTextPath).GetComponent<NotificationText>();
        _popup = GameObject.Find(PathConstants.PopupPath).GetComponent<Popup>();
        _achievementManager = GameObject.Find(PathConstants.AchievementManagerPath).GetComponent<AchievementManager>();
        _infoPanelButtonsContainer = GameObject.Find(PathConstants.ButtonPanelPath).transform;
        _templateButton = ButtonHelper.CreateTemplateButton($"{PathConstants.ButtonPanelPath}/Settings");
    }

    /// <summary>
    /// Add an interactable button to the info panel.
    /// This is the same place as Achievements, Options, etc.
    /// </summary>
    /// <param name="text">Text to display on the button, internally also sets the object name.</param>
    /// <param name="clickAction">Action delegate that occurs on button click.</param>
    /// <param name="icon">Texture2D icon to display on the button. (Aspect Ratio of 2:1 canvas size recommended to avoid stretching)</param>
    [Obsolete("Images do NOT load anymore.")]
    public void AddPanelButton(string text, Action clickAction = null, Texture2D icon = null)
    {
        Plugin.Logger.Debug($"Registering panel button: {text}");
        var button = Instantiate(_templateButton, _infoPanelButtonsContainer?.transform);
        if (button is null)
        {
            Plugin.Logger.Warning("Unable to instantiate template panel button");
            return;
        }
        
        button.name = text;

        // Prevents StartingText from overwriting TMP text
        var visualContent = button.transform.Find("Background/Content");
        var startingText = visualContent.GetChild(1).GetComponent<StartingText>();
        if (startingText) DestroyImmediate(startingText);

        // Get rid of duplicated component's image and add rawimage to allow custom texture loading
        var buttonImage = visualContent.GetChild(0).gameObject;
        DestroyImmediate(buttonImage.GetComponent<Image>());
        var rawImage = buttonImage.AddComponent<RawImage>();
        
        // Update TMP text
        var tmp = visualContent.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.m_text = text;
        
        // Add event listener
        button.GetComponent<Button>().onClick.RemoveAllListeners();
        button.GetComponent<Button>().onClick.AddListener((UnityAction)(() => { clickAction?.Invoke(); }));
        // icon ??= Texture2D.redTexture;
        rawImage.texture = icon;
        
        Plugin.Logger.Debug($"Button {text} added successfully");
    }

    /// <summary>
    /// Add an interactable button to the info panel (the same place as Achievements, Options, etc.)
    /// and return it so the caller can customise it further.
    /// </summary>
    /// <param name="text">Text to display on the button, internally also sets the object name.</param>
    /// <param name="clickAction">Action delegate that occurs on button click.</param>
    /// <param name="icon">Sprite displayed in the button's icon slot. See <see cref="SpriteHelper"/> for loading one.
    /// If null, the icon slot is left empty.</param>
    /// <returns>The new button GameObject, or null if the info panel could not be found.</returns>
    public GameObject CreatePanelButton(string text, Action clickAction = null, Sprite icon = null)
    {
        Plugin.Logger.Debug($"Creating panel button: {text}");
        if (_templateButton == null || _infoPanelButtonsContainer == null)
        {
            Plugin.Logger.Warning("Info panel or template button not available; cannot create panel button");
            return null;
        }

        var button = Instantiate(_templateButton, _infoPanelButtonsContainer);
        button.name = text;

        var visualContent = button.transform.Find("Background/Content");
        if (visualContent != null && visualContent.childCount >= 2)
        {
            // Keep the template's own Image so sizing and materials match the native buttons.
            var image = visualContent.GetChild(0).GetComponent<Image>();
            if (image != null)
            {
                image.sprite = icon;
                image.preserveAspect = true;
                image.enabled = icon != null;
                Plugin.Logger.Msg($"[SpriteHelper] button '{text}': icon slot '{visualContent.GetChild(0).name}' " +
                                  (icon != null ? $"received sprite '{icon.name}'" : "left empty (no sprite supplied)"));
            }
            else
            {
                Plugin.Logger.Warning($"[SpriteHelper] button '{text}': icon slot '{visualContent.GetChild(0).name}' has no Image component");
            }

            // Same treatment as AddPanelButton: drop StartingText so it can't overwrite our label.
            var label = visualContent.GetChild(1);
            var startingText = label.GetComponent<StartingText>();
            if (startingText) DestroyImmediate(startingText);
            var tmp = label.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = text;
                tmp.m_text = text;
            }
        }
        else
        {
            Plugin.Logger.Warning($"Panel button '{text}' has an unexpected layout; icon and label were not applied");
        }

        var uiButton = button.GetComponent<Button>();
        if (uiButton != null)
        {
            uiButton.onClick = new Button.ButtonClickedEvent();
            uiButton.onClick.AddListener((UnityAction)(() => clickAction?.Invoke()));
        }

        return button;
    }

    /// <summary>
    /// Creates a settings toggle in the user interface with the specified display name
    /// and binds it to a configuration entry.
    /// </summary>
    /// <remarks>
    /// Settings bound through <see cref="Config.BaseConfig"/> are now discovered automatically by
    /// configuration UIs (such as IdleConfig) via <see cref="ConfigRegistry"/>, so new mods do not need this.
    /// When such a UI is installed, this method no longer injects a toggle into the vanilla options menu;
    /// it only applies <paramref name="displayName"/> and forwards value changes to <paramref name="onChanged"/>.
    /// Without one, it behaves exactly as before.
    /// </remarks>
    /// <param name="displayName">The display name of the toggle in the UI</param>
    /// <param name="configEntry">The configuration entry representing the state of the toggle.</param>
    /// <param name="onChanged">Callback action invoked when the toggle value changes.</param>
    [Obsolete("Settings bound via BaseConfig are discovered automatically by config UIs such as IdleConfig. " +
              "For change callbacks, subscribe to MelonPreferences_Entry.OnEntryValueChanged instead.")]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void CreateSettingsToggle(string displayName, MelonPreferences_Entry<bool> configEntry, Action<bool> onChanged)
    {
        LegacySettingsBridge.Register(Assembly.GetCallingAssembly(), displayName, configEntry);

        if (ConfigRegistry.HasUIProvider)
        {
            Plugin.Logger.Debug($"Config UI provider present; '{displayName}' will be shown there instead of the options menu");
            LegacySettingsBridge.WireCallback(configEntry, onChanged);
            return;
        }

        Plugin.Logger.Debug($"Registering settings toggle: {displayName}");
        var template = GameObject.Find($"{PathConstants.SettingsTogglePath}/Confirm Portal");
        var toggle = Instantiate(template, template.transform.parent);
        toggle.transform.SetSiblingIndex(anyModSettings ? 1 : 0);
        toggle.name = displayName;
        
        UIHelper.SetLabelText(toggle.transform.Find("Label"), displayName);

        var toggler = toggle.transform.Find("Animated Switch").GetComponent<Toggle>();
        toggler.onValueChanged.RemoveAllListeners();
        toggler.isOn = configEntry.Value;
        toggler.onValueChanged.AddListener((UnityAction<bool>)(state => { SettingsHelper.OnToggleChanged(state, configEntry, onChanged);}));

        if (anyModSettings) return;
        anyModSettings = true;
        CreateSettingsCategory("Mod Settings");
    }

    /// <summary>
    /// Creates a settings category in the UI to group related configuration toggles.
    /// </summary>
    /// <param name="displayName">The name to display for the settings category in the user interface.</param>
    /// <param name="index">The position index where the settings category should be placed in the UI hierarchy.</param>
    public void CreateSettingsCategory(string displayName, int index = 0)
    {
        Plugin.Logger.Debug($"Registering settings category: {displayName}");
        var template = GameObject.Find($"{PathConstants.SettingsTogglePath}/Screen Title");
        var title = Instantiate(template, template.transform.parent);
        title.transform.SetSiblingIndex(index);
        title.name = displayName;
        UIHelper.SetLabelText(title.transform.Find("Label"), displayName);
    }
    
    /// <summary>
    /// Show a dialog popup with information and interactable buttons.
    /// </summary>
    /// <param name="title">The top title of the dialog.</param>
    /// <param name="subtitle">The subtitle of the dialog.</param>
    /// <param name="descriptionText">The general message to show on the dialog.</param>
    /// <param name="descriptionColor">Colour of the description text.</param>
    /// <param name="centerDescription">Whether to center the description text in the vertical space.</param>
    /// <param name="confirmText">Confirm button text. If left empty, this button won't render.</param>
    /// <param name="confirmAction">Action to execute on confirm button press.</param>
    /// <param name="cancelText">Cancel button text.</param>
    /// <param name="cancelAction">Action to execute on confirm button press. Closes dialog by default.</param>
    /// <param name="content">The GameObject to display in the popup</param>
    /// <param name="overridePopup">If true, any existing popup will be overridden by this popup. If false, this will display on top of any existing popups.</param>
    public void ShowDialog(string title = "Dialog", string subtitle = "", string descriptionText = "", Color descriptionColor = default, bool centerDescription = false, string confirmText = "", Action confirmAction = null, string cancelText = "Close", Action cancelAction = null, GameObject content = null, bool overridePopup = false)
    {
        if (_popup == null) return;
        Plugin.Logger.Debug("Popup found");
        var panel = _popup.transform.GetChild(0).GetChild(0);
        if (!panel) return;
        Plugin.Logger.Debug("Panel found");
        panel.transform.GetChild(1).gameObject.SetActive(false);
        
        var container = panel.transform.GetChild(3).GetChild(0).GetChild(0);
        content?.transform.SetParent(container);
        
        _popup.Show(new()
        {
            title = title,
            subtitle = subtitle,
            confirmActionText = confirmText,
            confirmAction = confirmAction,
            overrideCurrentPopup = overridePopup,
            description = descriptionText,
            descriptionColor = descriptionColor,
            cancelActionText = cancelText,
            cancelAction = cancelAction,
            centeredDescription = centerDescription,
        });
    }

    /// <summary>
    /// Display an achievement unlocked popup
    /// </summary>
    /// <param name="achievement">The achievement to display.
    /// See <see cref="Achievement"/> for more information, and use the <see cref="AchievementHelper"/> class to easily create an achievement.
    /// <seealso cref="AchievementHelper.CreateAchievement"/>
    /// </param>
    public void ShowAchievement(Achievement achievement) => _achievementManager?.ShowNextUnlockedAchivement(achievement);

    /// <summary>
    /// Show a text in the center of the screen, the same type as lucky box event texts.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="shine">Whether to play a sparkle/shine animation with this text.</param>
    public void ShowNotification(string message, bool shine)
    {
        Plugin.Logger.Debug($"Attempting to show notification with message: {message}");
        _notificationText?.Show(message, shine);
    }

    /// <summary>
    /// Loads an asset bundle from the specified file path.
    /// </summary>
    /// <param name="bundlePath">The file path of the asset bundle to load.</param>
    /// <returns>The loaded AssetBundle, or null if the loading fails.</returns>
    [Obsolete("IL2CPP seems to have an issue with loading asset bundles. ")]
    public static AssetBundle LoadBundle(string bundlePath)
    {
        try
        {
            var fullPath = Path.Combine(bundlePath);
            var bundle = AssetBundle.LoadFromFile(fullPath);
            if (bundle) return bundle;
            Plugin.Logger.Error($"Failed to load asset bundle from {fullPath}");
            return null;
        }
        catch (Exception e)
        {
            Plugin.Logger.Error($"Error loading asset bundle: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Load texture image from file path.
    /// </summary>
    /// <param name="filePath">The path pointing to the image file.</param>
    /// <returns>Texture2D object or null on failure.</returns>
    public static Texture2D LoadTextureFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Plugin.Logger.Error($"[SpriteHelper] file '{filePath}': NOT FOUND");
            return null;
        }

        try
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Plugin.Logger.Msg($"[SpriteHelper] file '{filePath}': OK, {fileData.Length} bytes");
            // ImageConversion.LoadImage throws MissingMethodException on this game's interop; see SpriteHelper.
            return SpriteHelper.LoadTextureFromBytes(fileData, Path.GetFileName(filePath));
        }
        catch (Exception e)
        {
            Plugin.Logger.Error($"Error loading texture from file: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Loads a texture from an asset bundle by its name.
    /// </summary>
    /// <param name="bundle">The asset bundle to load the texture from.</param>
    /// <param name="textureName">The name of the texture to be loaded from the bundle.</param>
    /// <returns>The loaded texture as a Texture2D object, or null if the texture cannot be loaded or an error occurs.</returns>
    [Obsolete("IL2CPP seems to have an issue with loading asset bundles. Use LoadTextureFromFile instead.")]
    public static Texture2D LoadTextureFromBundle(AssetBundle bundle, string textureName)
    {
        if (bundle == null)
        {
            Plugin.Logger.Error("Bundle is null");
            return null;
        }

        try
        {
            var texture = bundle.LoadAsset(textureName).Cast<Texture2D>();
            if (texture) return texture;
            Plugin.Logger.Error($"Failed to load texture '{textureName}' from bundle");
            return null;
        }
        catch (Exception e)
        {
            Plugin.Logger.Error($"Error loading texture from bundle: {e.Message}");
            return null;
        }
    }
}
