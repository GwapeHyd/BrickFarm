using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;

    [Header("Données")]
    public PlayerData playerData;
    public UIManager uiManager;

    [Header("Articles du shop")]
    public List<ShopItemData> itemDataList = new List<ShopItemData>();
    public List<ShopItemUI> itemUIList = new List<ShopItemUI>();

    private void Start()
    {
        // Lie chaque UI à sa data
        for (int i = 0; i < itemDataList.Count && i < itemUIList.Count; i++)
        {
            itemUIList[i].Setup(itemDataList[i], this);
        }

        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    public void ToggleShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(!shopPanel.activeSelf);

            // Rafraîchir l'UI à l'ouverture
            if (shopPanel.activeSelf)
                RefreshAllItems();
        }
    }

    public void TryBuy(ShopItemData item)
    {
        if (item == null || !CanAfford(item) || !IsUnlocked(item)) return;

        int owned = GetOwnedQuantity(item);
        int cost = item.GetCurrentCost(owned);

        switch (item.currency)
        {
            case ShopCurrencyType.leaf:
                playerData.leaf -= cost;
                break;
            case ShopCurrencyType.coin:
                playerData.coin -= cost;
                break;
            case ShopCurrencyType.mush:
                playerData.mush -= cost;
                break;
            case ShopCurrencyType.Bush:
                var bushOwned = playerData.ownedBricks.Find(b => b.data != null && b.data.brickType == BrickType.Bush);
                if (bushOwned != null)
                    bushOwned.quantity -= cost;
                break;
        }

        // Ajouter la brique dans ownedBricks
        var ownedBrick = playerData.ownedBricks.Find(b => b.data == item.brickData);
        if (ownedBrick != null)
        {
            ownedBrick.quantity++;
        }
        else
        {
            playerData.ownedBricks.Add(new OwnedBrickData { data = item.brickData, quantity = 1 });
        }

        // Rafraîchir les UI
        uiManager.UpdateCurrencyUI();
        uiManager.UpdateBrickUI();
        RefreshAllItems();

        Debug.Log($"Acheté : {item.displayName} pour {cost} {item.currency}");
    }

    public bool CanAfford(ShopItemData item)
    {
        int owned = GetOwnedQuantity(item);
        int cost = item.GetCurrentCost(owned);

        switch (item.currency)
        {
            case ShopCurrencyType.leaf:
                return playerData.leaf >= cost;
            case ShopCurrencyType.coin:
                return playerData.coin >= cost;
            case ShopCurrencyType.mush:
                return playerData.mush >= cost;
            case ShopCurrencyType.Bush:
                var bushOwned = playerData.ownedBricks.Find(b => b.data != null && b.data.brickType == BrickType.Bush);
                return bushOwned != null && bushOwned.quantity >= cost;
            default:
                return false;
        }
    }

    public bool IsUnlocked(ShopItemData item)
    {
        if (!item.hasUnlockCondition) return true;

        var ownedBrick = playerData.ownedBricks.Find(b => b.data != null && b.data.brickType == item.requiredBrickType);
        return ownedBrick != null && ownedBrick.quantity >= item.requiredQuantity;
    }

    public int GetOwnedQuantity(ShopItemData item)
    {
        var ownedBrick = playerData.ownedBricks.Find(b => b.data == item.brickData);
        return ownedBrick != null ? ownedBrick.quantity : 0;
    }

    private void RefreshAllItems()
    {
        foreach (var ui in itemUIList)
            ui.Refresh();
    }
}