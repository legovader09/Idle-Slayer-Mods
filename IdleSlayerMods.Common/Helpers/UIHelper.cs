using Il2Cpp;
using Il2CppTMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IdleSlayerMods.Common.Helpers;

/// <summary>
/// Provides helper methods for UI operations within the Idle Slayer Mods framework.
/// </summary>
public static class UIHelper
{
    /// <summary>
    /// Updates the text of a given label Transform with the specified text.
    /// </summary>
    /// <param name="label">The Transform representing the label whose text will be updated.</param>
    /// <param name="text">The new text to display on the label.</param>
    /// <returns>The updated Transform after setting the label text.</returns>
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static Transform SetLabelText(Transform label, string text)
    {
        Object.Destroy(label.GetComponent<StartingText>());
        Object.Destroy(label.GetComponent<FontSwitcher>());
        var tmp = label.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.m_text = text;
        return label;
    }
}