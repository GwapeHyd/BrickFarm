using UnityEngine;

[CreateAssetMenu(fileName = "New ShopItem", menuName = "BrickFarm/ShopItemData")]
public class ShopItemData : ScriptableObject
{
    public string displayName;
    public BrickData brickData;

    [Header("Prix")]
    public int baseCost = 1;
    public float costMultiplier = 2f;
    public CurrencyType currency = CurrencyType.leaf;

    // Coût actuel selon le nombre déjà acheté
    public int GetCurrentCost(int alreadyOwned)
    {
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, alreadyOwned));
    }


    public int GetCurrentCostLeaf(int alreadyOwned)
    {
        if (currency != CurrencyType.leaf) return -1;
        return GetCurrentCost(alreadyOwned - 5);
    }
}