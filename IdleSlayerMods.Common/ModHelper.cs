using UnityEngine;
using UnityEngine.Events;

namespace IdleSlayerMods.Common;

public sealed class ModHelper : MonoBehaviour
{
    public static event Action<ModHelper>? ModHelperMounted;
    private AchievementManager? _achievementManager;
    private NotificationText? _notificationText;
    private Popup? _popup;

    private void Awake()
    {
        OnModHelperMounted();
        _notificationText = GameObject.Find("Notification Text").GetComponent<NotificationText>();
        _popup = GameObject.Find("Popup").GetComponent<Popup>();
        _achievementManager = GameObject.Find("Achievement Manager").GetComponent<AchievementManager>();
    }

    public void ShowDialog(string title = "", string subtitle = "", string descriptionText = "", Color descriptionColor = default, bool centerDescription = false, string confirmText = "", UnityAction? confirmAction = null, string cancelText = "", UnityAction? cancelAction = null, string? toggleText = null, Sprite? image = null,bool overridePopup = false)
    {
        _popup?.Show(new()
        {
            title = title,
            subtitle = subtitle,
            confirmActionText = confirmText,
            confirmAction = confirmAction,
            toggleText = toggleText,
            overrideCurrentPopup = overridePopup,
            description = descriptionText,
            descriptionColor = descriptionColor,
            cancelActionText = cancelText,
            cancelAction = cancelAction,
            centeredDescription = centerDescription,
            sprite = image
        });
    }

    public void ShowAchievement(Achievement achievement) => _achievementManager?.ShowNextUnlockedAchivement(achievement);

    public void ShowNotification(string message, bool shine)
    {
        Plugin.Log.LogDebug($"Attempting to show notification with message: {message}");
        _notificationText?.Show(message, shine);
    }

    private void OnModHelperMounted()
    {
        ModHelperMounted?.Invoke(this);
    }
}