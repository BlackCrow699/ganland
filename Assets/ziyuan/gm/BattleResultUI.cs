using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Victory settlement window for EXP, level and item drops.</summary>
[ExecuteAlways]
public class BattleResultUI : MonoBehaviour
{
    public BattleManager battleManager;
    public GameObject resultPanel;
    public TMP_Text resultText;
    public Button continueButton;

    private static readonly Color ButtonColor = new Color(0.1f, 0.13f, 0.22f, 1f);

    void Awake()
    {
        if (battleManager == null) battleManager = FindObjectOfType<BattleManager>();
        BuildIfMissing();
        if (continueButton != null) continueButton.onClick.AddListener(Continue);
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    void Update()
    {
        if (battleManager == null) battleManager = FindObjectOfType<BattleManager>();
        if (resultPanel == null) BuildIfMissing();
        if (battleManager == null || resultPanel == null) return;
        bool show = battleManager.battleFinished && battleManager.playerWon && battleManager.resultReady;
        if (show && !resultPanel.activeSelf) Refresh();
        resultPanel.SetActive(show);
        if (show && continueButton != null && !continueButton.gameObject.activeSelf)
            continueButton.gameObject.SetActive(true);
    }

    void Refresh()
    {
        if (resultText == null) return;
        resultText.text = string.Format(
            "VICTORY!\n\nEXP GAINED   +{0}\nLEVEL   {1} -> {2}\n\nDROPS\n{3}",
            battleManager.expRewardGained,
            battleManager.levelBeforeReward,
            battleManager.levelAfterReward,
            battleManager.rewardDropSummary);
    }

    public void Continue()
    {
        if (battleManager != null) battleManager.ContinueAfterResult();
    }

    void BuildIfMissing()
    {
        if (resultPanel != null) return;
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        resultPanel = new GameObject("BattleResultPanel_Auto");
        resultPanel.transform.SetParent(canvas.transform, false);
        resultPanel.transform.localScale = Vector3.one;
        resultPanel.transform.SetAsLastSibling();
        Image image = resultPanel.AddComponent<Image>();
        image.color = new Color(0.025f, 0.045f, 0.09f, 0.98f);
        Outline outline = resultPanel.AddComponent<Outline>();
        outline.effectColor = new Color(0.95f, 0.72f, 0.28f, 1f);
        outline.effectDistance = new Vector2(3f, 3f);
        RectTransform panelRect = resultPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(700f, 500f);
        Canvas panelCanvas = resultPanel.AddComponent<Canvas>();
        panelCanvas.overrideSorting = true;
        panelCanvas.sortingOrder = 1000;
        resultPanel.AddComponent<GraphicRaycaster>();

        resultText = UIHelper.CreateText(resultPanel.transform, "", 22, new Vector2(0f, 35f), new Vector2(620f, 350f), TextAlignmentOptions.Center);
        continueButton = UIHelper.CreateButton(resultPanel.transform, "CONTINUE", new Vector2(0f, -205f), new Vector2(220f, 56f), new Vector2(210f, 48f), 20, ButtonColor);
    }
}
