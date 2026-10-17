using System;
using System.Collections.Generic;
using System.Linq;
using IdleSlayerMods.Common.Config;
using IdleSlayerMods.Common.Extensions;
using Il2Cpp;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace IdleConfig.UI;

/// <summary>
/// The IdleConfig settings window. Built by cloning the game's Options popup and filling it with
/// controls generated from <see cref="ConfigRegistry"/>: a heading per mod, then its toggles, then its key binds.
/// </summary>
internal sealed class ConfigWindow
{
    internal const string PopupName = "IdleConfig Mod Settings";
    private const string WindowTitle = "Mod Settings";

    private const string OptionsPopupPath = "UIManager/Popup Options";
    private const string KeyBindsPopupPath = "UIManager/Popup Key Binds";
    private const string ContentPath = "Overlay/Panel/Scroll View/Viewport/Content";
    private const string TitlePath = "Overlay/Panel/Title";
    private const string ConfirmButtonPath = "Overlay/Panel/Confirm Button";
    private const string CancelButtonPath = "Overlay/Panel/Cancel Button";
    private const string ToggleTemplateName = "Confirm Portal";
    private const string HeaderTemplateName = "Screen Title";
    private const string KeyBindTemplateName = "BF Ability 2";

    private const string CapturePrompt = "Press a key...";
    private const string UnboundText = "-";

    // Mouse buttons are excluded so the click that starts a capture can't immediately become the binding.
    private static readonly KeyCode[] CapturableKeys = Enum.GetValues(typeof(KeyCode))
        .Cast<KeyCode>()
        .Distinct()
        .Where(k => k != KeyCode.None && k is < KeyCode.Mouse0 or > KeyCode.Mouse6)
        .ToArray();

    private readonly List<ToggleRow> _toggles = new();
    private readonly List<KeyBindRow> _keyBinds = new();

    private GameObject _popup;
    private PopupOptions _popupOptions;
    private Transform _content;
    private Transform _staging;
    private GameObject _toggleTemplate;
    private GameObject _headerTemplate;
    private GameObject _keyBindTemplate;

    private int _builtVersion = -1;
    private bool _suppressToggleEvents;
    private bool _loggedKeyBindLayout;
    private KeyBindRow _capturing;
    private int _captureStartFrame;

    private ConfigWindow() { }

    internal static ConfigWindow Create()
    {
        var window = new ConfigWindow();
        return window.Initialise() ? window : null;
    }

    internal void Toggle()
    {
        if (IsVisible) Close();
        else Show();
    }

    /// <summary>
    /// Called every frame by the IdleConfig behaviour. Drives key capture.
    /// </summary>
    internal void Tick()
    {
        if (_capturing == null) return;

        if (!IsVisible)
        {
            CancelCapture();
            return;
        }

        // Skip the frame the capture started on.
        if (Time.frameCount <= _captureStartFrame) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelCapture();
            return;
        }

