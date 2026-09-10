using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("datare")]
    public Vector3 playerReturnPosition;
    [HideInInspector] public bool hasPendingPlayerReturn;
    // Legacy single-enemy field. Keep it for existing scene/prefab references.
    public GameObject enemyToSpawn;
    // The encounter trigger fills this list before loading the battle scene.
    // A list makes multi-enemy encounters possible without changing the scene setup.
    public List<GameObject> enemiesToSpawn = new List<GameObject>();
    [HideInInspector] public string activeEncounterId;
    [HideInInspector] public Vector3 activeEncounterPosition;
    public List<string> defeatedEncounterIds = new List<string>();
    public List<Vector3> defeatedEncounterPositions = new List<Vector3>();

    public void SetActiveEncounter(string encounterId)
    {
        activeEncounterId = encounterId;
    }

    public void SetActiveEncounterPosition(Vector3 position)
    {
        activeEncounterPosition = position;
    }

    public void MarkActiveEncounterDefeated()
    {
        if (string.IsNullOrEmpty(activeEncounterId)) return;
        if (defeatedEncounterIds == null) defeatedEncounterIds = new List<string>();
        if (!defeatedEncounterIds.Contains(activeEncounterId)) defeatedEncounterIds.Add(activeEncounterId);
        if (defeatedEncounterPositions == null) defeatedEncounterPositions = new List<Vector3>();
        bool knownPosition = false;
        for (int i = 0; i < defeatedEncounterPositions.Count; i++)
        {
            if (Vector3.Distance(defeatedEncounterPositions[i], activeEncounterPosition) < 0.25f) { knownPosition = true; break; }
        }
        if (!knownPosition) defeatedEncounterPositions.Add(activeEncounterPosition);
        PlayerPrefs.SetInt("defeated_encounter_" + activeEncounterId, 1);
        PlayerPrefs.Save();
        activeEncounterId = string.Empty;
    }

    public bool IsEncounterDefeated(string encounterId)
    {
        return !string.IsNullOrEmpty(encounterId) && defeatedEncounterIds != null && defeatedEncounterIds.Contains(encounterId);
    }

    public void ResetDefeatedEncounters()
    {
        EncounterTrigger[] sceneEncounters = FindObjectsOfType<EncounterTrigger>(true);
        for (int i = 0; i < sceneEncounters.Length; i++)
        {
            if (sceneEncounters[i] != null)
                PlayerPrefs.DeleteKey("defeated_encounter_" + sceneEncounters[i].EncounterId);
        }
        if (defeatedEncounterIds != null)
        {
            for (int i = 0; i < defeatedEncounterIds.Count; i++)
            {
                string encounterId = defeatedEncounterIds[i];
                if (!string.IsNullOrEmpty(encounterId)) PlayerPrefs.DeleteKey("defeated_encounter_" + encounterId);
            }
            defeatedEncounterIds.Clear();
        }
        if (defeatedEncounterPositions != null) defeatedEncounterPositions.Clear();
        activeEncounterId = string.Empty;
        activeEncounterPosition = Vector3.zero;
        PlayerPrefs.Save();
    }

    [Header("party")]
    // Party members are stored as combat prefabs in formation order.
    // The first entry is always the protagonist; more characters can be added later.
    [Min(1)] public int maxPartySize = 4;
    public List<GameObject> partyMembers = new List<GameObject>();

    public void SetEncounter(IList<GameObject> enemies)
    {
        enemiesToSpawn.Clear();
        if (enemies != null)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i] != null) enemiesToSpawn.Add(enemies[i]);
            }
        }

        enemyToSpawn = enemiesToSpawn.Count > 0 ? enemiesToSpawn[0] : null;
    }

    public bool AddPartyMember(GameObject memberPrefab)
    {
        if (memberPrefab == null || partyMembers.Contains(memberPrefab)) return false;
        if (partyMembers.Count >= Mathf.Max(1, maxPartySize)) return false;

        partyMembers.Add(memberPrefab);
        return true;
    }

    public bool RemovePartyMember(GameObject memberPrefab)
    {
        return memberPrefab != null && partyMembers.Remove(memberPrefab);
    }

    public bool SwapPartyMembers(int firstIndex, int secondIndex)
    {
        if (firstIndex < 0 || secondIndex < 0 ||
            firstIndex >= partyMembers.Count || secondIndex >= partyMembers.Count)
        {
            return false;
        }

        GameObject first = partyMembers[firstIndex];
        partyMembers[firstIndex] = partyMembers[secondIndex];
        partyMembers[secondIndex] = first;
        return true;
    }

    [Header("charater file")]
    public string playerName = "Craven";
    public int level = 1;
    public int currentExp = 0;
    public int maxHP = 100;
    public int currentHP = 100;
    public int maxMP = 50;
    public int currentMP = 50;
    public int attackPower = 20;
    public int defense = 10;
    public int speed = 10;
    public List<string> unlockedSkillIds = new List<string>();
    [HideInInspector] public bool hasPlayerRuntimeState;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Migrate projects/saves created before the protagonist was renamed.
        // This also overrides the old serialized scene default ("hero") so
        // every existing scene starts using the new canonical name.
        if (string.IsNullOrWhiteSpace(playerName) || playerName == "hero")
        {
            playerName = "Craven";
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void QueuePlayerReturn(Vector3 position)
    {
        playerReturnPosition = position;
        hasPendingPlayerReturn = true;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "BattleForest") return;
        if (!hasPendingPlayerReturn) return;
        StartCoroutine(RestorePlayerAfterSceneLoad());
    }

    IEnumerator RestorePlayerAfterSceneLoad()
    {
        // Wait until all map objects have completed Awake/Start and physics
        // has initialized, then place the player at the encounter position.
        for (int attempt = 0; attempt < 8; attempt++)
        {
            yield return null;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.SetPositionAndRotation(playerReturnPosition, player.transform.rotation);
                Rigidbody body = player.GetComponent<Rigidbody>();
                if (body != null)
                {
                    body.position = playerReturnPosition;
                    body.velocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
                Debug.Log("Player restored to encounter position: " + playerReturnPosition);
                hasPendingPlayerReturn = false;
                yield break;
            }
        }
        Debug.LogWarning("Could not find a Player object to restore after returning from battle.");
    }
}
