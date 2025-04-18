using IdleSlayerMods.Common.Extensions;
using Il2Cpp;
using UnityEngine;

namespace BetterLoadLocation;

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
        "bosses_victor_alpha",
        // "map_factory",
        // "map_frozen_fields",
        // "map_funky_space",
        // "map_haunted_castle",
        // "map_hills",
        // "map_hot_desert",
        // "map_jungle",
        // "map_modern_city",
        // "map_mystic_valley",
        // "map_forest"
    ];

    public void Start()
    {
        ChangeMapPatch.OnChangeMap += OnChangeMap;
    }

    public void RestoreMap()
    {
        var lastMap = Plugin.Config.LastMap.Value;
        if (_blackList.Contains(lastMap) || _mapController.selectedMap.name == lastMap) return;
        
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
            _mapController.SpawnPortal(newMap, new(new Vector2(_mapController.player.position.x + 20f, _mapController.groundYPos)));
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
