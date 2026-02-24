using IdleSlayerMods.Common.Constants;
using Il2CppTMPro;
using UnityEngine;

namespace IdleSlayerMods.Common;

/// <inheritdoc />
public class TitleChanger : MonoBehaviour
{
    private bool _initialized;

    private void Awake()
    {
        if (!Plugin.Settings.ShowModVersionOnTitleScreen.Value)
        {
            Destroy(this);
        }
    }

    private void LateUpdate()
    {
        if (_initialized) return;

        var canvasGo = GameObject.Find(PathConstants.TitleVersionPath);
        if (!canvasGo) return;

        _initialized = true;

        var modVersionGo = Instantiate(canvasGo, canvasGo.transform.parent, true);
        modVersionGo.name = "ModVersion";

        var rectTransform = modVersionGo.GetComponent<RectTransform>();
        var textMesh = modVersionGo.GetComponent<TextMeshProUGUI>();
        textMesh.text = $"Mods Core v{MyPluginInfo.PLUGIN_VERSION}";
        textMesh.enableWordWrapping = false;
        textMesh.autoSizeTextContainer = true;
        textMesh.enableAutoSizing = true;
        textMesh.fontSizeMin = 8;
        textMesh.fontSizeMax = 14;

        rectTransform.anchoredPosition = new(-1132, 8);
        rectTransform.sizeDelta = new(500, 50);
    }
}