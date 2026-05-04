using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeItemUI : MonoBehaviour
{
    [Header("Références UI")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;        
    public Button upgradeButton;           
    public Button itemClickArea;            

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
        bool maxed = data.IsMaxed(level);
        bool isOneShot = data.maxLevel == 1;

        if (maxed)
        {
            levelText.text = $"<color=#FFD700>MAX</color>";
            nameText.text = $"<color=#FFD700>{data.upgradeName}</color>";
            if (costText != null)
                costText.gameObject.SetActive(false);
            if (upgradeButton != null)
                upgradeButton.gameObject.SetActive(false);
        }
        else
        {
            if (costText != null)
                costText.gameObject.SetActive(true);
            if (upgradeButton != null)
                upgradeButton.gameObject.SetActive(true);
            
            levelText.text = isOneShot ? "" : $"Lv {level}";
            nameText.text = data.upgradeName;

            if (costText != null)
            {
                int cost = data.GetCost(level);
                costText.text = CurrencyFormatter.Format(cost);
            }

            upgradeButton.interactable = manager.CanAfford(data);
        }
    }

    void ShowDescription()
    {
        if (descriptionPanel == null || descriptionText == null) return;

        int level = manager.GetLevel(data);
        string extra = data.IsMaxed(level) ? "\n<color=#FFD700>— ACTIVÉ —</color>" : "";
        float chancePct = data.GetChance(level) * 100f;

        descriptionText.text =
            $"<b>{data.upgradeName}</b>  (Lv {level})\n" +
            $"{data.description}\n" +
            (chancePct > 0 ? $"\nChance actuelle : <b>{chancePct:0}%" : "") + extra;

        TogglePanel();
    }

    void TogglePanel()
    {
        if (descriptionPanel != null)
            descriptionPanel.SetActive(!descriptionPanel.activeSelf);
    }
}