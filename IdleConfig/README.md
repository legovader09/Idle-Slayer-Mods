# Idle Config

An in-game settings window for Idle Slayer mods.

Mods built on [IdleSlayerMods.Common](../IdleSlayerMods.Common/README.md) store their settings in `.cfg` files in the
`UserData` folder, which players normally have to find and edit by hand. Idle Config lets players change those
settings in game from one window, without editing files and without each mod building its own settings screen.

## Requirements

- MelonLoader
- [IdleSlayerMods.Common](../IdleSlayerMods.Common/README.md) **1.4.0 or later**

## What it does

Idle Config adds an **Idle Config** button to the info panel (alongside Achievements, Options, etc.). The button
opens a **Mod Settings** window that lists the settings of every installed mod that uses IdleSlayerMods.Common's
`BaseConfig`:

- One heading per mod, using the mod's name, in alphabetical order.
- A **toggle** for every on/off (`bool`) setting.
- A **key binding** for every key (`KeyCode`) setting. Click the key, then press the new key. Press Esc to cancel.

Changes are saved to the mod's `.cfg` file immediately. The window always shows the current values when opened,
including values changed by the mods themselves (for example, with a hotkey).

## For players

Install Idle Config alongside `IdleSlayerMods.Common`. Any compatible mods you have installed appear in the window automatically.

Settings that aren't on/off switches or key bindings (numbers, text, lists) aren't shown. Change those in the mod's `.cfg` file in the `UserData` folder.

## For mod authors

You don't need to reference Idle Config or write any code for it. Bind your settings with `BaseConfig` as usual:

```csharp
protected override void SetBindings()
{
    Enabled = Bind("MyMod", "Enabled", true, "Turns the mod on or off.");     // shown as a toggle
    ToggleKey = Bind("MyMod", "Toggle Key", KeyCode.F6, "Toggles the mod.");  // shown as a key binding
    Multiplier = Bind("MyMod", "Multiplier", 1.5f, "Speed multiplier.");      // not shown; edited in the .cfg file
}
```

This also works for mods built against older versions of `IdleSlayerMods.Common`, as long as the player has 1.4.0 or later installed.

Tips:

- **Use readable keys.** The setting's key is used as its label, so `"Toggle Key"` reads better than `"toggleKey"`.
- **Bind key settings as `KeyCode`,** not `string`, to get a key binding control.
- **Read `.Value` when you need it,** or subscribe to changes if you cache values, so changes made in the window take effect straight away. See [Reacting to changes](../IdleSlayerMods.Common/Config/README.md#reacting-to-changes).
- **Hide settings players shouldn't change** with `HideFromConfigUI(entry)`.
- **Mention restarts** in the description of any setting that only takes effect at startup.

See [Configuration UIs](../IdleSlayerMods.Common/Config/README.md#configuration-uis) for full details.

### Legacy CreateSettingsToggle

When Idle Config is installed, toggles added with the obsolete `ModHelper.CreateSettingsToggle` appear in Idle Config instead of the game's Options menu, using the name the mod passed in. The mod's `onChanged` callback still runs when the value changes.

## Limitations

- Only `bool` and `KeyCode` settings get controls. Other setting types work normally in their mods but must be edited in the `.cfg` file.
- Setting descriptions aren't shown in the window.
- Each key setting holds a single key. Mouse buttons can't be bound, and there's no way to clear a binding from the window.
- If a new key binding is already used by another mod's setting, Idle Config writes a warning to the log, but doesn't prevent it.
- Settings are grouped by mod only; sections within a mod aren't shown as separate headings.
- Whether a change takes effect immediately depends on the mod. Most apply straight away; some only apply after the game is restarted.
