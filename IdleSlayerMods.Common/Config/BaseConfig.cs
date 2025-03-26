using MelonLoader;
using MelonLoader.Utils;

namespace IdleSlayerMods.Common.Config;

public abstract class BaseConfig
{
    private readonly MelonPreferences_Category _cfg;

    protected BaseConfig(string cfgName)
    {
        _cfg = MelonPreferences.CreateCategory(cfgName);
        _cfg.SetFilePath(Path.Combine(MelonEnvironment.UserDataDirectory, $"{cfgName}.cfg"));
        SetBindings();
    }
    
    protected abstract void SetBindings();

    /// <summary>
    /// Creates and returns a MelonPreferences config entry.
    /// </summary>
    /// <param name="key">Name of the config item</param>
    /// <param name="defaultValue">Default value of config item</param>
    /// <param name="description">Friendly description of config item</param>
    /// <typeparam name="T">Data type of config item</typeparam>
    /// <returns>MelonPreferences_Entry of type T</returns>
    protected virtual MelonPreferences_Entry<T> Bind<T>(string key, T defaultValue, string description = "")
    {
        if (_cfg.HasEntry(key)) return _cfg.GetEntry<T>(key);
        var entry =  _cfg.CreateEntry(key, defaultValue, description: description);
        _cfg.SaveToFile();
        return entry;
    }
}
