using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Displays the current formation and character status in a hand-built pause menu.</summary>
public class PartyMenuController : MonoBehaviour
{
    [System.Serializable]
    public class MemberVisual
    {
        [Tooltip("Optional row root to hide when this party slot is empty.")]
        public GameObject row;
        public Image portrait;
        public TMP_Text nameText;
        public TMP_Text levelText;
        public TMP_Text hpText;
        public TMP_Text mpText;
        public Image hpFill;
        public Image mpFill;
    }

    [Header("Panels")]
    public GameObject partyPanel;
    public GameObject pausePanel;

    [Header("Content")]
    public List<TMP_Text> memberStatusTexts = new List<TMP_Text>();
    [Tooltip("Optional hand-built JRPG rows. Assign one entry per party slot.")]
    public List<MemberVisual> memberVisuals = new List<MemberVisual>();

    [Header("Optional")]
    public Button partyButton;
    public Button backButton;

    public bool IsPartyOpen
    {
        get { return partyPanel != null && partyPanel.activeSelf; }
    }

    void Awake()
    {
        if (partyButton != null) partyButton.onClick.AddListener(OpenParty);
        if (backButton != null) backButton.onClick.AddListener(CloseParty);
        // Refresh once at runtime so an already-open/test-visible panel does
        // not keep the placeholder text from a previous scene state.
        if (Application.isPlaying) RefreshParty();
    }

    public void OpenParty()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (partyPanel != null) partyPanel.SetActive(true);
        RefreshParty();
    }

    public void CloseParty()
    {
        if (partyPanel != null) partyPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void RefreshParty()
    {
        // GameManager is normally a DontDestroyOnLoad singleton. The scene
        // object can still be found directly during initialization (or when
        // entering Play Mode before its Awake has assigned instance).
        GameManager manager = GameManager.instance;
        if (manager == null) manager = FindObjectOfType<GameManager>();
        if (manager == null) return;

        // Older scene data intentionally has an empty partyMembers list: the
        // protagonist is stored in GameManager's legacy scalar fields. Treat
        // that state as a one-member party until real party prefabs are added.
        List<GameObject> members = manager.partyMembers;
        int listedMemberCount = members != null ? members.Count : 0;
        for (int i = 0; i < memberStatusTexts.Count; i++)
        {
            if (memberStatusTexts[i] == null) continue;
            if (listedMemberCount != 0 && i >= listedMemberCount) continue;
            if (listedMemberCount == 0 && i == 0)
            {
                memberStatusTexts[i].text = string.Format(
                    "1. {0}\nLV {1}\nHP {2}/{3}    MP {4}/{5}\nATK {6}    DEF {7}    SPD {8}",
                    manager.playerName, manager.level, manager.currentHP, manager.maxHP,
                    manager.currentMP, manager.maxMP, manager.attackPower, manager.defense, manager.speed);
                continue;
            }
            if (i >= listedMemberCount || members[i] == null)
            {
                memberStatusTexts[i].text = "EMPTY SLOT";
                continue;
            }

            PlayerUnit player = members[i].GetComponent<PlayerUnit>();
            if (player == null)
            {
                memberStatusTexts[i].text = members[i].name + "\nCOMBAT DATA UNAVAILABLE";
                continue;
            }

            memberStatusTexts[i].text = string.Format(
                "{0}. {1}\nLV {2}\nHP {3}/{4}    MP {5}/{6}\nATK {7}    DEF {8}    SPD {9}",
                i + 1, player.unitName, player.level, player.currentHP, player.maxHP,
                player.currentMP, player.maxMP, player.attackPower, player.defense, player.speed);
        }

        for (int i = 0; i < memberVisuals.Count; i++)
        {
            MemberVisual visual = memberVisuals[i];
            if (visual == null) continue;
            PlayerUnit player = null;
            if (listedMemberCount == 0 && i == 0)
            {
                ApplyManagerVisual(visual, manager);
                continue;
            }
            else if (members != null && i < listedMemberCount && members[i] != null)
            {
                player = members[i].GetComponent<PlayerUnit>();
            }
            ApplyMemberVisual(visual, player, i);
        }
    }

    void ApplyMemberVisual(MemberVisual visual, PlayerUnit player, int index)
    {
        if (player == null)
        {
            if (visual.nameText != null) visual.nameText.text = "EMPTY SLOT";
            if (visual.levelText != null) visual.levelText.text = string.Empty;
            if (visual.hpText != null) visual.hpText.text = string.Empty;
            if (visual.mpText != null) visual.mpText.text = string.Empty;
            if (visual.hpFill != null) visual.hpFill.fillAmount = 0f;
            if (visual.mpFill != null) visual.mpFill.fillAmount = 0f;
            return;
        }
        if (visual.nameText != null) visual.nameText.text = string.Format("{0}. {1}", index + 1, player.unitName);
        if (visual.levelText != null) visual.levelText.text = string.Format("LV {0}", player.level);
        if (visual.hpText != null) visual.hpText.text = string.Format("{0}/{1}", player.currentHP, player.maxHP);
        if (visual.mpText != null) visual.mpText.text = string.Format("{0}/{1}", player.currentMP, player.maxMP);
        if (visual.hpFill != null) visual.hpFill.fillAmount = player.maxHP > 0 ? Mathf.Clamp01((float)player.currentHP / player.maxHP) : 0f;
        if (visual.mpFill != null) visual.mpFill.fillAmount = player.maxMP > 0 ? Mathf.Clamp01((float)player.currentMP / player.maxMP) : 0f;
    }

    void ApplyManagerVisual(MemberVisual visual, GameManager manager)
    {
        if (visual.nameText != null) visual.nameText.text = "1. " + manager.playerName;
        if (visual.levelText != null) visual.levelText.text = "LV " + manager.level;
        if (visual.hpText != null) visual.hpText.text = string.Format("{0}/{1}", manager.currentHP, manager.maxHP);
        if (visual.mpText != null) visual.mpText.text = string.Format("{0}/{1}", manager.currentMP, manager.maxMP);
        if (visual.hpFill != null) visual.hpFill.fillAmount = manager.maxHP > 0 ? Mathf.Clamp01((float)manager.currentHP / manager.maxHP) : 0f;
        if (visual.mpFill != null) visual.mpFill.fillAmount = manager.maxMP > 0 ? Mathf.Clamp01((float)manager.currentMP / manager.maxMP) : 0f;
    }

}
