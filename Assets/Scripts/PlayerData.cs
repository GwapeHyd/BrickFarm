using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class OwnedBrickData
{
    public BrickData data;
    public int quantity;
    public int maxQuantity = 20;
}

[CreateAssetMenu(fileName = "PlayerData", menuName = "BrickFarm/PlayerData", order = 1)]
public class PlayerData : ScriptableObject
{
    [Header("Bricks Possédées")]
    public List<OwnedBrickData> ownedBricks = new List<OwnedBrickData>();

    [Header("Stats")]
    public int damage;
    public float damageMultiplier;

    [Header("Currencies")]
    public int leaf = 0;
    public int coin = 0;

#if UNITY_EDITOR
    [ContextMenu("Reset Player Data")]
    private void ResetPlayerData()
    {
        leaf = 0;
        coin = 0;
        damage = 0;
        damageMultiplier = 1f;
        foreach (var ownedBrick in ownedBricks)
        {
            ownedBrick.quantity = 0;
        }
    }
#endif
}
