# Using BaseConfig

In order to utilise `BaseConfig`, we must first create a class that inherits from `BaseConfig`.

Note that `BaseConfig` requires a `string` to be passed down into it, this is used as an identifier - so we will do exactly that directly from the constructor:
`public sealed class Settings(string configName) : BaseConfig(configName)`.

The BaseConfig constructor accepts 2 additional arguments, `showSaveLog` and `showLoadLog`, both are `false` by default, but when enabled will show the logs for saving and loading (useful for sanity checks)

Since `SetBindings()` is **abstract**, it means we must now implement it ourselves **in the derived class**. This is where you will create the settings for your mod. See the example below:

```csharp
using IdleSlayerMods.Common.Config;

public sealed class Settings(string configName) : BaseConfig(configName, [optional] bool showSaveLog, [optional] bool showLoadLog)
{
    internal MelonPreferences_Entry<KeyCode> ExampleInputKey;

    protected override void SetBindings()
    {
        ExampleInputKey = Bind("ExampleCategory (Optional, but recommended for organization)", "ExampleInputKey", KeyCode.B,
            "The key bind for my mod");
    }
}
```
Including the category in the binding will place your config entry under a separate heading within your config file, otherwise it will use the default heading (`configName`).

Finally, to be able to use the settings in your mod, you will need to add the following in your `Plugin.cs Load()` method:

```csharp
public class Plugin : MelonMod
{
    internal static Settings Settings;
    
    public override void OnInitializeMelon()
    {
        Settings = new(MyPluginInfo.PLUGIN_GUID);
    }
}
```
`MyPluginInfo` here is a static class containing the plugin name, but this could be anything really. 

In this example, I also make the Settings variable static, this is so that any `MonoBehaviour` scripts can easily access the settings, just like how the log is accessible.

### Example usage:

```csharp
using UnityEngine;

public class ExampleBehaviour : MonoBehaviour
{
    ..

    private void Update()
    {
        if (Input.GetKeyDown(Plugin.Settings.ExampleInputKey.Value))
        {
            // Do some cool actions
        }
    }
}
```

## Saving a config entry value

To easily save the config entry value, the extension method `SaveEntry()` exists. 

- **It is important to note that config files get auto-saved on game close. This will be changed to be optional in a future update.**

### Base Config Extension

| static class BaseConfigExtensions | MelonLoader logging extension class                        |
|-----------------------------------|------------------------------------------------------------|
| static void SaveEntry()           | Immediately saves the selected config entry value to file. |
