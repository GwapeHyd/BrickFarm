using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade", menuName = "BrickFarm/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    public string upgradeName;
    [TextArea] public string description;

    [Header("Coût (x10 par niveau)")]
    public int baseCost = 1;
    public int maxLevel = 0;

    [Header("Effet")]
    public float baseChance = 0f;    
    public float chancePerLevel = 0.10f; // +10% par niveau
    public int baseSpores = 0;
    public int sporesPerLevel = 1; // +1 spore par niveau

    public int GetCost(int currentLevel)
    {
        return Mathf.RoundToInt(baseCost * Mathf.Pow(10, currentLevel));
    }

    public float GetChance(int level)
    {
        return baseChance + chancePerLevel * level;
    }

    public int GetSporeCount(int level)
    {
        return baseSpores + sporesPerLevel * level;
    }

    public bool IsMaxed(int level)
    {
        return maxLevel > 0 && level >= maxLevel;
    }
}