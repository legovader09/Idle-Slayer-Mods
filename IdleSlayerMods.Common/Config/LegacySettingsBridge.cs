using System.Reflection;
using MelonLoader;

namespace IdleSlayerMods.Common.Config;

/// <summary>
/// Keeps legacy <see cref="ModHelper.CreateSettingsToggle"/> callers working once a registry-driven
/// configuration UI is installed: the toggle shows up in that UI, and the mod's callback still fires.
/// </summary>
internal static class LegacySettingsBridge
{
    private static readonly HashSet<MelonPreferences_Entry> WiredEntries = new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// Ensures the entry is in the registry and applies the display name the mod asked for.
    /// </summary>
    internal static void Register(Assembly callingAssembly, string displayName, MelonPreferences_Entry<bool> entry)
    {
        if (entry == null) return;

        var info = ConfigRegistry.TryGetEntry(entry, out var existing)
            ? existing
            : ConfigRegistry.RegisterExternalEntry(callingAssembly, entry);

        info?.SetDisplayNameOverride(displayName);
    }

    /// <summary>
    /// Forwards value changes to the legacy onChanged callback. Subscribes once per entry, because
    /// CreateSettingsToggle is typically called from a component's Start every time the Game scene loads.
    /// </summary>
    internal static void WireCallback(MelonPreferences_Entry<bool> entry, Action<bool> onChanged)
    {
        if (entry == null || onChanged == null) return;

        lock (WiredEntries)
        {
            if (!WiredEntries.Add(entry)) return;
        }

        entry.OnEntryValueChanged.Subscribe((_, newValue) =>
        {
            try
            {
                onChanged(newValue);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.Error($"Settings callback for '{entry.Identifier}' threw: {ex}");
            }
        });
    }
}
