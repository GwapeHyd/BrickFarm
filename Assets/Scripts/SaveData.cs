using System;
using System.Collections.Generic;

/// <summary>Snapshot complet de l'état du joueur, sérialisable en JSON.</summary>
[Serializable]
public class SaveData
{
    // ── Currencies ───────────────────────────────────────────────────────────
    public int leaf;
    public int mush;
    public int coin;    public int skillPoint;
    // ── Stats ────────────────────────────────────────────────────────────────
    public int damage;
    public float damageMultiplier;

    // ── Bricks possédées ─────────────────────────────────────────────────────
    public List<BrickSaveEntry> ownedBricks = new List<BrickSaveEntry>();

    // ── Upgrades ─────────────────────────────────────────────────────────────
    public List<UpgradeSaveEntry> upgrades = new List<UpgradeSaveEntry>();
}

[Serializable]
public class BrickSaveEntry
{
    public string brickId;   // correspond à BrickData.id
    public int quantity;
}

[Serializable]
public class UpgradeSaveEntry
{
    public string upgradeName;  // correspond à UpgradeData.upgradeName
    public int level;
}