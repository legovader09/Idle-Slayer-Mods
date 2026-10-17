using Il2Cpp;
using Il2CppTMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IdleConfig.UI;

/// <summary>
/// Text helpers for cloned game UI. The game's StartingText and FontSwitcher components rewrite labels
/// from localisation data, so they are removed immediately (not at end of frame) before setting text.
/// </summary>
internal static class UiText
{
    internal static void Set(Transform label, string text)
    {
        if (!label) return;
        StripLocalisation(label.gameObject);

        var tmp = label.GetComponent<TextMeshProUGUI>();
        if (!tmp) return;
        tmp.text = text;
        tmp.m_text = text;
    }

    internal static void SetFirstChildText(Transform root, string text)
    {
        if (!root) return;
        var tmp = root.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp) Set(tmp.transform, text);
    }

    private static void StripLocalisation(GameObject target)
    {
        if (!target) return;

        var startingText = target.GetComponent<StartingText>();
        if (startingText) Object.DestroyImmediate(startingText);

        var fontSwitcher = target.GetComponent<FontSwitcher>();
        if (fontSwitcher) Object.DestroyImmediate(fontSwitcher);
    }
}
