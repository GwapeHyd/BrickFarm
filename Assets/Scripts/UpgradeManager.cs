using UnityEngine;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    [Header("Données")]
    public PlayerData playerData;
    public UIManager uiManager;

    [Header("Upgrades")]
    public List<UpgradeData> upgradeDataList = new List<UpgradeData>();
    public List<UpgradeItemUI> upgradeUIList = new List<UpgradeItemUI>();

    private Dictionary<UpgradeData, int> levels = new Dictionary<UpgradeData, int>();

    private void Awake()
    {
        // Initialise le dictionnaire à 0 — sera écrasé par SaveManager.Load() si save existante
        levels.Clear();
        foreach (var upgrade in upgradeDataList)
            levels[upgrade] = 0;
    }

    private void Start()
    {
        // Setup UI seulement — les levels sont déjà corrects (chargés dans GameManager.Awake)
        for (int i = 0; i < upgradeDataList.Count && i < upgradeUIList.Count; i++)
            upgradeUIList[i].Setup(upgradeDataList[i], this);

        RefreshAllUpgrades();
    }

    public int GetLevel(UpgradeData upgrade)
    {
        return levels.TryGetValue(upgrade, out int lvl) ? lvl : 0;
    }

    public void SetLevel(UpgradeData upgrade, int level)
    {
        if (levels.ContainsKey(upgrade))
            levels[upgrade] = Mathf.Max(0, level);
        else
            levels[upgrade] = Mathf.Max(0, level);

        RefreshAllUpgrades();
    }

    public void ApplyLoadedLevels()
    {
        RefreshAllUpgrades();
    }

    public bool CanAfford(UpgradeData upgrade)
    {
        int cost = upgrade.GetCost(GetLevel(upgrade));
        return playerData.mush >= cost;
    }

    public void TryBuyUpgrade(UpgradeData upgrade)
    {
        if (upgrade == null || !CanAfford(upgrade)) return;

        int cost = upgrade.GetCost(GetLevel(upgrade));
        playerData.mush -= cost;
        levels[upgrade]++;

        uiManager.UpdateCurrencyUI();
        RefreshAllUpgrades();

        int lvl = levels[upgrade];

        GameManager.SaveNow();

        Debug.Log($"[{upgrade.upgradeName}] Lv {lvl} — unlocked: {upgrade.IsMaxed(lvl)}");
    }

    /// <summary>Retourne la chance de Spore Explosion (upgrade index 0), ou 0 si non acheté.</summary>
    public float GetSporeExplosionChance()
    {
        if (upgradeDataList.Count == 0) return 0f;
        int lvl = GetLevel(upgradeDataList[0]);
        return lvl > 0 ? upgradeDataList[0].GetChance(lvl) : 0f;
    }

    public int GetSporeCount()
    {
        if (upgradeDataList.Count == 0) return 0;
        int lvl = GetLevel(upgradeDataList[0]);
        return upgradeDataList[0].GetSporeCount(lvl);
    }

    public bool IsAutoAimUnlocked()
    {
        if (upgradeDataList.Count < 2) return false;
        return GetLevel(upgradeDataList[1]) >= 1;
    }

    public void RefreshAllUpgrades()
    {
        foreach (var ui in upgradeUIList)
            ui.Refresh();
    }
}