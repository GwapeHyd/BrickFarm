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

    private void Start()
    {
        foreach (var upgrade in upgradeDataList)
            levels[upgrade] = 0;

        for (int i = 0; i < upgradeDataList.Count && i < upgradeUIList.Count; i++)
            upgradeUIList[i].Setup(upgradeDataList[i], this);
    }

    public int GetLevel(UpgradeData upgrade)
    {
        return levels.TryGetValue(upgrade, out int lvl) ? lvl : 0;
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

        Debug.Log($"Upgrade acheté : {upgrade.upgradeName} → Lv {levels[upgrade]}");
    }

    /// <summary>Retourne la chance de Spore Explosion (upgrade index 0), ou 0 si non acheté.</summary>
    public float GetSporeExplosionChance()
    {
        if (upgradeDataList.Count == 0) return 0f;
        int lvl = GetLevel(upgradeDataList[0]);
        return lvl > 0 ? upgradeDataList[0].GetChance(lvl) : 0f;
    }

    public void RefreshAllUpgrades()
    {
        foreach (var ui in upgradeUIList)
            ui.Refresh();
    }
}