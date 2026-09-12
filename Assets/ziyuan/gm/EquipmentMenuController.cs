using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Hand-wired per-character equipment screen reached from the party menu.</summary>
public class EquipmentMenuController : MonoBehaviour
{
    private static EquipmentMenuController instance;

    [Header("Character Panel")]
    public GameObject characterPanel;
    public TMP_Text characterNameText;
    public TMP_Text levelText;
    public TMP_Text hpText;
    public TMP_Text mpText;
    public TMP_Text atkText;
    public TMP_Text defText;
    public Button weaponSlotButton;
    public TMP_Text weaponSlotText;
    public Button armorSlotButton;
    public TMP_Text armorSlotText;
    public Button backButton;

    [Header("Equipment List")]
    public GameObject equipmentListPanel;
    public TMP_Text listTitleText;
    public RectTransform listContent;
    public Button equipmentButtonPrefab;
    public TMP_Text listEmptyText;
    public Button listBackButton;

    private string currentCharacterId;
    private GameObject returnPanel;
    private EquipmentSlot currentSlot;

    public static void Open(string characterId, GameObject returnPanel)
    {
        if (instance == null) instance = FindObjectOfType<EquipmentMenuController>();
        if (instance != null) instance.OpenForCharacter(characterId, returnPanel);
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        Bind();
        if (characterPanel != null) characterPanel.SetActive(false);
        if (equipmentListPanel != null) equipmentListPanel.SetActive(false);
    }

    void Bind()
    {
        if (weaponSlotButton != null) weaponSlotButton.onClick.AddListener(() => OpenList(EquipmentSlot.Weapon));
        if (armorSlotButton != null) armorSlotButton.onClick.AddListener(() => OpenList(EquipmentSlot.Armor));
        if (backButton != null) backButton.onClick.AddListener(CloseCharacter);
        if (listBackButton != null) listBackButton.onClick.AddListener(CloseList);
    }

    void OpenForCharacter(string characterId, GameObject returnPanel)
    {
        currentCharacterId = string.IsNullOrEmpty(characterId) ? "craven" : characterId;
        this.returnPanel = returnPanel;
        if (returnPanel != null) returnPanel.SetActive(false);
        if (equipmentListPanel != null) equipmentListPanel.SetActive(false);
        if (characterPanel != null) characterPanel.SetActive(true);
        RefreshCharacter();
    }

    public void CloseCharacter()
    {
        if (characterPanel != null) characterPanel.SetActive(false);
        if (equipmentListPanel != null) equipmentListPanel.SetActive(false);
        if (returnPanel != null) returnPanel.SetActive(true);
    }

    void OpenList(EquipmentSlot slot)
    {
        currentSlot = slot;
        if (characterPanel != null) characterPanel.SetActive(false);
        if (equipmentListPanel != null) equipmentListPanel.SetActive(true);
        RefreshList();
    }

    void CloseList()
    {
        if (equipmentListPanel != null) equipmentListPanel.SetActive(false);
        if (characterPanel != null) characterPanel.SetActive(true);
        RefreshCharacter();
    }

    public bool TryCloseTopMost()
    {
        if (equipmentListPanel != null && equipmentListPanel.activeSelf)
        {
            CloseList();
            return true;
        }
        if (characterPanel != null && characterPanel.activeSelf)
        {
            CloseCharacter();
            return true;
        }
        return false;
    }

    void RefreshCharacter()
    {
        CharacterStatus status;
        if (CharacterStatusService.TryGet(currentCharacterId, out status))
        {
            if (characterNameText != null) characterNameText.text = status.displayName;
            if (levelText != null) levelText.text = "LV " + status.level;
            if (hpText != null) hpText.text = "HP " + status.currentHP + "/" + status.maxHP;
            if (mpText != null) mpText.text = "MP " + status.currentMP + "/" + status.maxMP;
            if (atkText != null) atkText.text = "ATK " + status.attack;
            if (defText != null) defText.text = "DEF " + status.defense;
        }
        else
        {
            if (characterNameText != null) characterNameText.text = "UNKNOWN";
            if (levelText != null) levelText.text = "LV --";
            if (hpText != null) hpText.text = "HP --/--";
            if (mpText != null) mpText.text = "MP --/--";
            if (atkText != null) atkText.text = "ATK --";
            if (defText != null) defText.text = "DEF --";
        }

        CharacterLoadout loadout = EquipmentManager.GetOrCreate().GetOrCreateLoadout(currentCharacterId);
        if (weaponSlotText != null) weaponSlotText.text = loadout.weapon != null ? loadout.weapon.displayName : "None";
        if (armorSlotText != null) armorSlotText.text = loadout.armor != null ? loadout.armor.displayName : "None";
    }

    void RefreshList()
    {
        if (listTitleText != null) listTitleText.text = currentSlot == EquipmentSlot.Weapon ? "WEAPON" : "ARMOR";

        ClearListContent();

        EquipmentManager manager = EquipmentManager.GetOrCreate();
        CharacterLoadout loadout = manager.GetOrCreateLoadout(currentCharacterId);
        EquipmentData equipped = currentSlot == EquipmentSlot.Weapon ? loadout.weapon : loadout.armor;

        if (equipped != null) AddListButton(null, true);

        int shown = 0;
        if (manager.ownedEquipment != null)
        {
            for (int i = 0; i < manager.ownedEquipment.Count; i++)
            {
                EquipmentData item = manager.ownedEquipment[i];
                if (item == null || item.slot != currentSlot) continue;
                AddListButton(item, false);
                shown++;
            }
        }

        bool empty = equipped == null && shown == 0;
        if (listEmptyText != null) listEmptyText.gameObject.SetActive(empty);
    }

    void ClearListContent()
    {
        if (listContent == null) return;
        for (int i = listContent.childCount - 1; i >= 0; i--)
        {
            Transform child = listContent.GetChild(i);
            Destroy(child.gameObject);
        }
    }

    void AddListButton(EquipmentData item, bool isUnequip)
    {
        if (listContent == null || equipmentButtonPrefab == null) return;

        Button button = Instantiate(equipmentButtonPrefab, listContent, false);
        if (button == null) return;
        TMP_Text label = button.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            if (isUnequip)
            {
                label.text = "None (Unequip)";
            }
            else
            {
                string bonus = item.attackBonus > 0
                    ? "+" + item.attackBonus + " ATK"
                    : "+" + item.defenseBonus + " DEF";
                label.text = item.displayName + "   " + bonus;
            }
        }

        EquipmentData selected = item;
        button.onClick.AddListener(() => ChooseEquipment(selected));
    }

    void ChooseEquipment(EquipmentData item)
    {
        EquipmentManager manager = EquipmentManager.GetOrCreate();
        if (item == null) manager.Unequip(currentCharacterId, currentSlot);
        else manager.Equip(currentCharacterId, item);
        CloseList();
    }
}
