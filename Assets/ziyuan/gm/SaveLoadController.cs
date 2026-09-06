using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Scene-aware bridge between UI buttons and SaveSystem.</summary>
public class SaveLoadController : MonoBehaviour
{
    public string saveSlot = "slot1";
    public string explorationSceneName = "BattleForest";
    public string battleSceneName = "CombatScene";
    public Transform playerTransformOverride;

    private SaveSystem.SaveData pendingLoad;

    public void SaveGame()
    {
        GameManager manager = GameManager.instance;
        if (manager == null)
        {
            Debug.LogError("Save failed: GameManager.instance is null.");
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        Transform player = FindPlayerTransform();
        Vector3 position = player != null ? player.position : manager.playerReturnPosition;

        // CombatScene contains a temporary battle clone, so save the exploration
        // scene and the position recorded when the encounter began.
        if (sceneName == battleSceneName)
        {
            sceneName = explorationSceneName;
            position = manager.playerReturnPosition;
        }

        SaveSystem.Save(saveSlot, sceneName, position, manager);
    }

    public void LoadGame()
    {
        SaveSystem.SaveData data;
        if (!SaveSystem.TryLoad(saveSlot, out data))
        {
            Debug.LogWarning("No save file found in slot: " + saveSlot);
            return;
        }

        GameManager manager = GameManager.instance;
        SaveSystem.ApplyToManager(data, manager);
        pendingLoad = data;
        SceneManager.sceneLoaded += RestoreLoadedPlayer;
        SceneManager.LoadScene(data.sceneName);
    }

    public bool HasSave()
    {
        return SaveSystem.HasSave(saveSlot);
    }

    void RestoreLoadedPlayer(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= RestoreLoadedPlayer;
        if (pendingLoad == null) return;

        GameManager manager = GameManager.instance;
        manager.playerReturnPosition = new Vector3(pendingLoad.playerX, pendingLoad.playerY, pendingLoad.playerZ);
        Transform player = FindPlayerTransform();
        if (player != null) player.position = manager.playerReturnPosition;
        pendingLoad = null;
    }

    Transform FindPlayerTransform()
    {
        if (playerTransformOverride != null) return playerTransformOverride;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.transform : null;
    }
}
