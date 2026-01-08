using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using HarmonyLib;
using Il2CppTMPro;

namespace CustomSettingsMod;

public class CustomSettingsMod : MonoBehaviour
{
    private GameObject _modSettingPopup;
    private PopupOptions _modSettingPopupOptions;

    private GameObject _modKeyBindingsPopup;
    private KeyBindsManager _modKeyBindingManager;
    private static Dictionary<string, MelonPreferences_Entry<KeyCode>> _modKeybinds;
    private static Dictionary<string, bool> _bindFirstLoad;

    private int settingIndex = 0;
    private int keyBindIndex = 0;

    private void Start()
    {
        CreateModPopup();
        CreateTitleHeader("Mod1");
        CreateSettingToggle("test toggle", Plugin.Config.TestBool, func);

        _modKeybinds = new Dictionary<string, MelonPreferences_Entry<KeyCode>>();
        _bindFirstLoad = new Dictionary<string, bool>();

        CreateKeyBindingsPopup();
        CreateKeyBindingContent("Test Key", Plugin.Config.TestKeyCode);
        CreateKeyBindingContent("Mod Settings Key", Plugin.Config.ModMenuKey);
    }

    private void func(bool val) { }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(Plugin.Config.ModMenuKey.Value))
            ToggleMenu();
        if (Input.GetKeyDown(KeyCode.F2))
            ToggleKeyBindings();

    }

    private void ToggleMenu()
    {
        if (_modSettingPopupOptions.IsVisible())
            _modSettingPopupOptions.Close(false);
        else
            _modSettingPopupOptions.Show();
    }

    private void ToggleKeyBindings()
    {
        if (_modKeyBindingManager.IsVisible())
            _modKeyBindingManager.Close();
        else
            _modKeyBindingManager.Show();
    }

    private void CreateKeyBindingsPopup()
    {
        var keyBindingPopup = GameObject.Find("UIManager/Popup Key Binds");
        if (keyBindingPopup == null)
        {
            Melon<Plugin>.Logger.Error("keyBindingPopup not found in scene.");
            return;
        }

        var originalKeyBindingsManager = keyBindingPopup.GetComponent<KeyBindsManager>();
        if (originalKeyBindingsManager == null)
        {
            Melon<Plugin>.Logger.Warning("KeyBindsManager component not found on keyBindingPopup object.");
        }

        _modKeyBindingsPopup = Instantiate(keyBindingPopup, keyBindingPopup.transform.parent);
        if (_modKeyBindingsPopup == null)
        {
            Melon<Plugin>.Logger.Error("Failed to instantiate _modKeyBindingsPopup");
            return;
        }

        _modKeyBindingsPopup.name = "Mod Key Bindings";

        var titleLabel = _modKeyBindingsPopup.transform.Find("Overlay/Panel/Title");
        if (titleLabel != null)
        {
            var fontSwitcher = titleLabel.GetComponent<FontSwitcher>();
            if (fontSwitcher != null) Destroy(fontSwitcher);

            var startingText = titleLabel.GetComponent<StartingText>();
            if (startingText != null) Destroy(startingText);

            var tmp = titleLabel.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = "Mod Key Bindings";
                tmp.m_text = "Mod Key Bindings";
            }
        }
        else
        {
            Melon<Plugin>.Logger.Warning("Label under Overlay/Panel not found in cloned popup.");
        }

        _modKeyBindingManager = _modKeyBindingsPopup.GetComponent<KeyBindsManager>();
        if (_modKeyBindingManager == null)
        {
            Melon<Plugin>.Logger.Warning("Cloned popup does not have _modKeyBindingManager");
        }

        _modKeyBindingsPopup.SetActive(true);
        Melon<Plugin>.Logger.Msg("Custom mod key bindings popup created successfully.");

        var content = _modKeyBindingsPopup.transform.Find("Overlay/Panel/Scroll View/Viewport/Content");
        if (content != null)
        {
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                var child = content.GetChild(i);
                Destroy(child.gameObject);
            }
        }
        else
        {
            Melon<Plugin>.Logger.Warning("Content container not found in cloned popup.");
        }

        var closeButton = _modKeyBindingsPopup.transform.Find("Overlay/Panel/Close Button");
        if (closeButton != null)
        {
            Destroy(closeButton.gameObject);
        }
        else
        {
            Melon<Plugin>.Logger.Warning("Close Button not found.");
        }
    }

    private void CreateKeyBindingContent(string contentName, MelonPreferences_Entry<KeyCode> configEntry)
    {
        if (_modKeyBindingsPopup == null)
        {
            Melon<Plugin>.Logger.Error("_modKeyBindingsPopup is null. Cannot create setting toggle.");
            return;
        }

        var content = _modKeyBindingsPopup.transform.Find("Overlay/Panel/Scroll View/Viewport/Content");
        if (content == null)
        {
            Melon<Plugin>.Logger.Warning("Content container not found in _modKeyBindingsPopup");
            return;
        }

        var originalTemplate = GameObject.Find("UIManager/Popup Key Binds/Overlay/Panel/Scroll View/Viewport/Content/BF Ability 2");
        if (originalTemplate == null)
        {
            Melon<Plugin>.Logger.Warning("Original toggle template (bf ability 2) not found.");
            return;
        }

        var keyBindObj = Instantiate(originalTemplate, content);

        string nameWMod = "[MOD]" + contentName;

        keyBindObj.name = nameWMod;
        keyBindObj.transform.SetSiblingIndex(keyBindIndex++);

        var keyBind = keyBindObj.GetComponent<KeyBind>();
        if (keyBind != null)
        {
            keyBind.keyBindID = nameWMod;
            keyBind.keyBindLabel = contentName;

            keyBind.currentKeyBind1 = configEntry.Value;
            keyBind.localizedActionName = contentName;
        }
        else
        {
            Melon<Plugin>.Logger.Warning($"KeyBind component not found on '{contentName}' toggle object.");
        }

        var button2 = keyBindObj.transform.Find("Button 2");
        if (button2 != null)
            button2.gameObject.SetActive(false);
        else
        {
            Melon<Plugin>.Logger.Msg("button2 not found");
        }

        var button3 = keyBindObj.transform.Find("Button 3");
        if (button3 != null)
            button3.gameObject.SetActive(false);
        else
        {
            Melon<Plugin>.Logger.Msg("button3 not found");
        }

        _modKeybinds.Add(nameWMod, configEntry);
        _bindFirstLoad.Add(nameWMod, true);
        keyBind.LoadKeyBinds();

        /*var label = keyBindObj.transform.Find("Label");
        if (label != null)
        {
            UnityEngine.Object.Destroy(label.GetComponent("FontSwitcher"));

            var textComponent = label.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = name;
                textComponent.m_text = name;
                Melon<Plugin>.Logger.Msg($"{textComponent.m_text}");
                Melon<Plugin>.Logger.Msg($"{keyBind.localizedActionName}");
            }
            else
            {
                Melon<Plugin>.Logger.Warning("text component not found");
            }
        }*/

        Melon<Plugin>.Logger.Msg($"Key Binding '{contentName}' added");
    }

    [HarmonyPatch(typeof(KeyBindsManager), "IsVisible")]
    public class Patch_KeyBindsManager_IsVisible
    {
        static bool Prefix(KeyBindsManager __instance)
        {
            if (__instance.name != "Mod Key Bindings") return true;
            
            var content = __instance.transform.Find("Overlay/Panel/Scroll View/Viewport/Content");
            for (var i = 0; i < content.childCount; i++)
            {
                var child = content.GetChild(i);
                var label = child.Find("Label");
                if (label == null) continue;
                
                Destroy(label.GetComponent<FontSwitcher>());

                var textComponent = label.GetComponent<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    var name = child.name[5..];
                    textComponent.text = name;
                    textComponent.m_text = name;
                }
                else
                {
                    Melon<Plugin>.Logger.Warning("text component not found");
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KeyBind), "LoadKeyBinds")]
    public class Patch_KeyBind_LoadKeyBinds
    {
        static void Postfix(KeyBind __instance)
        {
            //Melon<Plugin>.Logger.Msg("LoadKeyBinds");
            if (!__instance.name.StartsWith("[MOD]")) return;
            
            Melon<Plugin>.Logger.Msg("[MOD] located");
            if (_modKeybinds.TryGetValue(__instance.name, out var entry))
            {
                Melon<Plugin>.Logger.Msg("entry value " + entry.Value);
                Melon<Plugin>.Logger.Msg("CurrentKeyBind1 " + __instance.currentKeyBind1);
                if (_bindFirstLoad[__instance.name])
                {
                    __instance.currentKeyBind1 = entry.Value;
                    _bindFirstLoad[__instance.name] = false;
                } else
                {
                    entry.Value = __instance.currentKeyBind1;
                }

                Melon<Plugin>.Logger.Msg("Changed to " + entry.Value);
                __instance.SetKeyButton1(entry.Value);
            }
            else
            {
                Melon<Plugin>.Logger.Warning("Key not found in keybinds dictionary.");
            }
        }
    }

    private void CreateModPopup()
    {
        var optionsPopup = GameObject.Find("UIManager/Popup Options");
        if (optionsPopup == null)
        {
            Melon<Plugin>.Logger.Error("Popup Options not found in scene.");
            return;
        }

        var originalPopupOptions = optionsPopup.GetComponent<PopupOptions>();
        if (originalPopupOptions == null)
        {
            Melon<Plugin>.Logger.Warning("Original PopupOptions component not found on Popup Options object.");
        }

        _modSettingPopup = Instantiate(optionsPopup, optionsPopup.transform.parent);
        if (_modSettingPopup == null)
        {
            Melon<Plugin>.Logger.Error("Failed to instantiate mod popup.");
            return;
        }

        _modSettingPopup.name = "Custom Mod Settings";

        var titleLabel = _modSettingPopup.transform.Find("Overlay/Panel/Title");
        if (titleLabel != null)
        {
            var fontSwitcher = titleLabel.GetComponent<FontSwitcher>();
            if (fontSwitcher != null) Destroy(fontSwitcher);

            var startingText = titleLabel.GetComponent<StartingText>();
            if (startingText != null) Destroy(startingText);

            var tmp = titleLabel.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = "Mod Settings";
                tmp.m_text = "Mod Settings";
            }
        }
        else
        {
            Melon<Plugin>.Logger.Warning("Label under Overlay/Panel not found in cloned popup.");
        }

        _modSettingPopupOptions = _modSettingPopup.GetComponent<PopupOptions>();
        if (_modSettingPopupOptions == null)
        {
            Melon<Plugin>.Logger.Warning("Cloned popup does not have PopupOptions component.");
        }

        _modSettingPopup.SetActive(true);
        Melon<Plugin>.Logger.Msg("Custom mod settings popup created successfully.");

        var content = _modSettingPopup.transform.Find("Overlay/Panel/Scroll View/Viewport/Content");
        if (content != null)
        {
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                var child = content.GetChild(i);
                Destroy(child.gameObject);
            }
        }
        else
        {
            Melon<Plugin>.Logger.Warning("Content container not found in cloned popup.");
        }

        var confirmButton = _modSettingPopup.transform.Find("Overlay/Panel/Confirm Button");
        if (confirmButton != null)
        {
            Destroy(confirmButton.gameObject);
        }
        else
        {
            Melon<Plugin>.Logger.Warning("Confirm Button not found.");
        }

        var cancelButton = _modSettingPopup.transform.Find("Overlay/Panel/Cancel Button");
        if (cancelButton != null)
        {
            Destroy(cancelButton.gameObject);
        }
        else
        {
            Melon<Plugin>.Logger.Warning("Cancel Button not found.");
        }
    }

    private void CreateTitleHeader(string titleText)
    {
        var sourceTitle = GameObject.Find("UIManager/Popup Options/Overlay/Panel/Scroll View/Viewport/Content/Screen Title");
        if (sourceTitle == null)
        {
            Melon<Plugin>.Logger.Warning("Screen Title template not found.");
            return;
        }

        var targetContent = _modSettingPopup.transform.Find("Overlay/Panel/Scroll View/Viewport/Content");
        if (targetContent == null)
        {
            Melon<Plugin>.Logger.Warning("Target content for mod settings not found.");
            return;
        }

        var newTitle = Instantiate(sourceTitle, targetContent);
        newTitle.name = titleText;
        newTitle.transform.SetSiblingIndex(settingIndex++);

        var label = newTitle.transform.Find("Label");
        if (label != null)
        {
            Destroy(label.GetComponent<StartingText>());
            Destroy(label.GetComponent<FontSwitcher>());

            var textComponent = label.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = titleText;
                textComponent.m_text = titleText;
            }
        }

        Melon<Plugin>.Logger.Msg($"Title header '{titleText}' added to mod popup.");
    }


    private void CreateSettingToggle(string settingsName, MelonPreferences_Entry<bool> configEntry, Action<bool> onChanged)
    {
        if (_modSettingPopup == null)
        {
            Melon<Plugin>.Logger.Error("Mod popup is null. Cannot create setting toggle.");
            return;
        }

        var content = _modSettingPopup.transform.Find("Overlay/Panel/Scroll View/Viewport/Content");
        if (content == null)
        {
            Melon<Plugin>.Logger.Warning("Content container not found in mod popup.");
            return;
        }

        var originalTemplate = GameObject.Find("UIManager/Popup Options/Overlay/Panel/Scroll View/Viewport/Content/Confirm Portal");
        if (originalTemplate == null)
        {
            Melon<Plugin>.Logger.Warning("Original toggle template (Confirm Portal) not found.");
            return;
        }

        var toggleObj = Instantiate(originalTemplate, content);
        toggleObj.name = settingsName;
        toggleObj.transform.SetSiblingIndex(settingIndex++);

        var label = toggleObj.transform.Find("Label");
        if (label != null)
        {
            Destroy(label.GetComponent<StartingText>());
            Destroy(label.GetComponent<FontSwitcher>());

            var textComponent = label.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = settingsName;
                textComponent.m_text = settingsName;
            }
        }

        var toggle = toggleObj.GetComponent<Toggle>();
        if (toggle != null)
        {
            toggle.isOn = configEntry.Value;

            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener((UnityAction<bool>)(isEnabled =>
            {
                OnToggleChanged(isEnabled, configEntry, onChanged);
            }));
        }
        else
        {
            Melon<Plugin>.Logger.Warning($"Toggle component not found on '{settingsName}' toggle object.");
        }

        Melon<Plugin>.Logger.Msg($"Toggle '{settingsName}' added to mod settings.");
    }


    private void OnToggleChanged(bool isEnabled, MelonPreferences_Entry<bool> configEntry, Action<bool> onChanged)
    {
        configEntry.Value = isEnabled;
        onChanged?.Invoke(isEnabled);
    }
    
    [HarmonyPatch(typeof(PopupOptions), "ReloadValues")]
    public class Patch_PopupOptions_ReloadValues
    {
        static bool Prefix(PopupOptions __instance)
        {
            return __instance.name != "Custom Mod Settings";
        }
    }
}
