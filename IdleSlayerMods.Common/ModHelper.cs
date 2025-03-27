using IdleSlayerMods.Common.Constants;
using IdleSlayerMods.Common.Helpers;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UIElements.Button;
using File = Il2CppSystem.IO.File;
using Path = Il2CppSystem.IO.Path;

namespace IdleSlayerMods.Common;

public class ModHelper : MonoBehaviour
{
    /// <summary>
    /// Event that occurs once the ModHelper has been added to the game scene and is ready to use.
    /// </summary>
    public static event Action<ModHelper> ModHelperMounted;
    private AchievementManager _achievementManager;
    private NotificationText _notificationText;
    private Popup _popup;
    private Transform _infoPanelButtonsContainer;
    private GameObject _templateButton;
    
    private void OnModHelperMounted()
    {
        ModHelperMounted?.Invoke(this);
        Melon<Plugin>.Logger.Msg("ModHelper mounted successfully");
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
    public void AddPanelButton(string text, Action clickAction = null, Texture2D icon = null)
    {
        Melon<Plugin>.Logger.Msg($"Registering panel button: {text}");
        var button = Instantiate(_templateButton, _infoPanelButtonsContainer?.transform);
        if (button is null)
        {
            Melon<Plugin>.Logger.Warning("Unable to instantiate template panel button");
            return;
        }
        
        button.name = text;

        // Prevents StartingText from overwriting TMP text
        var startingText = button.transform.GetChild(1).GetComponent<StartingText>();
        if (startingText) DestroyImmediate(startingText);

        // Get rid of duplicated component's image and add rawimage to allow custom texture loading
        var buttonImage = button.transform.GetChild(0).gameObject;
        DestroyImmediate(buttonImage.GetComponent<Image>());
        var rawImage = buttonImage.AddComponent<RawImage>();
        
        // Update TMP text
        var tmp = button.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.m_text = text; 
        
        // Add event listener
        button.GetComponent<Button>().add_onClick(clickAction);
        icon ??= Texture2D.redTexture;
        rawImage.texture = icon;
        
        Melon<Plugin>.Logger.Msg($"Button {text} added successfully");
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
        Melon<Plugin>.Logger.Msg("Popup found");
        var panel = _popup.transform.GetChild(0).GetChild(0);
        if (!panel) return;
        Melon<Plugin>.Logger.Msg("Panel found");
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
        Melon<Plugin>.Logger.Msg($"Attempting to show notification with message: {message}");
        _notificationText?.Show(message, shine);
    }
    
    [Obsolete("IL2CPP seems to have an issue with loading asset bundles. ")]
    public static AssetBundle LoadBundle(string bundlePath)
    {
        try
        {
            var fullPath = Path.Combine(bundlePath);
            var bundle = AssetBundle.LoadFromFile(fullPath);
            if (bundle != null) return bundle;
            Melon<Plugin>.Logger.Error($"Failed to load asset bundle from {fullPath}");
            return null;
        }
        catch (Exception e)
        {
            Melon<Plugin>.Logger.Error($"Error loading asset bundle: {e.Message}");
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
        if (!File.Exists(filePath)) return null;

        try
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            var il2CPPData = new Il2CppStructArray<byte>(fileData);
            
            if (ImageConversion.LoadImage(texture, il2CPPData))
            {
                texture.Apply(true, false);
                return texture;
            }
            
            Melon<Plugin>.Logger.Error($"Failed to load texture from file: {filePath}");
            Destroy(texture);
            return null;
        }
        catch (Exception e)
        {
            Melon<Plugin>.Logger.Error($"Error loading texture from file: {e.Message}");
            return null;
        }
    }

    [Obsolete("IL2CPP seems to have an issue with loading asset bundles. Use LoadTextureFromFile instead.")]
    public static Texture2D LoadTextureFromBundle(AssetBundle bundle, string textureName)
    {
        if (bundle == null)
        {
            Melon<Plugin>.Logger.Error("Bundle is null");
            return null;
        }

        try
        {
            var texture = bundle.LoadAsset(textureName).Cast<Texture2D>();
            if (texture) return texture;
            Melon<Plugin>.Logger.Error($"Failed to load texture '{textureName}' from bundle");
            return null;
        }
        catch (Exception e)
        {
            Melon<Plugin>.Logger.Error($"Error loading texture from bundle: {e.Message}");
            return null;
        }
    }
}