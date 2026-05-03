using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Brick", menuName = "Brick/BrickData")]
public class BrickData : ScriptableObject
{
    public string id;
    public int maxHP;
    public Sprite icon;
    public GameObject prefab;
    public BrickType brickType;

    [Header("Loot Settings")]
    public List<LootChance> lootOnHit;
    public List<LootChance> lootOnDestroy;
}

[System.Serializable]
public class LootChance
{
    public Sprite icon; // Ex: pièce, champignon
    public int amount;
    [Range(0f, 1f)] public float chance;
}