using MelonLoader;
using MelonLoader.Utils;

namespace IdleSlayerMods.Common.Config;

/// <summary>
/// Abstract base class for managing configuration settings and preferences.
/// This class serves as the foundation for creating configuration files and binds setting values
/// to specific preferences. It provides utility methods for initialization, logging, and binding values.
/// </summary>
public abstract class BaseConfig
{
    private MelonPreferences_Category _cfg;
    private string _configPath;
    private bool _showLoadLog;
    private bool _showSaveLog;

    /// <summary>
    /// Abstract base class for defining configuration settings and managing preferences.
    /// Provides methods for initialization, logging, and binding configuration values.
    /// Must be inherited and extended to define specific configuration bindings.
    /// </summary>
    /// <param name="cfgName">The name of the configuration category.</param>
    /// <param name="showLoadLog">Flag indicating whether logs should be shown when loading configuration. Default is false.</param>
    /// <param name="showSaveLog">Flag indicating whether logs should be shown when saving configuration. Default is false.</param>
    protected BaseConfig(string cfgName, bool showLoadLog = false, bool showSaveLog = false)
    {
        Init(cfgName, showLoadLog, showSaveLog);
    }

    /// <summary>
    /// Abstract base class for defining configuration settings and managing preferences.
    /// Provides methods for initialization, logging, and binding configuration values.
    /// Must be inherited and extended to define specific configuration bindings.
    /// </summary>
    /// <param name="cfgName">The name of the configuration category.</param>
    protected BaseConfig(string cfgName)
    {
        Init(cfgName);
    }
    
    /// <summary>
    /// Method that gets called after the config file has been initialized. Use this to bind your config values.
    /// </summary>
    protected abstract void SetBindings();

    /// <summary>
    /// Initializes the configuration settings, including setting the file path, managing logging preferences,
    /// creating the configuration category, and binding configuration values through the SetBindings method.
    /// </summary>
    /// <param name="cfgName">The name of the configuration category.</param>
    /// <param name="showLoadLog">Flag indicating whether logs should be shown when loading configuration. Default is false.</param>
    /// <param name="showSaveLog">Flag indicating whether logs should be shown when saving configuration. Default is false.</param>
    private void Init(string cfgName, bool showLoadLog = false, bool showSaveLog = false)
    {
        _showLoadLog = showLoadLog;
        _showSaveLog = showSaveLog;
        _cfg = MelonPreferences.CreateCategory(cfgName);
        _configPath = Path.Combine(MelonEnvironment.UserDataDirectory, $"{cfgName}.cfg");
        _cfg.SetFilePath(_configPath, true, _showLoadLog);
        SetBindings();
    }

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
        cat.SetFilePath(_configPath, true, _showLoadLog);
        if (cat.HasEntry(key)) return cat.GetEntry<T>(key);
        var entry =  cat.CreateEntry(key, defaultValue, description: description);
        cat.SaveToFile(_showSaveLog);
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
        _cfg.SaveToFile(_showSaveLog);
        return entry;
    }
}
