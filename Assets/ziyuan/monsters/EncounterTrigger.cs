using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EncounterTrigger : MonoBehaviour
{
    public GameObject combatEnemyPrefab;

    [Header("随机敌人数量")]
    [Min(1)] public int minEnemies = 1;
    [Min(1)] public int maxEnemies = 3;

    [Tooltip("可选：配置多个敌人预制体时，每只敌人会从这里随机选择。为空则使用 combatEnemyPrefab。")]
    public List<GameObject> enemyVariants = new List<GameObject>();

    private bool triggered;
    private Vector3 identityPosition;

    private void Awake()
    {
        // Capture the map spawn position before the roaming monster can move.
        // The encounter identity must not use its later collision position.
        identityPosition = transform.position;
    }

    public string EncounterId
    {
        get
        {
            Vector3 p = identityPosition;
            // Round position so tiny prefab/physics differences after a scene
            // reload do not create a new identity for the same map monster.
            p = new Vector3(Mathf.Round(p.x * 10f) / 10f, Mathf.Round(p.y * 10f) / 10f, Mathf.Round(p.z * 10f) / 10f);
            return gameObject.scene.name + "|" + GetHierarchyPath(transform) + "|" + p.ToString("F1");
        }
    }

    string GetHierarchyPath(Transform current)
    {
        string path = current != null ? current.name : "Encounter";
        while (current != null && current.parent != null)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }
        return path;
    }

    private void Start()
    {
        // Delay one frame so the persistent GameManager has completed Awake
        // after a scene load before checking the defeated list.
        Invoke(nameof(ApplyDefeatedState), 0f);
    }

    void ApplyDefeatedState()
    {
        bool defeated = PlayerPrefs.GetInt("defeated_encounter_" + EncounterId, 0) == 1;
        if (GameManager.instance != null) defeated |= GameManager.instance.IsEncounterDefeated(EncounterId);
        if (!defeated && GameManager.instance != null && GameManager.instance.defeatedEncounterPositions != null)
        {
            for (int i = 0; i < GameManager.instance.defeatedEncounterPositions.Count; i++)
            {
                if (Vector3.Distance(GameManager.instance.defeatedEncounterPositions[i], transform.position) < 0.75f)
                {
                    defeated = true;
                    break;
                }
            }
        }
        if (defeated)
        {
            triggered = true;
            Transform root = transform;
            while (root.parent != null && root.parent.gameObject.scene.IsValid()) root = root.parent;
            root.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.CompareTag("Player")) return;

        if (GameManager.instance != null)
        {
            triggered = true;
            GameManager.instance.SetActiveEncounter(EncounterId);
            GameManager.instance.SetActiveEncounterPosition(transform.position);
            GameManager.instance.QueuePlayerReturn(other.transform.position);
            int low = Mathf.Max(1, minEnemies);
            int high = Mathf.Max(low, maxEnemies);
            int enemyCount = Random.Range(low, high + 1);
            List<GameObject> encounter = new List<GameObject>(enemyCount);

            for (int i = 0; i < enemyCount; i++)
            {
                GameObject prefab = combatEnemyPrefab;
                if (enemyVariants != null && enemyVariants.Count > 0)
                {
                    prefab = enemyVariants[Random.Range(0, enemyVariants.Count)];
                }
                if (prefab != null) encounter.Add(prefab);
            }

            GameManager.instance.SetEncounter(encounter);
            SceneManager.LoadScene("CombatScene");
        }
    }
}
