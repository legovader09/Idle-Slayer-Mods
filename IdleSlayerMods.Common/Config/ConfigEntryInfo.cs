using MelonLoader;
using UnityEngine;

namespace IdleSlayerMods.Common.Config;

/// <summary>
/// Read-only metadata describing a single configuration entry that was bound through <see cref="BaseConfig"/>.
/// Obtain instances from <see cref="ConfigRegistry"/>.
/// </summary>
public sealed class ConfigEntryInfo
{
    private string _displayNameOverride;

    internal ConfigEntryInfo(ModConfigInfo owner, MelonPreferences_Entry entry, Type valueType)
    {
        Owner = owner;
        Entry = entry;
        ValueType = valueType;
        Kind = Classify(valueType);
    }

    /// <summary>
    /// The mod configuration this entry belongs to.
    /// </summary>
    public ModConfigInfo Owner { get; }

    /// <summary>
    /// The underlying MelonPreferences entry.
    /// </summary>
    public MelonPreferences_Entry Entry { get; }

    /// <summary>
    /// The CLR type of the entry's value.
    /// </summary>
    public Type ValueType { get; }

    /// <summary>
    /// How a configuration UI should present this entry.
    /// </summary>
    public ConfigEntryKind Kind { get; }

    /// <summary>
    /// True when a configuration UI is able to generate a control for this entry.
    /// </summary>
    public bool IsSupported => Kind != ConfigEntryKind.Unsupported;

    /// <summary>
    /// The section (TOML table / MelonPreferences category) the entry is stored under.
    /// </summary>
    public string Section => Entry.Category?.Identifier;

    /// <summary>
    /// The key the entry is stored under.
    /// </summary>
    public string Key => Entry.Identifier;

    /// <summary>
    /// The name to show in a configuration UI.
    /// </summary>
    public string DisplayName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_displayNameOverride)) return _displayNameOverride;
            return string.IsNullOrWhiteSpace(Entry.DisplayName) ? Entry.Identifier : Entry.DisplayName;
        }
    }

    /// <summary>
    /// The entry's description, as passed to Bind.
    /// </summary>
    public string Description => Entry.Description;

    /// <summary>
    /// True when the entry should not be shown in configuration UIs.
    /// </summary>
    public bool IsHidden => HiddenByMod || Entry.IsHidden;

    internal bool HiddenByMod { get; set; }

    internal void SetDisplayNameOverride(string displayName)
    {
        if (!string.IsNullOrWhiteSpace(displayName)) _displayNameOverride = displayName;
    }

    /// <summary>
    /// Returns the entry as a bool entry, or null if it is not a toggle.
    /// </summary>
    public MelonPreferences_Entry<bool> AsToggle() => Entry as MelonPreferences_Entry<bool>;

    /// <summary>
    /// Returns the entry as a KeyCode entry, or null if it is not a key binding.
    /// </summary>
    public MelonPreferences_Entry<KeyCode> AsKeyBind() => Entry as MelonPreferences_Entry<KeyCode>;

    /// <summary>
    /// Saves the entry's category to its config file.
    /// </summary>
    public void Save()
    {
        try
        {
            Entry.Category?.SaveToFile(false);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.Error($"Failed to save config entry '{Key}' in section '{Section}': {ex.Message}");
        }
    }

    internal static ConfigEntryKind Classify(Type valueType)
    {
        if (valueType == typeof(bool)) return ConfigEntryKind.Toggle;
        if (valueType == typeof(KeyCode)) return ConfigEntryKind.KeyBind;
        return ConfigEntryKind.Unsupported;
    }

    /// <inheritdoc />
    public override string ToString() => $"{Owner?.ConfigName}/{Section}/{Key} ({Kind})";
}
