using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUIController : MonoBehaviour
{
    [Header("References")]
    public BattleManager battleManager;
    public GameObject skillPanel;

    [Header("Legacy Skill References")]
    public Button normalSkillButton;
    public Button ultimateSkillButton;
    public Button healSkillButton;
    public TMP_Text normalSkillText;
    public TMP_Text ultimateSkillText;
    public TMP_Text healSkillText;
    public TMP_Text normalSkillCostText;
    public TMP_Text ultimateSkillCostText;
    public TMP_Text healSkillCostText;

    [Header("Dynamic Skill References")]
    public List<Button> skillButtons = new List<Button>();
    public List<TMP_Text> skillNameTexts = new List<TMP_Text>();
    public List<TMP_Text> skillCostTexts = new List<TMP_Text>();

    [Header("Description and Targets")]
    public TMP_Text skillDescriptionText;
    public Button skillBackButton;
    public Button enemyTargetButton1;
    public Button enemyTargetButton2;
    public Button enemyTargetButton3;
    public Button allyTargetButton1;
    public Button allyTargetButton2;
    public Button allyTargetButton3;
    private bool listenersBound;

    void Awake()
    {
        if (battleManager == null) battleManager = FindObjectOfType<BattleManager>();
        BuildLegacyLists();
        BindListeners();
    }

    void Update()
    {
        if (battleManager != null) Refresh();
    }

    void BuildLegacyLists()
    {
        if (skillButtons == null) skillButtons = new List<Button>();
        if (skillNameTexts == null) skillNameTexts = new List<TMP_Text>();
        if (skillCostTexts == null) skillCostTexts = new List<TMP_Text>();
        if (skillButtons.Count == 0)
        {
            skillButtons.Add(normalSkillButton);
            skillButtons.Add(ultimateSkillButton);
            skillButtons.Add(healSkillButton);
        }
        if (skillNameTexts.Count == 0)
        {
            skillNameTexts.Add(normalSkillText);
            skillNameTexts.Add(ultimateSkillText);
            skillNameTexts.Add(healSkillText);
        }
        if (skillCostTexts.Count == 0)
        {
            skillCostTexts.Add(normalSkillCostText);
            skillCostTexts.Add(ultimateSkillCostText);
            skillCostTexts.Add(healSkillCostText);
        }
    }

    void BindListeners()
    {
        if (listenersBound || battleManager == null) return;
        listenersBound = true;
        for (int i = 0; i < skillButtons.Count; i++)
        {
            int index = i;
            if (skillButtons[i] != null) skillButtons[i].onClick.AddListener(() => battleManager.UseSkill(index));
        }
        if (skillBackButton != null) skillBackButton.onClick.AddListener(battleManager.CancelSkillSelection);
        if (enemyTargetButton1 != null) enemyTargetButton1.onClick.AddListener(() => battleManager.SelectSkillTarget(0));
        if (enemyTargetButton2 != null) enemyTargetButton2.onClick.AddListener(() => battleManager.SelectSkillTarget(1));
        if (enemyTargetButton3 != null) enemyTargetButton3.onClick.AddListener(() => battleManager.SelectSkillTarget(2));
        if (allyTargetButton1 != null) allyTargetButton1.onClick.AddListener(() => battleManager.SelectSkillAllyTarget(0));
        if (allyTargetButton2 != null) allyTargetButton2.onClick.AddListener(() => battleManager.SelectSkillAllyTarget(1));
        if (allyTargetButton3 != null) allyTargetButton3.onClick.AddListener(() => battleManager.SelectSkillAllyTarget(2));
    }

    void Refresh()
    {
        PlayerUnit player = battleManager.currentActor as PlayerUnit;
        if (skillPanel != null) skillPanel.SetActive(battleManager.IsSkillSelectionOpen);
        for (int i = 0; i < skillButtons.Count; i++)
        {
            TMP_Text nameText = i < skillNameTexts.Count ? skillNameTexts[i] : null;
            TMP_Text costText = i < skillCostTexts.Count ? skillCostTexts[i] : null;
            RefreshSkill(i, player, skillButtons[i], nameText, costText);
        }
        if (skillDescriptionText != null)
        {
            int selected = battleManager.SelectedSkillIndex;
            skillDescriptionText.text = player != null && selected >= 0 && selected < player.skills.Count && player.IsSkillUnlocked(selected)
                ? player.skills[selected].description : string.Empty;
        }
        bool showTargets = battleManager.IsSelectingSkillTarget && battleManager.IsPlayerTurn;
        bool showAllyTargets = showTargets && battleManager.IsSelectingAllyTarget;
        bool showEnemyTargets = showTargets && !battleManager.IsSelectingAllyTarget;
        RefreshTarget(enemyTargetButton1, 0, showEnemyTargets);
        RefreshTarget(enemyTargetButton2, 1, showEnemyTargets);
        RefreshTarget(enemyTargetButton3, 2, showEnemyTargets);
        RefreshAllyTarget(allyTargetButton1, 0, showAllyTargets);
        RefreshAllyTarget(allyTargetButton2, 1, showAllyTargets);
        RefreshAllyTarget(allyTargetButton3, 2, showAllyTargets);
    }

    void RefreshSkill(int index, PlayerUnit player, Button button, TMP_Text nameText, TMP_Text costText)
    {
        bool unlocked = player != null && player.IsSkillUnlocked(index);
        SkillData skill = unlocked ? player.skills[index] : null;
        if (button != null)
        {
            button.gameObject.SetActive(skill != null);
            button.interactable = skill != null && player.currentMP >= skill.manaCost && battleManager.IsPlayerTurn && !battleManager.IsSelectingSkillTarget;
        }
        if (nameText != null) nameText.text = skill != null ? skill.displayName : string.Empty;
        if (costText != null) costText.text = skill != null ? "MP " + skill.manaCost : string.Empty;
    }

    void RefreshTarget(Button button, int enemyIndex, bool show)
    {
        if (button == null) return;
        CombatUnit enemy = enemyIndex < battleManager.spawnedEnemies.Count && battleManager.spawnedEnemies[enemyIndex] != null
            ? battleManager.spawnedEnemies[enemyIndex].GetComponent<CombatUnit>() : null;
        button.gameObject.SetActive(show);
        button.interactable = show && enemy != null && enemy.currentHP > 0;
    }

    void RefreshAllyTarget(Button button, int allyIndex, bool show)
    {
        if (button == null) return;
        CombatUnit ally = allyIndex < battleManager.spawnedParty.Count && battleManager.spawnedParty[allyIndex] != null
            ? battleManager.spawnedParty[allyIndex].GetComponent<CombatUnit>() : null;
        button.gameObject.SetActive(show);
        button.interactable = show && ally != null && ally.currentHP > 0;
    }
}
