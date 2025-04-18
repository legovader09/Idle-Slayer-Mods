using IdleSlayerMods.Common.Extensions;
using Il2Cpp;
using UnityEngine;

namespace RestoreLastMap;

public class MapRestorer : MonoBehaviour
{
    private readonly MapController _mapController = MapController.instance;
    private bool _hasRestored;
    
    private readonly List<string> _blackList =
    [
        "dialog_mt_otto_ascending_heights",
        "map_bonus_stage_1",
        "map_bonus_stage_2",
        "map_bonus_stage_3",
        "map_special_bonus_stage",
        "bosses_victor_alpha"
    ];

    public void Start()
    {
        ChangeMapPatch.OnChangeMap += OnChangeMap;
    }

    public void RestoreMap()
    {
        var lastMap = Plugin.Config.LastMap.Value;
        if (string.IsNullOrWhiteSpace(lastMap) || _blackList.Contains(lastMap) || _mapController.selectedMap.name == lastMap) return;
        
        var newMap = _mapController.maps.First(x => x.name == lastMap);
        Plugin.Logger.Debug($"Restoring map: {lastMap}");

        if (Plugin.Config.InstantTransfer.Value)
        {
            var tempName = newMap.localizedName;
            newMap.localizedName = "";
            _mapController.SpawnPortal(newMap, new(new Vector2(_mapController.player.position.x + 1f, _mapController.groundYPos)));
            newMap.localizedName = tempName;
            Plugin.ModHelper.ShowNotification("Restoring last map...", false);
        }
        else
        {
            var tempName = newMap.localizedName;
            newMap.localizedName = "Restore Last Map";
            _mapController.SpawnPortal(newMap, new(new Vector2(_mapController.player.position.x + 25f, _mapController.groundYPos)));
            newMap.localizedName = tempName;
        }
    }

    public void Update()
    {
        if (!GameState.IsRunner() || _hasRestored) return;
        _hasRestored = true;
        RestoreMap();
    }

    private static void ChangeMap(BaseMap map)
    {
        if (string.IsNullOrWhiteSpace(map.name)) return;
        Plugin.Config.LastMap.Value = map.name.Trim();
        Plugin.Config.LastMap.SaveEntry();
    }

    /// Save current map to last map on destroy to avoid portal showing up on next boot
    public void OnDestroy() => ChangeMap(_mapController.selectedMap);

    private static void OnChangeMap(Map map) => ChangeMap(map);
}
