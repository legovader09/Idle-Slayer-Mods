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

    protected virtual MelonPreferences_Entry<T> Bind<T>(string section, string key, T defaultValue, string description = "")
    {
        var entry =  _cfg.CreateEntry(key, defaultValue, description: description);
        _cfg.SaveToFile();
        return entry;
    }
}
