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
        if (item == null || !CanAfford(item)) return;

        int owned = GetOwnedQuantity(item);
        int cost = item.GetCurrentCostLeaf(owned);

        // Déduire la monnaie
        if (item.currency == CurrencyType.leaf)
            playerData.leaf -= cost;
        else if (item.currency == CurrencyType.coin)
            playerData.coin -= cost;

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
        int cost = item.GetCurrentCostLeaf(owned);

        if (item.currency == CurrencyType.leaf)
            return playerData.leaf >= cost;
        else if (item.currency == CurrencyType.coin)
            return playerData.coin >= cost;

        return false;
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