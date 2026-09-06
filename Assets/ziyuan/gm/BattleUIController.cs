using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Connects a hand-built CombatScene UI to BattleManager.
/// Drag the scene references into the Inspector; no UI hierarchy is created here.
/// </summary>
public class BattleUIController : MonoBehaviour
{
    [Header("References")]
    public BattleManager battleManager;
    public TMP_Text turnText;
    public TMP_Text messageText;
    public List<TMP_Text> partyStatusTexts = new List<TMP_Text>();
    public List<TMP_Text> enemyStatusTexts = new List<TMP_Text>();
    public List<Button> commandButtons = new List<Button>();
    private bool lastBattleFinished;

    void Awake()
    {
        transform.localScale = Vector3.one;
        if (battleManager == null) battleManager = FindObjectOfType<BattleManager>();
        // BattleCanvas already owns this controller in the scene. Attach the
        // result UI here so settlement is available even if dynamic Canvas
        // lookup is affected by scene hierarchy changes.
        if (GetComponent<BattleResultUI>() == null)
        {
            gameObject.AddComponent<BattleResultUI>();
        }
    }

    void Update()
    {
        if (battleManager == null) return;

        RefreshTurnText();
        RefreshUnits(battleManager.spawnedParty, partyStatusTexts);
        RefreshUnits(battleManager.spawnedEnemies, enemyStatusTexts);

        bool canAct = battleManager.IsPlayerTurn && !battleManager.battleFinished && !battleManager.IsSkillSelectionOpen && !battleManager.IsSelectingSkillTarget;
        for (int i = 0; i < commandButtons.Count; i++)
        {
            if (commandButtons[i] != null) commandButtons[i].interactable = canAct;
        }

        if (battleManager.battleFinished != lastBattleFinished)
        {
            lastBattleFinished = battleManager.battleFinished;
            if (messageText != null)
            {
                messageText.text = battleManager.battleFinished ? "BATTLE FINISHED" : "CHOOSE ACTION";
            }
        }
    }

    void RefreshTurnText()
    {
        if (turnText == null) return;
        turnText.text = string.Format("TURN {0}", battleManager.turnNumber);

        if (messageText != null && !battleManager.battleFinished)
        {
            messageText.text = battleManager.IsPlayerTurn ? "CHOOSE ACTION" : "ENEMY TURN...";
        }
    }

    void RefreshUnits(List<GameObject> objects, List<TMP_Text> labels)
    {
        for (int i = 0; i < labels.Count; i++)
        {
            if (labels[i] == null) continue;
            CombatUnit unit = i < objects.Count && objects[i] != null
                ? objects[i].GetComponent<CombatUnit>()
                : null;

            if (unit == null)
            {
                labels[i].text = string.Empty;
                continue;
            }

            PlayerUnit player = unit as PlayerUnit;
            string state = unit.currentHP <= 0
                ? "DOWN"
                : player != null
                    ? string.Format("HP {0}/{1}    MP {2}/{3}    SPD {4}", unit.currentHP, unit.maxHP, player.currentMP, player.maxMP, unit.speed)
                    : string.Format("HP {0}/{1}    SPD {2}", unit.currentHP, unit.maxHP, unit.speed);
            labels[i].text = unit.unitName + "\n" + state;
        }
    }

}
