using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    [Header("Références UI")]
    public Image iconImage;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI costText;
    public Button buyButton;

    private ShopItemData itemData;
    private ShopManager shopManager;

    public void Setup(ShopItemData data, ShopManager manager)
    {
        itemData = data;
        shopManager = manager;

        if (iconImage != null && data.brickData != null)
            iconImage.sprite = data.brickData.icon;

        if (itemNameText != null)
            itemNameText.text = data.displayName;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => shopManager.TryBuy(itemData));

        Refresh();
    }

    public void Refresh()
    {
        if (itemData == null || shopManager == null) return;

        int owned = shopManager.GetOwnedQuantity(itemData);
        int cost = itemData.GetCurrentCost(owned);

        if (costText != null)
            costText.text = CurrencyFormatter.Format(cost);

        // Griser le bouton si pas assez de resources
        bool canAfford = shopManager.CanAfford(itemData);
        buyButton.interactable = canAfford;
    }
}