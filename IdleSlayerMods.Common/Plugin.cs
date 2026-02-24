using IdleSlayerMods.Common;
using MelonLoader;

[assembly: MelonInfo(typeof(Plugin), MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION, MyPluginInfo.PLUGIN_AUTHOR)]
[assembly: MelonPriority(100)]
[assembly: MelonColor(1, 1, 255, 100)]
[assembly: MelonAuthorColor(1, 1, 155, 70)]

namespace IdleSlayerMods.Common;

/// <inheritdoc />
public class Plugin : MelonMod
{
    internal static readonly Settings Settings = new(MyPluginInfo.PLUGIN_GUID);
    internal static readonly MelonLogger.Instance Logger = Melon<Plugin>.Logger;

    /// <inheritdoc />
    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    /// <inheritdoc />
    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName == "Title Screen")
        {
            ModUtils.RegisterComponent<TitleChanger>(false);
        }
        
        if (sceneName != "Game") return;
        ModUtils.RegisterComponent<ModHelper>();
    }
}