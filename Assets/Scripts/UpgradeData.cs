using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade", menuName = "BrickFarm/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    public string upgradeName;
    [TextArea] public string description;

    [Header("Coût (x10 par niveau)")]
    public int baseCost = 10;

    [Header("Effet")]
    public float baseChance = 0.05f;    // 5% de base
    public float chancePerLevel = 0.02f; // +2% par niveau

    /// <summary>Coût pour passer du niveau currentLevel au niveau suivant.</summary>
    public int GetCost(int currentLevel)
    {
        return Mathf.RoundToInt(baseCost * Mathf.Pow(10, currentLevel));
    }

    /// <summary>Chance totale (0..1) au niveau donné.</summary>
    public float GetChance(int level)
    {
        return baseChance + chancePerLevel * level;
    }
}