# Configuration (BaseConfig)

`BaseConfig` gives your mod its own config file with typed settings. You declare each setting once, read it anywhere in your mod, and its value is saved to and loaded from `UserData/<configName>.cfg` automatically.

## Creating a config

Inherit from `BaseConfig` and declare your settings in `SetBindings()`:

```csharp
using IdleSlayerMods.Common.Config;
using MelonLoader;
using UnityEngine;

internal sealed class Settings(string configName) : BaseConfig(configName)
{
    internal MelonPreferences_Entry<bool> Enabled;
    internal MelonPreferences_Entry<KeyCode> ToggleKey;
    internal MelonPreferences_Entry<float> SpeedMultiplier;

    protected override void SetBindings()
    {
        Enabled = Bind("MyMod", "Enabled", true, "Turns the mod on or off.");
        ToggleKey = Bind("MyMod", "Toggle Key", KeyCode.F6, "Key that toggles the mod.");
        SpeedMultiplier = Bind("MyMod", "Speed Multiplier", 1.5f, "How much faster things run.");
    }
}
```

Create it once when your mod initialises, and keep it somewhere your code can reach:

```csharp
public class Plugin : MelonMod
{
    internal static Settings Settings;

    public override void OnInitializeMelon()
    {
        Settings = new("MyMod"); // creates/loads UserData/MyMod.cfg
    }
}
```

The name you pass becomes the file name. A common choice is your mod's GUID or name.

To log when the file is loaded and saved, pass the optional constructor arguments:
`BaseConfig(configName, showLoadLog: true, showSaveLog: true)`. Both default to `false`.

## Binding settings

```csharp
protected MelonPreferences_Entry<T> Bind<T>(string section, string key, T defaultValue, string description = "")
```

| Parameter | Description |
|-----------|-------------|
| `section` | The `[section]` heading the setting is written under in the .cfg file. |
| `key` | The setting's name in the file. Configuration UIs also use it as the label, so a readable name like `"Toggle Key"` works well. |
| `defaultValue` | The value used when the setting isn't in the file yet. It also sets the setting's type `T`. |
| `description` | Written to the file as a comment above the setting. |

If the setting already exists in the file, its saved value is loaded; otherwise it's added with the default. `Bind` returns a `MelonPreferences_Entry<T>`. Read and write the setting through its `.Value` property.

**Value types:** `T` can be any type MelonLoader can store in a config file, for example `bool`, `int`, `float`, `double`, `string`, enums such as `KeyCode`, arrays and `List<T>`. Every type works the same way: it's saved, loaded and read through `.Value`.

**Section names:** MelonLoader shares sections between mods by name, so two mods using the same section name can end up saving into each other's files. Use a section name unique to your mod. A warning is logged if two configs use the same one.

> The older overload `Bind<T>(string key, T defaultValue, string description = "")` is obsolete. It writes to a section named after the config. Prefer the overload with a `section`.

## Reading and saving values

Read `.Value` wherever you need the setting:

```csharp
public class MyBehaviour : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(Plugin.Settings.ToggleKey.Value))
        {
            // toggle something
        }
    }
}
```

To change a setting from code, set `.Value`. To write it to disk immediately, call the `SaveEntry()` extension method. MelonLoader also saves all settings when the game closes.

```csharp
using IdleSlayerMods.Common.Extensions;

Plugin.Settings.SpeedMultiplier.Value = 2f;
Plugin.Settings.SpeedMultiplier.SaveEntry();
```

## Reacting to changes

A setting's value can change while the game is running, for example when a player edits it in a configuration UI or when your own code sets it. The new value is written into the same `MelonPreferences_Entry` your `Settings` class holds.

- **If your code always reads `.Value` when it needs the setting, you don't need to do anything.** Each read returns the current value.
- **If your code copies the value into a field, or uses it once (for example at startup), subscribe to `OnEntryValueChanged`.** Otherwise your code keeps using the old value until the field is set again.

### Reading `.Value` directly (recommended)

The `Update` example above always sees the latest key, so it needs no subscription. Prefer this pattern where practical.

### Keeping a cached value in sync

If you cache a setting in a field, subscribe where the field is set and update it when the setting changes:

```csharp
using MelonLoader;
using UnityEngine;

public class MyBehaviour : MonoBehaviour
{
    private bool _enabled;                               // cached copy, read every frame
    private LemonAction<bool, bool> _onEnabledChanged;

    private void Awake()
    {
        _enabled = Plugin.Settings.Enabled.Value;

        _onEnabledChanged = OnEnabledChanged;
        Plugin.Settings.Enabled.OnEntryValueChanged.Subscribe(_onEnabledChanged);
    }

    private void OnDestroy()
    {
        // The setting outlives this component, so remove the handler with it.
        Plugin.Settings.Enabled.OnEntryValueChanged.Unsubscribe(_onEnabledChanged);
    }

    private void OnEnabledChanged(bool oldValue, bool newValue) => _enabled = newValue;

    private void Update()
    {
        if (!_enabled) return;
        // ...
    }
}
```

