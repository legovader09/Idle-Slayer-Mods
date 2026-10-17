# IdleSlayerMods.Common

Shared library for Idle Slayer mods built with MelonLoader (IL2CPP). It provides:

- **Configuration files** with a simple base class (`BaseConfig`)
- **In-game helpers** for notifications, dialogs, achievement popups and info panel buttons (`ModHelper`)
- **Runtime image loading** for icons and other sprites (`SpriteHelper`)
- **Component registration** and debug logging helpers (`ModUtils`, `LoggerExtension`)
- **A configuration registry** that configuration UIs can read to display your settings (`ConfigRegistry`)

## Getting started

**Players** install the mod from [Nexus Mods](https://www.nexusmods.com/idleslayer/mods/3).

**Mod authors** reference the [NuGet package](https://www.nuget.org/packages/IdleSlayerMods.Common) and declare
the dependency so MelonLoader loads the library before your mod:

```csharp
[assembly: MelonAdditionalDependencies("IdleSlayerMods.Common")]
```

Players of your mod need the library installed too.

## Registering components (ModUtils)

Custom `MonoBehaviour` classes must be registered with IL2CPP before they can be used. `ModUtils.RegisterComponent<T>()`
does this and creates an instance in the scene for you.

| Member                                              | Description                                                                                                                                                                      |
|-----------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `GameObject RegisterComponent<T>()`                 | Registers `T` with IL2CPP, creates a new GameObject named after `T` with the component attached, and marks it `DontDestroyOnLoad`. Returns the GameObject, or `null` on failure. |
| `GameObject RegisterComponent<T>(bool dontDestroy)` | Same, but the GameObject is only kept across scene loads when `dontDestroy` is `true`.                                                                                           |
| `bool DebugMode`                                    | Whether debug logging is enabled (read-only, see [Logging](#logging)).                                                                                                           |

A common pattern is to create your component when the game scene loads:

```csharp
public override void OnSceneWasLoaded(int buildIndex, string sceneName)
{
    if (sceneName != "Game") return;
    ModUtils.RegisterComponent<MyBehaviour>(false); // destroyed with the scene
}
```

`RegisterComponent` always creates its own GameObject. To attach a component to an existing GameObject instead,
register the type yourself first with Il2CppInterop's `ClassInjector.RegisterTypeInIl2Cpp<T>()`, then call
`AddComponent<T>()`. Calling `AddComponent<T>()` on an unregistered type fails.

## Logging

`LoggerExtension` adds debug logging to any MelonLoader logger. Debug messages only appear when debug mode is on.

```csharp
using IdleSlayerMods.Common.Extensions;

LoggerInstance.Debug("Only shown when debug mode is enabled");
```

| Member                                                                     | Description                                                                   |
|----------------------------------------------------------------------------|-------------------------------------------------------------------------------|
| `Debug(this MelonLogger.Instance logger, string message)`                  | Logs a grey `[DEBUG]` message when debug mode is on.                          |
| `Debug(this MelonLogger.Instance logger, ColorARGB color, string message)` | Logs an `[ERROR]`-prefixed message in the given colour when debug mode is on. |

Debug mode is the `Debug Mode` setting in the `[IdleSlayerCore]` section of `UserData/IdleSlayerMods.Common.cfg`.

## In-game helpers (ModHelper)

`ModHelper` is a component created in the **Game scene only**. Get the instance from the `ModHelperMounted` event,
which fires each time the Game scene loads:

```csharp
using IdleSlayerMods.Common;
using MelonLoader;

public class Plugin : MelonMod
{
    internal static ModHelper ModHelperInstance;

    public override void OnInitializeMelon()
    {
        ModHelper.ModHelperMounted += instance => ModHelperInstance = instance;
    }
}
```

The event is raised from `ModHelper.Awake`, so use the instance from `Start` or later:

```csharp
public class MyBehaviour : MonoBehaviour
{
    private void Start()
    {
        Plugin.ModHelperInstance.ShowNotification("Hello from my mod!", false);
    }
}
```

| Member                                                                                     | Description                                                                                                                                                                                                                                                                                       |
|--------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `static event Action<ModHelper> ModHelperMounted`                                          | Raised when the helper is created in the Game scene.                                                                                                                                                                                                                                              |
| `void ShowNotification(string message, bool shine)`                                        | Shows a centred message like the game's event texts. `shine` adds a sparkle animation.                                                                                                                                                                                                            |
| `void ShowDialog(...)`                                                                     | Shows the game's popup dialog. All parameters are optional: `title`, `subtitle`, `descriptionText`, `descriptionColor`, `centerDescription`, `confirmText`, `confirmAction`, `cancelText`, `cancelAction`, `content`, `overridePopup`. The confirm button only appears when `confirmText` is set. |
| `void ShowAchievement(Achievement achievement)`                                            | Shows the achievement-unlocked popup. See [Achievements](#achievements).                                                                                                                                                                                                                          |
| `GameObject CreatePanelButton(string text, Action clickAction = null, Sprite icon = null)` | Adds a button to the info panel (next to Achievements, Options, etc.) and returns it, or `null` if the panel isn't available. See [Loading images](#loading-images) for icons.                                                                                                                    |
| `static Texture2D LoadTextureFromFile(string filePath)`                                    | Loads a PNG file from disk. Embedded resources are preferred; see [Loading images](#loading-images).                                                                                                                                                                                              |

### Obsolete members

These still compile and work as described, but shouldn't be used in new code.

| Member                                                                                                                                | Use instead                                                                                                                                                                        |
|---------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `void AddPanelButton(string text, Action clickAction = null, Texture2D icon = null)`                                                  | `CreatePanelButton`. The icon passed to `AddPanelButton` is not displayed.                                                                                                         |
| `void CreateSettingsToggle(string displayName, MelonPreferences_Entry<bool> configEntry, Action<bool> onChanged)`                     | Bind the setting with `BaseConfig`; see [Configuration UIs](https://github.com/legovader09/Idle-Slayer-Mods/blob/master/IdleSlayerMods.Common/Config/README.md#configuration-uis). |
| `static AssetBundle LoadBundle(string bundlePath)` / `static Texture2D LoadTextureFromBundle(AssetBundle bundle, string textureName)` | `SpriteHelper`. AssetBundles are unreliable in this game.                                                                                                                          |

`CreateSettingsToggle` adds a toggle to the game's own Options menu, together with `CreateSettingsCategory(string displayName, int index = 0)`,
which adds a heading there. If a configuration UI that reads `ConfigRegistry` is installed, `CreateSettingsToggle`
no longer adds the toggle to the Options menu (to avoid showing it twice). It still forwards changes to your
`onChanged` callback, and the setting appears in that UI under `displayName`.

## Achievements

`AchievementHelper.CreateAchievement` creates an `Achievement` object you can display with `ModHelper.ShowAchievement`:

```csharp
using IdleSlayerMods.Common.Helpers;
using Il2Cpp;

var achievement = AchievementHelper.CreateAchievement(
    AchievementType.Misc, "Speed Demon", "Reach 16x game speed");

Plugin.ModHelperInstance.ShowAchievement(achievement);
```

| Parameter                             | Description                                     |
|---------------------------------------|-------------------------------------------------|
| `AchievementType type`                | The game's achievement category.                |
| `string achievementName`              | Name shown in the popup.                        |
| `string description`                  | Description, usually the unlock condition.      |
| `Sprite icon = null`                  | Icon shown in the popup.                        |
| `bool grantsProgressionPoint = false` | Whether it counts towards achievement progress. |

The achievement is created for display only; it is not added to the game's achievement list or Steam.

## Configuration

`BaseConfig` gives your mod its own `.cfg` file with typed settings in a few lines of code. Settings bound through it
are also published to `ConfigRegistry`, so configuration UIs can show them without extra work from you.

See **[Configuration (BaseConfig)](https://github.com/legovader09/Idle-Slayer-Mods/blob/master/IdleSlayerMods.Common/Config/README.md)**.

## Loading images

Embed PNG images in your mod's DLL and load them with `SpriteHelper`. The image ships inside your DLL, so there are
no extra files to install or lose.

> Don't call `UnityEngine.ImageConversion.LoadImage` directly: it throws `MissingMethodException` in this game's
> IL2CPP environment. `SpriteHelper` decodes PNGs itself and doesn't use `LoadImage`.

### 1. Embed the image

Add the PNG to your project (for example `Assets/MyIcon.png`) as an `EmbeddedResource` in your `.csproj`. Setting a
`LogicalName` fixes the resource name, so it's easy to look up:

```xml
<ItemGroup>
  <None Remove="Assets\MyIcon.png" />
  <EmbeddedResource Include="Assets\MyIcon.png" LogicalName="MyMod.Assets.MyIcon.png" />
</ItemGroup>
```

The build action must be `EmbeddedResource`. Build actions that copy the file next to your DLL (such as `Content`
or `None` with `CopyToOutputDirectory`) don't embed it, so it won't be found.

Without a `LogicalName`, the resource is named `<RootNamespace>.<Folder>.<FileName>`. When looking a resource up,
`SpriteHelper` tries the exact name first, then any resource name ending with the name you pass (case-insensitive).

### 2. Load it

```csharp
using IdleSlayerMods.Common.Helpers;
using UnityEngine;

internal static class MyIcons
{
    private const string IconResource = "MyMod.Assets.MyIcon.png";
    private static Sprite _icon;

    internal static Sprite Icon
    {
        get
        {
            if (_icon == null)
                _icon = SpriteHelper.LoadSpriteFromEmbeddedResource(typeof(MyIcons).Assembly, IconResource);
            return _icon;
        }
    }
}
```

- Pass your own mod's assembly (`typeof(AnyTypeInYourMod).Assembly`); that's where the resource is.
- Load on Unity's main thread, such as from a component's `Start` or `Update`.
- Textures and sprites created by `SpriteHelper` survive scene changes, so load once and cache the result as above.
- Every method returns `null` on failure. Each loading stage writes a `[SpriteHelper]` line to the log, so you can
  see which stage failed.

### 3. Use it

Assign the sprite to any `UnityEngine.UI.Image`, or pass it to any function that expects an Image such as `ModHelper.CreatePanelButton`:

```csharp
public class MyBehaviour : MonoBehaviour
{
    private void Start()
    {
        Plugin.ModHelperInstance.CreatePanelButton("My Mod", OnClick, MyIcons.Icon);
    }

    private static void OnClick() { /* open your mod's window, etc. */ }
}
```

### Supported formats

**Use PNG.** PNGs are decoded in managed code and support all colour types (greyscale, RGB, palette, with or without
alpha), bit depths 1–16, transparency and interlacing. Other formats (such as JPG) are passed to
`ImageConversion.LoadImage` and will fail in this game, so convert them to PNG first.

### SpriteHelper reference

| Member                                                                                                                                                   | Description                                                                    |
|----------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------|
| `Sprite LoadSpriteFromEmbeddedResource(Assembly assembly, string resourceName, FilterMode filterMode = FilterMode.Bilinear, float pixelsPerUnit = 100f)` | Loads an embedded PNG as a Sprite. Use `FilterMode.Point` for crisp pixel art. |
| `Texture2D LoadTextureFromEmbeddedResource(Assembly assembly, string resourceName, FilterMode filterMode = FilterMode.Bilinear)`                         | Loads an embedded PNG as a Texture2D.                                          |
| `byte[] ReadEmbeddedResource(Assembly assembly, string resourceName)`                                                                                    | Returns the raw bytes of an embedded resource.                                 |
| `Texture2D LoadTextureFromBytes(byte[] data, string name = null, FilterMode filterMode = FilterMode.Bilinear)`                                           | Decodes PNG bytes into a Texture2D.                                            |
| `Sprite CreateSprite(Texture2D texture, float pixelsPerUnit = 100f)`                                                                                     | Wraps a whole texture in a centred Sprite.                                     |

For a complete working example, see how [IdleConfig](https://github.com/legovader09/Idle-Slayer-Mods/tree/master/IdleConfig) loads its menu button icon (`IdleConfig.csproj` and `UI/IconLoader.cs`).
