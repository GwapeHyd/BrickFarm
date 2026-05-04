using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gère la sauvegarde et le chargement via PlayerPrefs (JSON).
/// Appelé par GameManager au démarrage (Load) et à chaque modification importante (Save).
/// </summary>
public class SaveManager : MonoBehaviour
{
    private const string SAVE_KEY = "BrickFarm_Save";

    [Header("Références")]
    public PlayerData playerData;
    public UpgradeManager upgradeManager;

    // ── Liste des BrickData connus (pour retrouver l'asset par id au chargement)
    [Tooltip("Assigner ici tous les BrickData assets existants dans le projet.")]
    public List<BrickData> allBrickDataAssets = new List<BrickData>();

    // ────────────────────────────────────────────────────────────────────────
    #region Save

    public void Save()
    {
        SaveData data = new SaveData();

        // Currencies
        data.leaf     = playerData.leaf;
        data.mush = playerData.mush;
        data.coin     = playerData.coin;

        // Stats
        data.damage           = playerData.damage;
        data.damageMultiplier = playerData.damageMultiplier;

        // Bricks
        foreach (var entry in playerData.ownedBricks)
        {
            if (entry.data == null) continue;
            data.ownedBricks.Add(new BrickSaveEntry
            {
                brickId  = entry.data.id,
                quantity = entry.quantity
            });
        }

        // Upgrades
        if (upgradeManager != null)
        {
            foreach (var upgrade in upgradeManager.upgradeDataList)
            {
                data.upgrades.Add(new UpgradeSaveEntry
                {
                    upgradeName = upgrade.upgradeName,
                    level       = upgradeManager.GetLevel(upgrade)
                });
            }
        }

        string json = JsonUtility.ToJson(data, prettyPrint: false);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();

        Debug.Log($"[SaveManager] Sauvegarde OK — {data.ownedBricks.Count} bricks, {data.upgrades.Count} upgrades.");
    }

    #endregion

    // ────────────────────────────────────────────────────────────────────────
    #region Load

    public void Load()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY))
        {
            Debug.Log("[SaveManager] Aucune sauvegarde trouvée — partie fraîche.");
            return;
        }

        string json = PlayerPrefs.GetString(SAVE_KEY);

        SaveData data;
        try { data = JsonUtility.FromJson<SaveData>(json); }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] JSON corrompu : {e.Message}");
            return;
        }

        // Currencies
        playerData.leaf     = data.leaf;
        playerData.mush = data.mush;
        playerData.coin     = data.coin;

        // Stats
        playerData.damage           = data.damage;
        playerData.damageMultiplier = data.damageMultiplier;

        // Bricks — on remet les quantités à zéro d'abord
        foreach (var owned in playerData.ownedBricks)
            owned.quantity = 0;

        foreach (var saved in data.ownedBricks)
        {
            // Trouve l'entrée existante par id
            var owned = playerData.ownedBricks.Find(
                b => b.data != null && b.data.id == saved.brickId);

            if (owned != null)
            {
                owned.quantity = saved.quantity;
            }
            else
            {
                // L'asset n'est pas encore dans ownedBricks — on le crée depuis allBrickDataAssets
                BrickData asset = allBrickDataAssets.Find(a => a.id == saved.brickId);
                if (asset != null)
                {
                    playerData.ownedBricks.Add(new OwnedBrickData
                    {
                        data     = asset,
                        quantity = saved.quantity
                    });
                }
                else
                {
                    Debug.LogWarning($"[SaveManager] BrickData introuvable pour id='{saved.brickId}'");
                }
            }
        }

        // Upgrades
        if (upgradeManager != null)
        {
            foreach (var saved in data.upgrades)
            {
                var upgrade = upgradeManager.upgradeDataList.Find(
                    u => u.upgradeName == saved.upgradeName);

                if (upgrade != null)
                {
                    upgradeManager.SetLevel(upgrade, saved.level);
                    Debug.Log($"[SaveManager] Upgrade chargée : '{upgrade.upgradeName}' Lv {saved.level}");
                }
                else
                    Debug.LogWarning($"[SaveManager] Upgrade introuvable : '{saved.upgradeName}'");
            }

            upgradeManager.ApplyLoadedLevels();
        }

        Debug.Log("[SaveManager] Chargement OK.");
    }

    #endregion

    // ────────────────────────────────────────────────────────────────────────
    #region Helpers

    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
        Debug.Log("[SaveManager] Sauvegarde supprimée.");
    }

    public bool HasSave() => PlayerPrefs.HasKey(SAVE_KEY);

    #endregion
}