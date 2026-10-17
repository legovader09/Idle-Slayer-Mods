using System.Reflection;
using IdleSlayerMods.Common.Extensions;
using MelonLoader;

namespace IdleSlayerMods.Common.Config;

/// <summary>
/// Read-only metadata describing one mod's configuration file and the entries bound into it.
/// Obtain instances from <see cref="ConfigRegistry.Configs"/>.
/// </summary>
public sealed class ModConfigInfo
{
    private readonly List<ConfigEntryInfo> _entries = [];

    internal ModConfigInfo(string configName, string filePath, Assembly assembly)
    {
        ConfigName = configName;
        FilePath = filePath;
        Assembly = assembly;
    }

    /// <summary>
    /// The name passed to the <see cref="BaseConfig"/> constructor (also the config file name).
    /// </summary>
    public string ConfigName { get; }

    /// <summary>
    /// Full path of the .cfg file.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// The assembly that declared the <see cref="BaseConfig"/> subclass.
    /// </summary>
    public Assembly Assembly { get; }

    /// <summary>
    /// The MelonLoader mod that owns this configuration, or null if it cannot be resolved yet.
    /// Resolved lazily because configs are often created before MelonLoader finishes registering the mod.
    /// </summary>
    public MelonBase Melon
    {
        get
        {
            if (field != null) return field;
            try
            {
                foreach (var melon in MelonBase.RegisteredMelons)
                {
                    if (melon?.MelonAssembly?.Assembly != Assembly) continue;
                    field = melon;
                    break;
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.Debug($"Unable to resolve melon for config '{ConfigName}': {ex.Message}");
            }
            return field;
        }
    }

    /// <summary>
    /// The name to show in a configuration UI: the mod's MelonInfo name, falling back to <see cref="ConfigName"/>.
    /// </summary>
    public string DisplayName
    {
        get
        {
            var name = Melon?.Info?.Name;
            return string.IsNullOrWhiteSpace(name) ? ConfigName : name;
        }
    }

    /// <summary>
    /// A snapshot of every entry bound into this configuration, in binding order.
    /// </summary>
    public IReadOnlyList<ConfigEntryInfo> Entries
    {
        get
        {
            lock (ConfigRegistry.SyncRoot) return _entries.ToArray();
        }
    }

    /// <summary>
    /// Entries that a configuration UI should display (supported type and not hidden).
    /// </summary>
    public IEnumerable<ConfigEntryInfo> VisibleEntries => Entries.Where(e => e.IsSupported && !e.IsHidden);

    /// <summary>
    /// True when at least one entry should be displayed.
    /// </summary>
    public bool HasVisibleEntries => VisibleEntries.Any();

    /// <summary>
    /// Saves every category this configuration has entries in.
    /// </summary>
    public void Save()
    {
        foreach (var category in Entries.Select(e => e.Entry.Category).Where(c => c != null).Distinct())
        {
            try
            {
                category.SaveToFile(false);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.Error($"Failed to save config '{ConfigName}' section '{category.Identifier}': {ex.Message}");
            }
        }
    }

    // Caller holds ConfigRegistry.SyncRoot.
    internal void AddEntry(ConfigEntryInfo entry) => _entries.Add(entry);

    /// <inheritdoc />
    public override string ToString() => $"{DisplayName} ({ConfigName})";
}
