using UnityEngine;

/// <summary>
/// Marks a position in a scene where the player can arrive via cross-scene teleport.
/// Give each spawn point a unique id and reference that id from DoorTeleport.
/// </summary>
public class SceneSpawnPoint : MonoBehaviour
{
    [Tooltip("Unique id used by DoorTeleport to reference this spawn point.")]
    public string spawnPointId = "spawn";
}
