using BepInEx.Configuration;

namespace IdleSlayerMods.Common.Config;

public abstract class BaseConfig
{
    private ConfigFile? _cfg;

    protected virtual void SetBindings(ConfigFile? cfg) => _cfg = cfg;

    protected virtual ConfigEntry<T>? Bind<T>(string section, string key, T defaultValue, string description = "") =>
        _cfg?.Bind(
            section: section,
            key: key,
            defaultValue: defaultValue,
            configDescription: new(
                description, typeof(T) != typeof(bool) ? new AcceptableValueRange<int>(0, 100) : null
            )
        );
}
