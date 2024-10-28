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
                description
            )
        );
    
    protected virtual ConfigEntry<int>? Bind(string section, string key, int defaultValue, string description = "", int minValue = 0, int maxValue = 100) =>
        _cfg?.Bind(
            section: section,
            key: key,
            defaultValue: defaultValue,
            configDescription: new(
                description, new AcceptableValueRange<int>(minValue, maxValue)
            )
        );
}