        foreach (var key in CapturableKeys)
        {
            if (!Input.GetKeyDown(key)) continue;
            CompleteCapture(key);
            return;
        }
    }

    internal void Dispose()
    {
        CancelCapture();
        if (_popup != null) Object.Destroy(_popup);
        _popup = null;
    }

    private bool IsVisible
    {
        get
        {
            if (!_popup) return false;
            return _popupOptions ? _popupOptions.IsVisible() : _popup.activeSelf;
        }
    }

    private void Show()
    {
        if (!_popup) return;

        if (_builtVersion != ConfigRegistry.Version) Rebuild();

        if (_popupOptions) _popupOptions.Show();
        else _popup.SetActive(true);

        // Re-wired on every show: PopupOptions may add its own listeners to the button during Start.
        WireCloseButton();
        RefreshValues();
    }

    private void Close()
    {
        CancelCapture();
        if (!_popup) return;

        if (_popupOptions) _popupOptions.Close(false);
        else _popup.SetActive(false);
    }

    private bool Initialise()
    {
        var optionsPopup = GameObject.Find(OptionsPopupPath);
        if (!optionsPopup)
        {
            Plugin.Logger.Error($"'{OptionsPopupPath}' not found; IdleConfig window cannot be created.");
            return false;
        }

        var optionsContent = optionsPopup.transform.Find(ContentPath);
        _toggleTemplate = optionsContent?.Find(ToggleTemplateName)?.gameObject;
        _headerTemplate = optionsContent?.Find(HeaderTemplateName)?.gameObject;
        if (!_toggleTemplate) Plugin.Logger.Warning($"Toggle template '{ToggleTemplateName}' not found; toggles will be skipped.");
        if (!_headerTemplate) Plugin.Logger.Warning($"Heading template '{HeaderTemplateName}' not found; headings will be skipped.");

        _keyBindTemplate = FindKeyBindTemplate();
        if (!_keyBindTemplate) Plugin.Logger.Warning("Key bind template not found; key binds will be skipped.");

        _popup = Object.Instantiate(optionsPopup, optionsPopup.transform.parent);
        _popup.name = PopupName;
        _popupOptions = _popup.GetComponent<PopupOptions>();

        var title = _popup.transform.Find(TitlePath);
        if (title != null) UiText.Set(title, WindowTitle);

        _content = _popup.transform.Find(ContentPath);
        if (_content == null)
        {
            Plugin.Logger.Error("Content container not found in cloned options popup.");
            Object.Destroy(_popup);
            _popup = null;
            return false;
        }

        ClearContent();

        var confirm = _popup.transform.Find(ConfirmButtonPath);
        if (confirm) Object.Destroy(confirm.gameObject);

        var cancel = _popup.transform.Find(CancelButtonPath);
        if (cancel) UiText.SetFirstChildText(cancel, "Close");

        // Inactive holder used to instantiate rows without running their Awake.
        var staging = new GameObject("IdleConfig Staging");
        staging.SetActive(false);
        staging.transform.SetParent(_popup.transform, false);
        _staging = staging.transform;

        _popup.SetActive(true);
        Plugin.Logger.Msg("IdleConfig window created.");
        return true;
    }

    private static GameObject FindKeyBindTemplate()
    {
        var keyBindsPopup = GameObject.Find(KeyBindsPopupPath);
        var content = keyBindsPopup?.transform.Find(ContentPath);
        if (!content) return null;

        var named = content.Find(KeyBindTemplateName);
        if (named) return named.gameObject;

        for (var i = 0; i < content.childCount; i++)
        {
            var child = content.GetChild(i);
            if (child.GetComponent<KeyBind>() != null) return child.gameObject;
        }
        return null;
    }

    private void WireCloseButton()
    {
        var cancel = _popup.transform.Find(CancelButtonPath);
        var button = cancel?.GetComponent<Button>();
        if (!button) return;

        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener((UnityAction)(() => Close()));
    }

    private void ClearContent()
    {
        for (var i = _content.childCount - 1; i >= 0; i--)
        {
            var child = _content.GetChild(i).gameObject;
            child.SetActive(false);
            Object.Destroy(child);
        }
    }

    private void Rebuild()
    {
        CancelCapture();
        ClearContent();
        _toggles.Clear();
        _keyBinds.Clear();

        var mods = ConfigRegistry.Configs
            .Where(c => c.HasVisibleEntries)
            .OrderBy(c => c.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (mods.Count == 0) AddHeader("No mod settings found");

        foreach (var mod in mods)
        {
            AddHeader(mod.DisplayName);

            var entries = mod.VisibleEntries.ToList();
            foreach (var entry in entries.Where(e => e.Kind == ConfigEntryKind.Toggle)) AddToggle(entry);
            foreach (var entry in entries.Where(e => e.Kind == ConfigEntryKind.KeyBind)) AddKeyBind(entry);
        }

        _builtVersion = ConfigRegistry.Version;
        Plugin.Logger.Msg($"IdleConfig built {_toggles.Count} toggle(s) and {_keyBinds.Count} key bind(s) across {mods.Count} mod(s).");
    }

    private void RefreshValues()
    {
        _suppressToggleEvents = true;
        try
        {
            foreach (var row in _toggles) row.Refresh();
        }
        finally
        {
            _suppressToggleEvents = false;
        }

        foreach (var row in _keyBinds) row.Refresh();
    }

    private void AddHeader(string text)
    {
        if (!_headerTemplate) return;

        var header = Object.Instantiate(_headerTemplate, _content);
        header.name = $"[IdleConfig] {text}";
        var label = header.transform.Find("Label");
        if (label) UiText.Set(label, text);
    }

    // Toggles follow the proven CustomSettingsMod / CreateSettingsToggle pattern:
    // clone "Confirm Portal" directly and replace runtime listeners.
    private void AddToggle(ConfigEntryInfo entry)
    {
        if (!_toggleTemplate) return;
        var typed = entry.AsToggle();
        if (typed == null) return;

        var row = Object.Instantiate(_toggleTemplate, _content);
        row.name = RowName(entry);

        var label = row.transform.Find("Label");
        if (label) UiText.Set(label, entry.DisplayName);

        var toggle = row.GetComponentInChildren<Toggle>(true);
        if (!toggle)
        {
            Plugin.Logger.Warning($"Toggle component not found for '{entry}'.");
            Object.Destroy(row);
            return;
        }

        toggle.onValueChanged.RemoveAllListeners();
        toggle.isOn = typed.Value;
        toggle.onValueChanged.AddListener((UnityAction<bool>)(value => OnToggleChanged(entry, value)));

        _toggles.Add(new ToggleRow(entry, toggle));
    }

    private void OnToggleChanged(ConfigEntryInfo entry, bool value)
    {
        if (_suppressToggleEvents) return;

        var typed = entry.AsToggle();
        if (typed == null || typed.Value == value) return;

        typed.Value = value;
        entry.Save();
    }

    // Key binds reuse the game's key bind row. As in CustomSettingsMod, the displayed key is rendered by the
    // game's own KeyBind.SetKeyButton1(KeyCode), which writes to the KeyBind's serialized keyBind1Text.
    // The KeyBind is kept but disabled before the row ever activates, so its Start() -> LoadKeyBinds() never
    // runs: that is what reset the PoC's rows to the template's saved key ("X") and needed the Harmony patches.
    // Its keyBindID is also renamed so nothing it does can touch the real "BF Ability 2" binding.
    private void AddKeyBind(ConfigEntryInfo entry)
    {
        if (_keyBindTemplate == null) return;
        if (entry.AsKeyBind() == null) return;

        var row = Object.Instantiate(_keyBindTemplate, _staging);
        row.name = RowName(entry);

        var keyBind = row.GetComponent<KeyBind>();
        if (!keyBind)
        {
            Plugin.Logger.Warning($"KeyBind component not found on key bind template; skipping '{entry}'.");
            Object.Destroy(row);
            return;
        }

        keyBind.enabled = false;
        keyBind.keyBindID = $"IdleConfig:{entry.Owner.ConfigName}/{entry.Section}/{entry.Key}";
        keyBind.keyBindLabel = entry.DisplayName;
        keyBind.localizedActionName = entry.DisplayName;

        if (!_loggedKeyBindLayout)
        {
            LogKeyBindLayout(keyBind);
            _loggedKeyBindLayout = true;
        }

        if (keyBind.button2) keyBind.button2.gameObject.SetActive(false);
        else SetChildActive(row.transform, "Button 2", false);
        if (keyBind.button3) keyBind.button3.gameObject.SetActive(false);
        else SetChildActive(row.transform, "Button 3", false);

        var label = row.transform.Find("Label");
        if (label) UiText.Set(label, entry.DisplayName);

        var button = keyBind.button1 ?? row.transform.Find("Button 1")?.GetComponent<Button>();
        if (!button)
        {
            Plugin.Logger.Warning($"Key bind button not found for '{entry}'.");
            Object.Destroy(row);
            return;
        }

        var keyRow = new KeyBindRow(entry, keyBind);
        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener((UnityAction)(() => BeginCapture(keyRow)));

        row.transform.SetParent(_content, false);
        _keyBinds.Add(keyRow);
    }

    // One-off diagnostic: shows which text the game uses for the key, versus the first text found under
    // the button (what the previous implementation wrote to).
    private static void LogKeyBindLayout(KeyBind keyBind)
    {
        try
        {
            var gameText = keyBind.keyBind1Text;
            var button = keyBind.button1;
            var firstText = button ? button.GetComponentInChildren<TextMeshProUGUI>(true) : null;
            Plugin.Logger.Debug(
                $"Key bind template: button1='{PathOf(button?.transform, keyBind.transform)}', " +
                $"keyBind1Text='{PathOf(gameText?.transform, keyBind.transform)}' (shows '{gameText?.text}'), " +
                $"first text under button1='{PathOf(firstText?.transform, keyBind.transform)}' " +
                $"(shows '{firstText?.text}', active={firstText && firstText.gameObject.activeSelf}), " +
                $"same object={gameText && firstText && gameText.Pointer == firstText.Pointer}");
        }
        catch (Exception ex)
        {
            Plugin.Logger.Warning($"Could not log key bind layout: {ex.Message}");
        }
    }

    private static string PathOf(Transform target, Transform root)
    {
        if (!target) return "<none>";
        var path = target.name;
        for (var t = target.parent; t != null && t != root; t = t.parent) path = $"{t.name}/{path}";
        return path;
    }

    private void BeginCapture(KeyBindRow row)
    {
        if (_capturing != null && _capturing != row) CancelCapture();

        _capturing = row;
        _captureStartFrame = Time.frameCount;
        row.SetText(CapturePrompt);
    }

    private void CompleteCapture(KeyCode key)
    {
        var row = _capturing;
        _capturing = null;
        if (row == null) return;

        var typed = row.Entry.AsKeyBind();
        if (typed != null && typed.Value != key)
        {
            typed.Value = key;
            row.Entry.Save();
            WarnOnConflicts(row.Entry, key);
        }
        row.Refresh();
        Plugin.Logger.Debug($"Key bind '{row.Entry.Owner.DisplayName}: {row.Entry.DisplayName}' -> config value {typed?.Value}, " +
                          $"displayed '{row.DisplayedText}'.");
    }

    private void CancelCapture()
    {
        var row = _capturing;
        _capturing = null;
        row?.Refresh();
    }

    private static void WarnOnConflicts(ConfigEntryInfo changed, KeyCode key)
    {
        var clashes = ConfigRegistry.GetEntries(ConfigEntryKind.KeyBind)
            .Where(e => e != changed && e.AsKeyBind()?.Value == key)
            .Select(e => $"{e.Owner.DisplayName}: {e.DisplayName}")
            .ToList();

        if (clashes.Count > 0)
            Plugin.Logger.Warning($"{key} is also bound to {string.Join(", ", clashes)}.");
    }

    private static string RowName(ConfigEntryInfo entry) => $"[IdleConfig] {entry.Owner.ConfigName}/{entry.Section}/{entry.Key}";

    private static void SetChildActive(Transform parent, string name, bool active)
    {
        var child = parent.Find(name);
        if (child) child.gameObject.SetActive(active);
    }

    private sealed class ToggleRow
    {
        private readonly ConfigEntryInfo _entry;
        private readonly Toggle _toggle;

        internal ToggleRow(ConfigEntryInfo entry, Toggle toggle)
        {
            _entry = entry;
            _toggle = toggle;
        }

        internal void Refresh()
        {
            var typed = _entry.AsToggle();
            if (typed == null || !_toggle || _toggle.isOn == typed.Value) return;
            _toggle.isOn = typed.Value;
        }
    }

    private sealed class KeyBindRow
    {
        private readonly KeyBind _keyBind;

        internal KeyBindRow(ConfigEntryInfo entry, KeyBind keyBind)
        {
            Entry = entry;
            _keyBind = keyBind;
            Refresh();
        }

        internal ConfigEntryInfo Entry { get; }

        internal string DisplayedText => _keyBind?.keyBind1Text?.text;

        // Same calls CustomSettingsMod made: set currentKeyBind1, then let the game render it.
        internal void Refresh()
        {
            var typed = Entry.AsKeyBind();
            if (typed == null || !_keyBind) return;

            try
            {
                _keyBind.currentKeyBind1 = typed.Value;
                _keyBind.SetKeyButton1(typed.Value);
            }
            catch (Exception ex)
            {
                Plugin.Logger.Warning($"SetKeyButton1 failed for '{Entry}': {ex.Message}; writing the key name directly.");
                SetText(typed.Value == KeyCode.None ? UnboundText : typed.Value.ToString());
            }
        }

        // Only used for the transient "Press a key..." prompt (and as a fallback).
        internal void SetText(string text)
        {
            var tmp = _keyBind?.keyBind1Text;
            if (!tmp) return;
            tmp.text = text;
            tmp.m_text = text;
        }
    }
}
