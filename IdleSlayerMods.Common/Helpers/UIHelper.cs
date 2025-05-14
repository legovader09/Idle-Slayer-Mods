using Il2Cpp;
using Il2CppTMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IdleSlayerMods.Common.Helpers;

public static class UIHelper
{
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