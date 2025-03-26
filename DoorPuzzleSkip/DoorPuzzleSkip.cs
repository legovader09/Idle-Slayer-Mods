using System.Collections;
using MelonLoader;
using UnityEngine;

namespace DoorPuzzleSkip;

public class DoorPuzzleSkip : MonoBehaviour
{
    private readonly GameObject _doorPuzzle = null;
    private bool _hasOpenedDoor;
    private bool _doorPuzzleFound;
    private const string DoorPuzzlePath = "Stationary Map/Underground(Clone)/Puzzle/DoorPuzzle";

    private void Awake()
    {
        MelonCoroutines.Start(FindDoorPuzzle());
    }

    private IEnumerator FindDoorPuzzle()
    {
        while (_doorPuzzleFound == false)
        {
            var doorPuzzle = _doorPuzzle.transform.Find(DoorPuzzlePath);
            if (doorPuzzle != null)
            {
                _doorPuzzleFound = true;
                Plugin.Logger.Msg("Door puzzle found");
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private void LateUpdate()
    {
        if (!_doorPuzzleFound || _hasOpenedDoor) return;
        _doorPuzzle.GetComponent<BoxCollider2D>().enabled = false;
        var door = _doorPuzzle.transform.GetChild(0).gameObject;
        door.active = true;
        _hasOpenedDoor = true;
    }
}