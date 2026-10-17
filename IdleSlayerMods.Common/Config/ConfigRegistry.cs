using System.Reflection;
using MelonLoader;
using MelonLoader.Utils;

namespace IdleSlayerMods.Common.Config;

/// <summary>
/// Central registry of every configuration created through <see cref="BaseConfig"/>.
/// <para>
/// Configuration UIs (such as IdleConfig) read from this registry to generate controls automatically,
/// so mods do not need to create UI elements themselves.
/// </para>
/// </summary>
public static class ConfigRegistry
{
    internal static readonly object SyncRoot = new();

    private static readonly List<ModConfigInfo> ConfigList = new();
    private static readonly Dictionary<string, ModConfigInfo> ConfigsByPath = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<MelonPreferences_Entry, ConfigEntryInfo> EntryLookup = new(ReferenceEqualityComparer.Instance);
    private static readonly HashSet<MelonPreferences_Entry> PendingHidden = new(ReferenceEqualityComparer.Instance);
    private static readonly Dictionary<string, string> SectionOwners = new(StringComparer.Ordinal);
    private static readonly HashSet<string> UIProviders = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Raised after a new entry has been registered. May be raised on any thread that binds config.
    /// </summary>
    public static event Action<ConfigEntryInfo> EntryRegistered;

    /// <summary>
    /// Incremented whenever the set of entries (or their visibility) changes.
    /// UIs can compare this to a cached value to decide whether to rebuild.
    /// </summary>
    public static int Version { get; private set; }

    /// <summary>
    /// A snapshot of all registered mod configurations, in registration order.
    /// </summary>
    public static IReadOnlyList<ModConfigInfo> Configs
    {
        get
        {
            lock (SyncRoot) return ConfigList.ToArray();
        }
    }

    /// <summary>
    /// A snapshot of every registered entry across all mods.
    /// </summary>
    public static IEnumerable<ConfigEntryInfo> GetEntries() => Configs.SelectMany(c => c.Entries);

    /// <summary>
    /// A snapshot of every registered entry of the given kind across all mods.
    /// </summary>
    public static IEnumerable<ConfigEntryInfo> GetEntries(ConfigEntryKind kind) => GetEntries().Where(e => e.Kind == kind);

    /// <summary>
    /// Looks up the registry metadata for a MelonPreferences entry.
    /// </summary>
    public static bool TryGetEntry(MelonPreferences_Entry entry, out ConfigEntryInfo info)
    {
        lock (SyncRoot)
        {
            if (entry != null) return EntryLookup.TryGetValue(entry, out info);
            info = null;
            return false;
        }
    }

    /// <summary>
    /// True when a mod has announced that it renders configuration UI from this registry.
    /// When true, legacy UI helpers such as <see cref="ModHelper.CreateSettingsToggle"/> stop injecting
    /// controls into the vanilla options menu to avoid duplicates.
    /// </summary>
    public static bool HasUIProvider
    {
        get
        {
            lock (SyncRoot) return UIProviders.Count > 0;
        }
    }

    /// <summary>
    /// Announces that a mod renders configuration UI from this registry.
    /// Call this from <c>OnInitializeMelon</c>.
    /// </summary>
    /// <param name="providerName">A name identifying the UI provider, used for logging.</param>
    public static void RegisterUIProvider(string providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName)) providerName = "Unknown";
        lock (SyncRoot)
        {
            if (!UIProviders.Add(providerName)) return;
        }
        Plugin.Logger?.Msg($"Config UI provider registered: {providerName}");
    }

    /// <summary>
    /// Hides or shows an entry in configuration UIs. Can be called before or after the entry is bound.
    /// </summary>
    public static void SetHidden(MelonPreferences_Entry entry, bool hidden)
    {
        if (entry == null) return;
        lock (SyncRoot)
        {
            if (EntryLookup.TryGetValue(entry, out var info))
            {
                if (info.HiddenByMod == hidden) return;
                info.HiddenByMod = hidden;
                Version++;
                return;
            }

            if (hidden) PendingHidden.Add(entry);
            else PendingHidden.Remove(entry);
        }
    }

    internal static ModConfigInfo RegisterConfig(string configName, string filePath, Assembly assembly)
    {
        lock (SyncRoot)
        {
            if (ConfigsByPath.TryGetValue(filePath, out var existing)) return existing;

            var info = new ModConfigInfo(configName, filePath, assembly);
            ConfigsByPath[filePath] = info;
            ConfigList.Add(info);
            Version++;
            return info;
        }
    }

    internal static ConfigEntryInfo RegisterEntry<T>(ModConfigInfo owner, MelonPreferences_Entry<T> entry)
    {
        if (owner == null || entry == null) return null;

        ConfigEntryInfo info;
        lock (SyncRoot)
        {
            if (EntryLookup.TryGetValue(entry, out var existing)) return existing;

            info = new(owner, entry, typeof(T))
            {
                HiddenByMod = PendingHidden.Remove(entry),
            };
            EntryLookup[entry] = info;
            owner.AddEntry(info);
            Version++;
        }

        try
        {
            EntryRegistered?.Invoke(info);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.Error($"ConfigRegistry.EntryRegistered handler threw: {ex}");
        }

        return info;
    }

    /// <summary>
    /// Registers a bool entry that was not bound through <see cref="BaseConfig"/> (legacy CreateSettingsToggle callers).
    /// The entry is attached to a synthetic configuration keyed by the calling assembly.
    /// </summary>
    internal static ConfigEntryInfo RegisterExternalEntry<T>(Assembly assembly, MelonPreferences_Entry<T> entry)
    {
        if (entry == null) return null;
        if (TryGetEntry(entry, out var existing)) return existing;

        var key = $"external://{assembly?.GetName().Name ?? "unknown"}";
        var owner = RegisterConfig(assembly?.GetName().Name ?? entry.Category?.Identifier ?? "Unknown", key, assembly);
        return RegisterEntry(owner, entry);
    }

    /// <summary>
    /// MelonPreferences category identifiers are global, so two configs binding the same section name
    /// share one category and whichever binds last decides the file it saves to. Warn when that happens.
    /// </summary>
    internal static void ClaimSection(string section, string filePath)
    {
        if (string.IsNullOrEmpty(section) || string.IsNullOrEmpty(filePath)) return;

        string owner;
        lock (SyncRoot)
        {
            if (!SectionOwners.TryGetValue(section, out owner))
            {
                SectionOwners[section] = filePath;
                return;
            }
        }

        if (string.Equals(owner, filePath, StringComparison.OrdinalIgnoreCase)) return;

        Plugin.Logger?.Warning(
            $"Config section '{section}' is used by both '{Path.GetFileName(owner)}' and '{Path.GetFileName(filePath)}'. " +
            "MelonPreferences shares categories by name, so these settings may be saved to the wrong file. " +
            $"Use a unique section name (files are in {MelonEnvironment.UserDataDirectory}).");
    }
}
