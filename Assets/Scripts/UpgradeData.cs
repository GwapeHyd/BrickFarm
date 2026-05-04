using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade", menuName = "BrickFarm/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    public string upgradeName;
    [TextArea] public string description;

    [Header("Coût (x10 par niveau)")]
    public int baseCost = 1;

    [Header("Effet")]
    public float baseChance = 0f;    
    public float chancePerLevel = 0.10f; // +10% par niveau
    public int baseSpores = 0;
    public int sporesPerLevel = 1; // +1 spore par niveau

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

    /// <summary>Nombre total de spores au niveau donné.</summary>
    public int GetSporeCount(int level)
    {
        return baseSpores + sporesPerLevel * level;
    }
}