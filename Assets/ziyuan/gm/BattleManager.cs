using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform enemySpawnPoint;
    public Transform playerSpawnPoint; 

    [Tooltip("可选：为敌人提供多个站位；为空时会围绕 enemySpawnPoint 自动排成一列。")]
    public List<Transform> enemyFormationPoints = new List<Transform>();

    [Tooltip("可选：为队伍提供多个站位；为空时会围绕 playerSpawnPoint 自动排成一列。")]
    public List<Transform> partyFormationPoints = new List<Transform>();

    [Header("Automatic Formation")]
    [Min(0.1f)] public float automaticFormationSpacing = 0.8f;
    [Min(0f)] public float automaticFormationRowDepth = 0.5f;

    [Header("Battle Objects")]
    // Legacy field retained so the current CombatScene continues to work.
    public GameObject playerCombatPrefab;

    [Tooltip("队伍顺序就是编队顺序。留空时使用 GameManager.partyMembers，再回退到 playerCombatPrefab。")]
    public List<GameObject> partyCombatPrefabs = new List<GameObject>();

    [Header("External Data")]
    public BattleConfig battleConfig;

    public readonly List<GameObject> spawnedEnemies = new List<GameObject>();
    public readonly List<GameObject> spawnedParty = new List<GameObject>();

    [Header("Turn System")]
    public float enemyActionDelay = 0.6f;
    [Tooltip("Length of the attack animation before damage is applied.")]
    public float attackAnimationDuration = 0.5f;
    public string playerAttackAnimationState = "Craven_Attack";
    public string enemyAttackAnimationState = "Wolf_Attack";
    public int potionHealAmount = 30;
    [Header("Battle Exit")]
    public string explorationSceneName = "BattleForest";
    public float returnToExplorationDelay = 1.0f;
    [Header("Victory Rewards")]
    public float potionDropChance = 1f;
    public float etherDropChance = 0.25f;
    [Range(0f, 1f)] public float escapeChance = 0.5f;
    public bool battleFinished { get; private set; }
    public bool playerWon { get; private set; }
    public bool resultReady { get; private set; }
    public int expRewardGained { get; private set; }
    public int levelBeforeReward { get; private set; }
    public int levelAfterReward { get; private set; }
    public string rewardDropSummary { get; private set; }
    public CombatUnit currentActor { get; private set; }
    public int turnNumber { get; private set; }
    public bool IsPlayerTurn { get { return waitingForPlayerInput; } }

    private readonly List<CombatUnit> turnOrder = new List<CombatUnit>();
    private readonly HashSet<CombatUnit> defendingUnits = new HashSet<CombatUnit>();
    private bool waitingForPlayerInput;
    private bool enemyRoutineRunning;
    private bool selectingSkillTarget;
    private bool skillSelectionOpen;
    private int pendingSkillIndex = -1;
    public bool IsSelectingSkillTarget { get { return selectingSkillTarget; } }
    public bool IsSkillSelectionOpen { get { return skillSelectionOpen; } }
    public int SelectedSkillIndex { get; private set; } = -1;

    void Start()
    {
        Canvas battleCanvas = FindObjectOfType<Canvas>();
        if (battleCanvas != null)
        {
            // The current CombatScene was saved with a zero canvas scale,
            // which makes every combat/result UI invisible.
            battleCanvas.transform.localScale = Vector3.one;
        }
        EnsureResultUI();
        SpawnEnemies();
        SpawnParty();
        BuildTurnOrder();
        BeginNextTurn();
    }

    void ApplyBattleConfig()
    {
        if (battleConfig == null) return;
        enemyActionDelay = battleConfig.enemyActionDelay;
        attackAnimationDuration = battleConfig.attackAnimationDuration;
        playerAttackAnimationState = battleConfig.playerAttackAnimationState;
        enemyAttackAnimationState = battleConfig.enemyAttackAnimationState;
        potionHealAmount = battleConfig.potionHealAmount;
        explorationSceneName = battleConfig.explorationSceneName;
        returnToExplorationDelay = battleConfig.returnToExplorationDelay;
        potionDropChance = battleConfig.potionDropChance;
        etherDropChance = battleConfig.etherDropChance;
        escapeChance = battleConfig.escapeChance;
    }
    void Update()
    {
        EnsureResultUI();
        if (resultReady && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)))
        {
            ContinueAfterResult();
            return;
        }
        if (battleFinished || !waitingForPlayerInput || currentActor == null) return;

        if (Input.GetKeyDown(KeyCode.J)) PlayerAttack();
        else if (Input.GetKeyDown(KeyCode.K)) PlayerDefend();
        else if (Input.GetKeyDown(KeyCode.L)) PlayerSkillAttack();
        else if (Input.GetKeyDown(KeyCode.I)) PlayerUsePotion();
        else if (Input.GetKeyDown(KeyCode.U)) PlayerTryEscape();
    }

    void EnsureResultUI()
    {
        if (FindObjectOfType<BattleResultUI>() != null) return;
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null) canvas.gameObject.AddComponent<BattleResultUI>();
    }

    void BuildTurnOrder()
    {
        turnOrder.Clear();
        AddUnitsToTurnOrder(spawnedParty);
        AddUnitsToTurnOrder(spawnedEnemies);
        turnOrder.Sort((a, b) =>
        {
            int speedCompare = b.speed.CompareTo(a.speed);
            return speedCompare != 0 ? speedCompare : a.GetInstanceID().CompareTo(b.GetInstanceID());
        });
    }

    void AddUnitsToTurnOrder(List<GameObject> objects)
    {
        for (int i = 0; i < objects.Count; i++)
        {
            CombatUnit unit = objects[i] != null ? objects[i].GetComponent<CombatUnit>() : null;
            if (unit != null) turnOrder.Add(unit);
        }
    }

    void BeginNextTurn()
    {
        if (battleFinished || CheckBattleEnded()) return;

        if (turnOrder.Count == 0)
        {
            FinishBattle(false);
            return;
        }

        int safety = 0;
        do
        {
            currentActor = turnOrder[turnNumber % turnOrder.Count];
            turnNumber++;
            safety++;
        }
        while ((currentActor == null || currentActor.currentHP <= 0) && safety <= turnOrder.Count);

        if (currentActor == null || currentActor.currentHP <= 0)
        {
            FinishBattle(false);
            return;
        }

        // Defending lasts until this unit's next turn starts.
        defendingUnits.Remove(currentActor);

        waitingForPlayerInput = currentActor is PlayerUnit;
        selectingSkillTarget = false;
        skillSelectionOpen = false;
        pendingSkillIndex = -1;
        SelectedSkillIndex = -1;
        Debug.Log($"Turn {turnNumber}: {currentActor.unitName} (Speed {currentActor.speed})");

        if (!waitingForPlayerInput && !enemyRoutineRunning)
        {
            StartCoroutine(ExecuteEnemyTurn(currentActor));
        }
    }

    public void PlayerAttack()
    {
        if (battleFinished || !waitingForPlayerInput || currentActor == null) return;

        CombatUnit target = FindFirstAlive(spawnedEnemies);
        if (target == null)
        {
            FinishBattle(true);
            return;
        }

        waitingForPlayerInput = false;
        StartCoroutine(ExecutePlayerAttack(currentActor, target, currentActor.attackPower, "attacks"));
    }

    IEnumerator ExecutePlayerAttack(CombatUnit attacker, CombatUnit target, int power, string action)
    {
        Debug.Log($"{attacker.unitName} {action} {target.unitName}.");
        yield return PlayAttackAnimation(attacker, true);
        if (!battleFinished && target != null && target.currentHP > 0)
        {
            target.TakeDamage(power);
            SyncPlayerData();
        }
        AdvanceAfterAction();
    }

    public void PlayerDefend()
    {
        if (!CanPlayerAct()) return;

        waitingForPlayerInput = false;
        defendingUnits.Add(currentActor);
        Debug.Log($"{currentActor.unitName} defends. Incoming damage is halved until the next turn.");
        AdvanceAfterAction();
    }

    public void PlayerSkillAttack()
    {
        OpenSkillSelection();
    }

    public void OpenSkillSelection()
    {
        if (!CanPlayerAct()) return;
        selectingSkillTarget = false;
        skillSelectionOpen = true;
        pendingSkillIndex = -1;
        SelectedSkillIndex = -1;
    }

    public void CancelSkillSelection()
    {
        if (battleFinished) return;
        selectingSkillTarget = false;
        skillSelectionOpen = false;
        pendingSkillIndex = -1;
        SelectedSkillIndex = -1;
    }

    public void UseSkill(int skillIndex)
    {
        if (!CanPlayerAct()) return;
        PlayerUnit player = currentActor as PlayerUnit;
        if (player == null || !player.IsSkillUnlocked(skillIndex)) return;
        SkillDefinition skill = player.skills[skillIndex];
        SelectedSkillIndex = skillIndex;
        if (player.currentMP < skill.mpCost)
        {
            Debug.Log($"Not enough MP for {skill.skillName}. Need {skill.mpCost}, current MP: {player.currentMP}.");
            return;
        }
        if (skill.needsEnemyTarget || skill.type == SkillType.SingleTargetDamage)
        {
            pendingSkillIndex = skillIndex;
            selectingSkillTarget = true;
            return;
        }
        ExecuteSkill(skillIndex, -1);
    }

    public void SelectSkillTarget(int enemyIndex)
    {
        if (!CanPlayerAct() || !selectingSkillTarget || pendingSkillIndex < 0) return;
        if (enemyIndex < 0 || enemyIndex >= spawnedEnemies.Count) return;
        CombatUnit target = spawnedEnemies[enemyIndex] != null ? spawnedEnemies[enemyIndex].GetComponent<CombatUnit>() : null;
        if (target == null || target.currentHP <= 0) return;
        int skillIndex = pendingSkillIndex;
        selectingSkillTarget = false;
        skillSelectionOpen = false;
        pendingSkillIndex = -1;
        SelectedSkillIndex = -1;
        ExecuteSkill(skillIndex, enemyIndex);
    }

    void ExecuteSkill(int skillIndex, int enemyIndex)
    {
        PlayerUnit player = currentActor as PlayerUnit;
        if (player == null || skillIndex < 0 || skillIndex >= player.skills.Count) return;
        SkillDefinition skill = player.skills[skillIndex];
        player.currentMP -= skill.mpCost;
        player.SaveDataToGameManager();
        waitingForPlayerInput = false;
        StartCoroutine(ExecuteSkillRoutine(player, skill, enemyIndex));
    }

    IEnumerator ExecuteSkillRoutine(PlayerUnit player, SkillDefinition skill, int enemyIndex)
    {
        yield return PlayAttackAnimation(player, true);
        if (!battleFinished)
        {
            if (skill.type == SkillType.SelfHeal)
            {
                player.currentHP = Mathf.Min(player.maxHP, player.currentHP + Mathf.Max(1, Mathf.RoundToInt(skill.power)));
                Debug.Log($"{player.unitName} uses {skill.skillName} and recovers HP.");
            }
            else if (skill.type == SkillType.AllEnemiesDamage)
            {
                for (int i = 0; i < spawnedEnemies.Count; i++)
                {
                    CombatUnit target = spawnedEnemies[i] != null ? spawnedEnemies[i].GetComponent<CombatUnit>() : null;
                    if (target != null && target.currentHP > 0) target.TakeDamage(Mathf.Max(1, Mathf.RoundToInt(player.attackPower * skill.power)));
                }
            }
            else if (enemyIndex >= 0 && enemyIndex < spawnedEnemies.Count)
            {
                CombatUnit target = spawnedEnemies[enemyIndex] != null ? spawnedEnemies[enemyIndex].GetComponent<CombatUnit>() : null;
                if (target != null && target.currentHP > 0) target.TakeDamage(Mathf.Max(1, Mathf.RoundToInt(player.attackPower * skill.power)));
            }
            SyncPlayerData();
        }
        AdvanceAfterAction();
    }

    public void PlayerUsePotion()
    {
        if (!CanPlayerAct()) return;

        waitingForPlayerInput = false;
        PlayerUnit player = currentActor as PlayerUnit;
        string message = "No usable item.";
        if (player == null || !InventoryManager.GetOrCreate().TryUseHealingItem(player, out message))
        {
            Debug.Log(message);
            waitingForPlayerInput = true;
            return;
        }
        Debug.Log(message);
        AdvanceAfterAction();
    }

    public void PlayerTryEscape()
    {
        if (!CanPlayerAct()) return;

        waitingForPlayerInput = false;
        if (Random.value <= escapeChance)
        {
            FinishBattle(false, "Escaped from battle!");
            return;
        }

        Debug.Log("Escape failed!");
        AdvanceAfterAction();
    }

    bool CanPlayerAct()
    {
        return !battleFinished && waitingForPlayerInput && currentActor != null && currentActor.currentHP > 0;
    }

    IEnumerator ExecuteEnemyTurn(CombatUnit enemy)
    {
        enemyRoutineRunning = true;
        yield return new WaitForSeconds(enemyActionDelay);

        EnemyUnit enemyUnit = enemy as EnemyUnit;
        SkillDefinition selectedSkill = SelectEnemySkill(enemyUnit);
        CombatUnit target = FindFirstAlive(spawnedParty);
        if (target != null && enemy != null && enemy.currentHP > 0)
        {
            yield return PlayAttackAnimation(enemy, false);
            if (selectedSkill != null)
            {
                if (enemyUnit != null) enemyUnit.currentMP = Mathf.Max(0, enemyUnit.currentMP - selectedSkill.mpCost);
                ExecuteSkillEffect(enemy, selectedSkill, target);
            }
            else
            {
                int attackPower = enemy.attackPower;
                if (defendingUnits.Contains(target))
                    attackPower = Mathf.Max(1, Mathf.CeilToInt(attackPower * GetDefendDamageMultiplier()));
                if (target.currentHP > 0) target.TakeDamage(attackPower);
            }
            SyncPlayerData();
        }

        enemyRoutineRunning = false;
        AdvanceAfterAction();
    }

    SkillDefinition SelectEnemySkill(EnemyUnit enemy)
    {
        if (enemy == null || enemy.skills == null) return null;
        SkillDefinition best = null;
        for (int i = 0; i < enemy.skills.Count; i++)
        {
            SkillDefinition candidate = enemy.skills[i];
            if (candidate == null || candidate.type == SkillType.SelfHeal || candidate.mpCost > enemy.currentMP) continue;
            if (candidate.needsEnemyTarget && FindFirstAlive(spawnedParty) == null) continue;
            if (best == null || candidate.aiPriority > best.aiPriority) best = candidate;
        }
        return best;
    }

    void ExecuteSkillEffect(CombatUnit attacker, SkillDefinition skill, CombatUnit target)
    {
        if (attacker == null || skill == null) return;
        int power = Mathf.Max(1, Mathf.RoundToInt(attacker.attackPower * skill.power));
        if (skill.type == SkillType.AllEnemiesDamage)
        {
            for (int i = 0; i < spawnedParty.Count; i++)
            {
                CombatUnit unit = spawnedParty[i] != null ? spawnedParty[i].GetComponent<CombatUnit>() : null;
                if (unit != null && unit.currentHP > 0) unit.TakeDamage(power);
            }
        }
        else if (target != null && target.currentHP > 0)
        {
            target.TakeDamage(power);
        }
    }

    float GetDefendDamageMultiplier()
    {
        return battleConfig != null ? battleConfig.defendDamageMultiplier : 0.5f;
    }
    IEnumerator PlayAttackAnimation(CombatUnit unit, bool playerAnimation)
    {
        if (unit == null) yield break;

        Vector3 originalScale = unit.transform.localScale;
        Animator animator = unit.GetComponent<Animator>();
        SpriteRenderer spriteRenderer = unit.GetComponentInChildren<SpriteRenderer>();
        Sprite originalSprite = spriteRenderer != null ? spriteRenderer.sprite : null;
        if (animator == null)
        {
            yield return new WaitForSeconds(attackAnimationDuration);
            yield break;
        }

        string stateName = playerAnimation ? playerAttackAnimationState : enemyAttackAnimationState;
        int stateHash = Animator.StringToHash(stateName);
        animator.enabled = true;
        if (animator.HasState(0, stateHash))
        {
            animator.Play(stateHash, 0, 0f);
            animator.Update(0f);
            // Keep the configured battle sprite size if an imported clip writes root scale.
            unit.transform.localScale = originalScale;
        }
        else
        {
            Debug.LogWarning($"Animator state '{stateName}' was not found on {unit.unitName}. Damage will still be applied.");
        }

        yield return new WaitForSeconds(Mathf.Max(0f, attackAnimationDuration));
        animator.enabled = false;
        unit.transform.localScale = originalScale;
        if (spriteRenderer != null && originalSprite != null)
        {
            spriteRenderer.sprite = originalSprite;
        }
    }

    void AdvanceAfterAction()
    {
        if (CheckBattleEnded()) return;
        BeginNextTurn();
    }

    bool CheckBattleEnded()
    {
        bool partyAlive = HasAliveUnit(spawnedParty);
        bool enemiesAlive = HasAliveUnit(spawnedEnemies);
        if (!partyAlive)
        {
            FinishBattle(false);
            return true;
        }
        if (!enemiesAlive)
        {
            FinishBattle(true);
            return true;
        }
        return false;
    }

    bool HasAliveUnit(List<GameObject> objects)
    {
        return FindFirstAlive(objects) != null;
    }

    CombatUnit FindFirstAlive(List<GameObject> objects)
    {
        for (int i = 0; i < objects.Count; i++)
        {
            CombatUnit unit = objects[i] != null ? objects[i].GetComponent<CombatUnit>() : null;
            if (unit != null && unit.currentHP > 0) return unit;
        }
        return null;
    }

    void FinishBattle(bool playerWon, string customMessage = null)
    {
        if (battleFinished) return;
        SyncPlayerData();
        battleFinished = true;
        waitingForPlayerInput = false;
        currentActor = null;
        this.playerWon = playerWon;
        Debug.Log(string.IsNullOrEmpty(customMessage) ? (playerWon ? "Battle won!" : "Battle lost!") : customMessage);
        if (playerWon)
        {
            AwardVictoryRewards();
            if (GameManager.instance != null) GameManager.instance.MarkActiveEncounterDefeated();
            resultReady = true;
        }
        else
        {
            StartCoroutine(ReturnToExploration());
        }
    }

    void AwardVictoryRewards()
    {
        PlayerUnit player = null;
        for (int i = 0; i < spawnedParty.Count; i++)
        {
            if (spawnedParty[i] == null) continue;
            player = spawnedParty[i].GetComponent<PlayerUnit>();
            if (player != null) break;
        }
        if (player == null) return;

        levelBeforeReward = player.level;
        EnemyUnit[] rewardEnemies = new EnemyUnit[spawnedEnemies.Count];
        for (int i = 0; i < spawnedEnemies.Count; i++)
            rewardEnemies[i] = spawnedEnemies[i] != null ? spawnedEnemies[i].GetComponent<EnemyUnit>() : null;
        expRewardGained = BattleRewardService.CalculateExperience(rewardEnemies, battleConfig != null ? battleConfig.fallbackExpReward : 20);
        player.GainExp(expRewardGained);
        levelAfterReward = player.level;

        InventoryManager inventory = InventoryManager.GetOrCreate();
        int potionCount = BattleRewardService.RollDropCount(spawnedEnemies.Count, potionDropChance);
        int etherCount = BattleRewardService.RollDropCount(spawnedEnemies.Count, etherDropChance);
        if (inventory != null)
        {
            if (potionCount > 0) inventory.AddItem("potion", "Potion", "Restores " + (battleConfig != null ? battleConfig.potionHealAmount : 30) + " HP.", potionCount, battleConfig != null ? battleConfig.potionHealAmount : 30, 0);
            if (etherCount > 0) inventory.AddItem("ether", "Ether", "Restores " + (battleConfig != null ? battleConfig.etherRestoreAmount : 20) + " MP.", etherCount, 0, battleConfig != null ? battleConfig.etherRestoreAmount : 20);
        }

        StringBuilder drops = new StringBuilder();
        if (potionCount > 0) drops.Append("Potion x").Append(potionCount);
        if (etherCount > 0)
        {
            if (drops.Length > 0) drops.Append("\n");
            drops.Append("Ether x").Append(etherCount);
        }
        rewardDropSummary = drops.Length > 0 ? drops.ToString() : "None";
        Debug.Log("Rewards: EXP +" + expRewardGained + ", Drops: " + rewardDropSummary);
    }

    public void ContinueAfterResult()
    {
        if (!battleFinished || !playerWon || !resultReady) return;
        resultReady = false;
        StartCoroutine(ReturnToExploration());
    }

    IEnumerator ReturnToExploration()
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, returnToExplorationDelay));
        if (GameManager.instance != null)
        {
            // Preserve the position captured by EncounterTrigger before the
            // battle scene was loaded. SaveLoadController uses the same value.
            Debug.Log("Returning to exploration at " + GameManager.instance.playerReturnPosition);
            GameManager.instance.enemiesToSpawn.Clear();
            GameManager.instance.enemyToSpawn = null;
        }
        Time.timeScale = 1f;
        if (GameManager.instance != null)
        {
            GameManager.instance.QueuePlayerReturn(GameManager.instance.playerReturnPosition);
        }
        SceneManager.LoadScene(explorationSceneName);
    }

    void SyncPlayerData()
    {
        for (int i = 0; i < spawnedParty.Count; i++)
        {
            if (spawnedParty[i] == null) continue;
            PlayerUnit player = spawnedParty[i].GetComponent<PlayerUnit>();
            if (player != null)
            {
                player.SaveDataToGameManager();
                return;
            }
        }
    }

    void SpawnEnemies()
    {
        List<GameObject> encounter = new List<GameObject>();
        if (GameManager.instance != null && GameManager.instance.enemiesToSpawn != null)
        {
            encounter.AddRange(GameManager.instance.enemiesToSpawn);
        }

        // Backwards compatibility for encounters saved before the list existed.
        if (encounter.Count == 0 && GameManager.instance != null && GameManager.instance.enemyToSpawn != null)
        {
            encounter.Add(GameManager.instance.enemyToSpawn);
        }

        for (int i = 0; i < encounter.Count; i++)
        {
            if (encounter[i] == null) continue;
            Transform slot = GetSlot(enemyFormationPoints, enemySpawnPoint, i, encounter.Count, false);
            GameObject spawned = Instantiate(encounter[i], slot.position, slot.rotation);
            PrepareBattleAnimator(spawned);
            spawnedEnemies.Add(spawned);
        }
    }

    void SpawnParty()
    {
        List<GameObject> party = new List<GameObject>();
        if (partyCombatPrefabs != null && partyCombatPrefabs.Count > 0)
        {
            party.AddRange(partyCombatPrefabs);
        }
        else if (GameManager.instance != null && GameManager.instance.partyMembers != null && GameManager.instance.partyMembers.Count > 0)
        {
            party.AddRange(GameManager.instance.partyMembers);
        }
        else if (playerCombatPrefab != null)
        {
            party.Add(playerCombatPrefab);
        }

        for (int i = 0; i < party.Count; i++)
        {
            if (party[i] == null) continue;
            Transform slot = GetSlot(partyFormationPoints, playerSpawnPoint, i, party.Count, true);
            GameObject spawned = Instantiate(party[i], slot.position, slot.rotation);
            PrepareBattleAnimator(spawned);
            spawnedParty.Add(spawned);
        }
    }

    void PrepareBattleAnimator(GameObject instance)
    {
        if (instance == null) return;
        Animator animator = instance.GetComponent<Animator>();
        if (animator != null) animator.enabled = false;
    }

    Transform GetSlot(List<Transform> configuredSlots, Transform fallback, int index, int totalCount, bool party)
    {
        if (configuredSlots != null && index < configuredSlots.Count && configuredSlots[index] != null)
        {
            return configuredSlots[index];
        }

        // Generate stable positions without requiring scene edits today.
        Vector3 origin = fallback != null ? fallback.position : Vector3.zero;
        Quaternion rotation = fallback != null ? fallback.rotation : Quaternion.identity;
        float spacing = Mathf.Max(0.1f, automaticFormationSpacing);
        int row = index / 3;
        int column = index % 3;
        int countInRow = Mathf.Min(3, totalCount - row * 3);
        float x = (column - (countInRow - 1) * 0.5f) * spacing;
        float z = row * (party ? -automaticFormationRowDepth : automaticFormationRowDepth);

        GameObject marker = new GameObject((party ? "PartySlot_" : "EnemySlot_") + index);
        marker.transform.SetPositionAndRotation(origin + new Vector3(x, 0f, z), rotation);
        marker.hideFlags = HideFlags.HideAndDontSave;
        return marker.transform;
    }
}
