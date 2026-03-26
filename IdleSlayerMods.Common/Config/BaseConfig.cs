using IdleSlayerMods.Common.Extensions;
using MelonLoader;
using MelonLoader.Utils;
using Tomlet;
using Tomlet.Models;

namespace IdleSlayerMods.Common.Config;

/// <summary>
/// Abstract base class for managing configuration settings and preferences.
/// This class serves as the foundation for creating configuration files and binds setting values
/// to specific preferences. It provides utility methods for initialisation, logging, and binding values.
/// </summary>
public abstract class BaseConfig
{
    private MelonPreferences_Category _cfg;
    private string _configPath;
    private bool _showLoadLog;
    private bool _showSaveLog;
    private bool _hasFixedArrayStrings;

    /// <summary>
    /// Abstract base class for defining configuration settings and managing preferences.
    /// Provides methods for initialisation, logging, and binding configuration values.
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
    /// Provides methods for initialisation, logging, and binding configuration values.
    /// Must be inherited and extended to define specific configuration bindings.
    /// </summary>
    /// <param name="cfgName">The name of the configuration category.</param>
    protected BaseConfig(string cfgName)
    {
        Init(cfgName);
    }
    
    /// <summary>
    /// Method that gets called after the config file has been initialised. Use this to bind your config values.
    /// </summary>
    protected abstract void SetBindings();

    /// <summary>
    /// Method that gets executed after the SetBindings method has been called. Use this to delete old config entries.
    /// </summary>
    protected virtual void OnPostBindingsCleanup() { }

    /// <summary>
    /// Method that gets executed before bindings are loaded.
    /// </summary>
    protected virtual void OnPreBindings()
    {
        if (_hasFixedArrayStrings) return;
        
        BaseConfigExtensions.FixStringArrays(_configPath);
        _hasFixedArrayStrings = true;
    }

    /// <summary>
    /// Initialises the configuration settings, including setting the file path, managing logging preferences,
    /// creating the configuration category, and binding configuration values through the SetBindings method.
    /// </summary>
    /// <param name="cfgName">The name of the configuration category.</param>
    /// <param name="showLoadLog">Flag indicating whether logs should be shown when loading configuration. Default is false.</param>
    /// <param name="showSaveLog">Flag indicating whether logs should be shown when saving configuration. Default is false.</param>
    private void Init(string cfgName, bool showLoadLog = false, bool showSaveLog = false)
    {
        try
        {
            _configPath = Path.Combine(MelonEnvironment.UserDataDirectory, $"{cfgName}.cfg");
            OnPreBindings();
        }
        catch (Exception ex)
        {
            Plugin.Logger.Error(ex.Message);
        }
        finally
        {
            _showLoadLog = showLoadLog;
            _showSaveLog = showSaveLog;
            _cfg = MelonPreferences.CreateCategory(cfgName);
            _cfg.SetFilePath(_configPath, true, _showLoadLog);
            try
            {
                SetBindings();
            }
            finally
            {
                OnPostBindingsCleanup();
            }
        }
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
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [Obsolete("Use Bind(string section, string key, ...) instead.")]
    protected virtual MelonPreferences_Entry<T> Bind<T>(string key, T defaultValue, string description = "")
    {
        if (_cfg.HasEntry(key)) return _cfg.GetEntry<T>(key);
        var entry =  _cfg.CreateEntry(key, defaultValue, description: description);
        _cfg.SaveToFile(_showSaveLog);
        return entry;
    }
    
    /// <summary>
    /// Deletes a MelonPreferences config entry.
    /// </summary>
    /// <param name="key">Name of the config item</param>
    /// <param name="section">Category of the config item</param>
    /// <returns>Returns true if successful, false if not.</returns>
    // ReSharper disable once UnusedMethodReturnValue.Global
    protected virtual bool DeleteBind(string section, string key)
    {
        if (string.IsNullOrWhiteSpace(section)) return false;
        if (!File.Exists(_configPath)) return false;

        var doc = TomlParser.ParseFile(_configPath);

        if (!doc.TryGetValue(section, out var catValue) || catValue is not TomlTable table) return false;
        if (!table.ContainsKey(key)) return false;
            
        table.Entries.Remove(key);

        File.WriteAllText(_configPath, doc.SerializedValue);

        return false;
    }
}
