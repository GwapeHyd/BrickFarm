using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeItemUI : MonoBehaviour
{
    [Header("Références UI")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;        // "10 mush"
    public Button upgradeButton;            // bouton +
    public Button itemClickArea;            // zone cliquable pour la description

    [Header("Description (partagée)")]
    public GameObject descriptionPanel;
    public TextMeshProUGUI descriptionText;

    private UpgradeData data;
    private UpgradeManager manager;

    public void Setup(UpgradeData upgradeData, UpgradeManager upgradeManager)
    {
        data = upgradeData;
        manager = upgradeManager;

        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(() => manager.TryBuyUpgrade(data));

        if (itemClickArea != null)
        {
            itemClickArea.onClick.RemoveAllListeners();
            itemClickArea.onClick.AddListener(ShowDescription);
        }

        Refresh();
    }

    public void Refresh()
    {
        if (data == null || manager == null) return;

        int level = manager.GetLevel(data);
        levelText.text = $"Lv {level}";
        nameText.text = data.upgradeName;

        int cost = data.GetCost(level);
        costText.text = CurrencyFormatter.Format(cost);

        upgradeButton.interactable = manager.CanAfford(data);
    }

    void ShowDescription()
    {
        if (descriptionPanel == null || descriptionText == null) return;

        int level = manager.GetLevel(data);
        float chancePct = data.GetChance(level) * 100f;

        descriptionText.text =
            $"<b>{data.upgradeName}</b>  (Lv {level})\n" +
            $"{data.description}\n" +
            $"Chance actuelle : {chancePct:0}%";

        TogglePanel();
    }

    void TogglePanel()
    {
        if (descriptionPanel != null)
            descriptionPanel.SetActive(!descriptionPanel.activeSelf);
    }
}