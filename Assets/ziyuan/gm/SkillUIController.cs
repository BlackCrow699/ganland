using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUIController : MonoBehaviour
{
    [Header("References")]
    public BattleManager battleManager;
    public GameObject skillPanel;

    [Header("Skills")]
    public Button normalSkillButton;
    public Button ultimateSkillButton;
    public Button healSkillButton;

    [Header("Skill Names")]
    public TMP_Text normalSkillText;
    public TMP_Text ultimateSkillText;
    public TMP_Text healSkillText;

    [Header("Skill MP Costs")]
    public TMP_Text normalSkillCostText;
    public TMP_Text ultimateSkillCostText;
    public TMP_Text healSkillCostText;

    [Header("Description")]
    public TMP_Text skillDescriptionText;
    public Button skillBackButton;

    [Header("Enemy Targets")]
    public Button enemyTargetButton1;
    public Button enemyTargetButton2;
    public Button enemyTargetButton3;
    private bool listenersBound;

    void Awake()
    {
        if (battleManager == null) battleManager = FindObjectOfType<BattleManager>();
        BindListeners();
    }

    void Update() { if (battleManager != null) Refresh(); }

    void BindListeners()
    {
        if (listenersBound || battleManager == null) return;
        listenersBound = true;
        if (normalSkillButton != null) normalSkillButton.onClick.AddListener(() => battleManager.UseSkill(0));
        if (ultimateSkillButton != null) ultimateSkillButton.onClick.AddListener(() => battleManager.UseSkill(1));
        if (healSkillButton != null) healSkillButton.onClick.AddListener(() => battleManager.UseSkill(2));
        if (skillBackButton != null) skillBackButton.onClick.AddListener(battleManager.CancelSkillSelection);
        if (enemyTargetButton1 != null) enemyTargetButton1.onClick.AddListener(() => battleManager.SelectSkillTarget(0));
        if (enemyTargetButton2 != null) enemyTargetButton2.onClick.AddListener(() => battleManager.SelectSkillTarget(1));
        if (enemyTargetButton3 != null) enemyTargetButton3.onClick.AddListener(() => battleManager.SelectSkillTarget(2));
    }

    void Refresh()
    {
        PlayerUnit player = battleManager.currentActor as PlayerUnit;
        if (skillPanel != null) skillPanel.SetActive(battleManager.IsSkillSelectionOpen);
        RefreshSkill(0, player, normalSkillButton, normalSkillText, normalSkillCostText);
        RefreshSkill(1, player, ultimateSkillButton, ultimateSkillText, ultimateSkillCostText);
        RefreshSkill(2, player, healSkillButton, healSkillText, healSkillCostText);
        if (skillDescriptionText != null)
        {
            int selected = battleManager.SelectedSkillIndex;
            skillDescriptionText.text = player != null && selected >= 0 && selected < player.skills.Count && player.IsSkillUnlocked(selected) ? player.skills[selected].description : string.Empty;
        }
        bool showTargets = battleManager.IsSelectingSkillTarget && battleManager.IsPlayerTurn;
        RefreshTarget(enemyTargetButton1, 0, showTargets);
        RefreshTarget(enemyTargetButton2, 1, showTargets);
        RefreshTarget(enemyTargetButton3, 2, showTargets);
    }

    void RefreshSkill(int index, PlayerUnit player, Button button, TMP_Text nameText, TMP_Text costText)
    {
        bool unlocked = player != null && player.IsSkillUnlocked(index);
        SkillDefinition skill = unlocked ? player.skills[index] : null;
        if (button != null)
        {
            button.gameObject.SetActive(unlocked);
            button.interactable = unlocked && player.currentMP >= skill.mpCost && battleManager.IsPlayerTurn && !battleManager.IsSelectingSkillTarget;
        }
        if (nameText != null) nameText.text = skill != null ? skill.skillName : string.Empty;
        if (costText != null) costText.text = skill != null ? "MP " + skill.mpCost : string.Empty;
    }

    void RefreshTarget(Button button, int enemyIndex, bool show)
    {
        if (button == null) return;
        CombatUnit enemy = enemyIndex < battleManager.spawnedEnemies.Count && battleManager.spawnedEnemies[enemyIndex] != null ? battleManager.spawnedEnemies[enemyIndex].GetComponent<CombatUnit>() : null;
        button.gameObject.SetActive(show);
        button.interactable = show && enemy != null && enemy.currentHP > 0;
    }
}
