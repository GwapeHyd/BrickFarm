using UnityEngine;

public enum ShopCurrencyType { leaf, coin, Bush }

[CreateAssetMenu(fileName = "New ShopItem", menuName = "BrickFarm/ShopItemData")]
public class ShopItemData : ScriptableObject
{
    public string displayName;
    public BrickData brickData;

    [Header("Prix")]
    public int baseCost = 1;
    public float costMultiplier = 2f;
    public ShopCurrencyType currency = ShopCurrencyType.leaf;

    [Header("Condition de déverrouillage")]
    public bool hasUnlockCondition = false;
    public BrickType requiredBrickType = BrickType.Bush;
    public int requiredQuantity = 10;

    // Coût actuel selon le nombre déjà acheté
    public int GetCurrentCost(int alreadyOwned)
    {
        if (currency == ShopCurrencyType.leaf)
            return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, alreadyOwned - 5));
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, alreadyOwned));
    }

}