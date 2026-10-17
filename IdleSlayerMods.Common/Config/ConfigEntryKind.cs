namespace IdleSlayerMods.Common.Config;

/// <summary>
/// Describes how a configuration entry can be presented by a configuration UI.
/// </summary>
public enum ConfigEntryKind
{
    /// <summary>
    /// The entry's value type has no generated UI control (string, numeric, list or custom types).
    /// </summary>
    Unsupported = 0,

    /// <summary>
    /// A <see cref="bool"/> entry, presented as an on/off toggle.
    /// </summary>
    Toggle = 1,

    /// <summary>
    /// A <see cref="UnityEngine.KeyCode"/> entry, presented as a key binding control.
    /// </summary>
    KeyBind = 2,
}
