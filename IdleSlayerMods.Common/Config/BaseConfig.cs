using MelonLoader;
using MelonLoader.Utils;

namespace IdleSlayerMods.Common.Config;

public abstract class BaseConfig
{
    private readonly MelonPreferences_Category _cfg;
    private readonly string _configPath;

    protected BaseConfig(string cfgName)
    {
        _cfg = MelonPreferences.CreateCategory(cfgName);
        _configPath = Path.Combine(MelonEnvironment.UserDataDirectory, $"{cfgName}.cfg");
        _cfg.SetFilePath(_configPath);
        SetBindings();
    }
    
    /// <summary>
    /// Method that gets called after the config file has been initialized. Use this to bind your config values.
    /// </summary>
    protected abstract void SetBindings();

    /// <summary>
    /// Creates and returns a MelonPreferences config entry.
    /// </summary>
    /// <param name="section">Category of the config item</param>
    /// <param name="key">Name of the config item</param>
    /// <param name="defaultValue">Default value of config item</param>
    /// <param name="description">Friendly description of config item</param>
    /// <typeparam name="T">Data type of config item</typeparam>
    /// <returns>MelonPreferences_Entry of type T</returns>
    protected virtual MelonPreferences_Entry<T> Bind<T>(string section, string key, T defaultValue, string description = "")
    {
        var cat = MelonPreferences.CreateCategory(section);
        cat.SetFilePath(_configPath);
        if (cat.HasEntry(key)) return cat.GetEntry<T>(key);
        var entry =  cat.CreateEntry(key, defaultValue, description: description);
        cat.SaveToFile();
        return entry;
    }
    
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
