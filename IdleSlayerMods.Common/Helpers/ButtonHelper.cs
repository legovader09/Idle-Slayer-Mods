using Il2CppTMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace IdleSlayerMods.Common.Helpers;

public static class ButtonHelper
{
    /// <summary>
    /// Duplicate an info panel button, and set values to default.
    /// </summary>
    /// <param name="templatePath">Path of the button GameObject.</param>
    /// <returns>New GameObject instance of an info panel button.</returns>
    internal static GameObject CreateTemplateButton(string templatePath)
    {
        var button = Object.Instantiate(GameObject.Find(templatePath));
        var visualContent = button.transform.Find("Background/Content");
        visualContent.GetChild(0).GetComponent<Image>().sprite = null;
        visualContent.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Button";
        button.GetComponent<Button>().onClick = null;
        return button;
    }
}