- Keep a reference to the handler so you can unsubscribe the same delegate.
- When subscribing from a component, unsubscribe in `OnDestroy`. A subscription made in `OnInitializeMelon` lasts for the whole session and doesn't need removing.
- The event also fires when your own code sets `.Value`, so make sure the handler is safe in that case.
- Subscribe where the cached value lives, not in `SetBindings()`: the field or object you want to update usually doesn't exist yet when settings are bound.
- Some settings can only take effect at startup (for example, ones that decide which patches are applied). For those, say in the setting's description that a restart is needed.

## Optional hooks

| Member | Description |
|--------|-------------|
| `protected abstract void SetBindings()` | **Required.** Bind your settings here. |
| `protected virtual void OnPreBindings()` | Runs before `SetBindings()`. The base implementation repairs badly formatted string arrays in the file, so call `base.OnPreBindings()` if you override it. |
| `protected virtual void OnPostBindingsCleanup()` | Runs after `SetBindings()`. Use it to remove settings your mod no longer uses. |
| `protected virtual bool DeleteBind(string section, string key)` | Removes a setting from the .cfg file. Returns `true` if it was removed. |
| `protected void HideFromConfigUI(params MelonPreferences_Entry[] entries)` | Keeps settings out of configuration UIs. See [Configuration UIs](#configuration-uis). |
| `protected ModConfigInfo ConfigInfo` | This config's entry in `ConfigRegistry`. See [Advanced: ConfigRegistry](#advanced-configregistry). |

Example of removing a renamed setting:

```csharp
protected override void OnPostBindingsCleanup()
{
    DeleteBind("MyMod", "Old Setting Name");
}
```

## Extension methods

`BaseConfigExtensions` (namespace `IdleSlayerMods.Common.Extensions`):

| Member                                                                                  | Description                                                                                           |
|-----------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------|
| `void SaveEntry<T>(this MelonPreferences_Entry<T> entry, bool showSaveMessage = false)` | Writes the setting's section to disk immediately.                                                     |
| `static void FixStringArrays(string configPath)`                                        | Repairs badly formatted string arrays in a config file. `OnPreBindings()` already calls this for you. |

## Configuration UIs

Every setting you `Bind` is also published to `ConfigRegistry`. Configuration UIs (mods that read the registry, such as [IdleConfig](https://github.com/legovader09/Idle-Slayer-Mods/tree/master/IdleConfig)) can then show your settings to players without any UI code in your mod. If no such UI is installed, nothing changes: your settings work exactly the same.

### Setting types vs. UI controls

These are two separate things:

- **Setting types** are what `BaseConfig` can store. All the [value types](#binding-settings) above are fully
  supported and always save, load and read normally.
- **UI controls** are what a configuration UI can display. The registry marks each setting with the kind of control a UI can offer for it:

| Setting type                                                     | Control kind (`ConfigEntryKind`) | What a UI can offer                                                                           |
|------------------------------------------------------------------|----------------------------------|-----------------------------------------------------------------------------------------------|
| `bool`                                                           | `Toggle`                         | An on/off toggle                                                                              |
| `KeyCode`                                                        | `KeyBind`                        | A key binding                                                                                 |
| Any other type (`int`, `float`, `string`, other enums, lists, …) | `Unsupported`                    | No control. The setting still works normally in your mod; players change it in the .cfg file. |

`Unsupported` only means there's no standard UI control for that type. It doesn't affect how the setting is stored or used.

To get a key binding control, bind key settings as `KeyCode`. `Bind("MyMod", "Toggle Key", KeyCode.F6)` gets one;
`Bind("MyMod", "Toggle Key", "F6")` is a `string` setting, which works in your code but gets no control.

### How your settings appear

- Settings are grouped under your mod's name from its `MelonInfo` attribute (or the config name if it can't be found).
- Each setting is labelled with its `key`.
- A change made in a UI is saved to your .cfg file straight away. See [Reacting to changes](#reacting-to-changes)
  for making sure your mod picks it up.

### Hiding settings

To keep a `bool` or `KeyCode` setting out of configuration UIs (for example, internal or advanced settings), call `HideFromConfigUI` after binding it. It's still saved, loaded and read normally:

```csharp
internal MelonPreferences_Entry<bool> DebugOverlay;

protected override void SetBindings()
{
    DebugOverlay = Bind("MyMod", "Debug Overlay", false);
    HideFromConfigUI(DebugOverlay);
}
```

### Replacing CreateSettingsToggle

`ModHelper.CreateSettingsToggle` is obsolete; you don't need it for settings bound through `BaseConfig`. If you use its `onChanged` callback, move that logic to `OnEntryValueChanged` (see
[Reacting to changes](#reacting-to-changes)), or read `.Value` directly.

## Advanced: ConfigRegistry

> This section is for authors of configuration UIs and tools. Regular mods don't need it: binding settings with `BaseConfig` is enough to appear in the registry.

`ConfigRegistry` (namespace `IdleSlayerMods.Common.Config`) records every config and setting created through `BaseConfig`, whatever its type.

### Reading the registry

```csharp
using System.Linq;
using IdleSlayerMods.Common.Config;

foreach (var mod in ConfigRegistry.Configs.Where(c => c.HasVisibleEntries))
{
    // mod.DisplayName: heading for this mod
    foreach (var entry in mod.VisibleEntries)
    {
        switch (entry.Kind)
        {
            case ConfigEntryKind.Toggle:
                var toggle = entry.AsToggle();      // MelonPreferences_Entry<bool>
                toggle.Value = !toggle.Value;
                entry.Save();
                break;
            case ConfigEntryKind.KeyBind:
                var keyBind = entry.AsKeyBind();    // MelonPreferences_Entry<KeyCode>
                break;
        }
    }
}
```

`Configs` and `Entries` return snapshots. Compare `ConfigRegistry.Version` with a value you stored earlier to see whether anything has been added or hidden since, and rebuild your UI only when it has changed.

### Announcing a UI

A configuration UI should announce itself from `OnInitializeMelon`:

```csharp
ConfigRegistry.RegisterUIProvider("My Config UI");
```

While at least one UI is registered, `ModHelper.CreateSettingsToggle` stops adding toggles to the game's Options menu, so settings aren't shown twice.

### API reference

**`ConfigRegistry`** (static)

| Member                                                                     | Description                                                                  |
|----------------------------------------------------------------------------|------------------------------------------------------------------------------|
| `IReadOnlyList<ModConfigInfo> Configs`                                     | All registered configs, in registration order.                               |
| `IEnumerable<ConfigEntryInfo> GetEntries()`                                | All registered settings across all configs.                                  |
| `IEnumerable<ConfigEntryInfo> GetEntries(ConfigEntryKind kind)`            | All settings of one control kind.                                            |
| `bool TryGetEntry(MelonPreferences_Entry entry, out ConfigEntryInfo info)` | Looks up the registry entry for a setting.                                   |
| `int Version`                                                              | Changes whenever settings are added or hidden.                               |
| `event Action<ConfigEntryInfo> EntryRegistered`                            | Raised after a setting is registered.                                        |
| `void RegisterUIProvider(string providerName)`                             | Announces a configuration UI.                                                |
| `bool HasUIProvider`                                                       | Whether any configuration UI has been announced.                             |
| `void SetHidden(MelonPreferences_Entry entry, bool hidden)`                | Hides or shows a setting in UIs. Works before or after the setting is bound. |

**`ModConfigInfo`**

| Member                                        | Description                                                                   |
|-----------------------------------------------|-------------------------------------------------------------------------------|
| `string ConfigName`                           | The name passed to the `BaseConfig` constructor.                              |
| `string DisplayName`                          | The owning mod's `MelonInfo` name, or `ConfigName` if the mod can't be found. |
| `string FilePath`                             | Full path of the .cfg file.                                                   |
| `Assembly Assembly`                           | The assembly that declared the config class.                                  |
| `MelonBase Melon`                             | The owning mod, or `null` if not found.                                       |
| `IReadOnlyList<ConfigEntryInfo> Entries`      | All settings in this config, in binding order.                                |
| `IEnumerable<ConfigEntryInfo> VisibleEntries` | Settings with a control kind other than `Unsupported` that aren't hidden.     |
| `bool HasVisibleEntries`                      | Whether `VisibleEntries` contains anything.                                   |
| `void Save()`                                 | Saves every section this config has settings in.                              |

**`ConfigEntryInfo`**

| Member                                        | Description                                    |
|-----------------------------------------------|------------------------------------------------|
| `ModConfigInfo Owner`                         | The config this setting belongs to.            |
| `MelonPreferences_Entry Entry`                | The underlying setting.                        |
| `Type ValueType`                              | The setting's value type.                      |
| `ConfigEntryKind Kind`                        | `Toggle`, `KeyBind` or `Unsupported`.          |
| `bool IsSupported`                            | `Kind` is not `Unsupported`.                   |
| `string Section` / `string Key`               | Where the setting is stored.                   |
| `string DisplayName`                          | The label to show.                             |
| `string Description`                          | The description passed to `Bind`.              |
| `bool IsHidden`                               | Whether the setting should be left out of UIs. |
| `MelonPreferences_Entry<bool> AsToggle()`     | The setting as a `bool` entry, or `null`.      |
| `MelonPreferences_Entry<KeyCode> AsKeyBind()` | The setting as a `KeyCode` entry, or `null`.   |
| `void Save()`                                 | Saves the setting's section to disk.           |
