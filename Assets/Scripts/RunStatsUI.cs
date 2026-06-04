using UnityEngine;
using TMPro;

public class RunStatsUI : MonoBehaviour
{
    [Header("Références")]
    public LevelManager levelManager;
    public SkillTreeManager skillTreeManager;
    public PlayerData playerData;

    [Header("Textes stats")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI goldPerBrickText;
    public TextMeshProUGUI pendingGoldText;

    [Header("Click (masqué si non débloqué)")]
    public GameObject clickStatsGroup;
    public TextMeshProUGUI clickDamageText;
    public TextMeshProUGUI clicksLeftText;

    void Update()
    {
        if (levelManager == null || skillTreeManager == null) return;

        // Niveau courant
        if (levelText != null)
            levelText.text = "Level " + levelManager.GetCurrentLevel().ToString();

        // Dégâts par balle
        if (damageText != null)
        {
            int baseDmg = playerData != null ? playerData.damage * (int)playerData.damageMultiplier : 1;
            int totalDmg = baseDmg + skillTreeManager.GetDamageBonus();
            damageText.text = totalDmg.ToString();
        }

        // Gold par brique
        if (goldPerBrickText != null)
        {
            goldPerBrickText.text = levelManager.GetGoldPerBrick().ToString();
        }

        // Gold de la run en cours
        if (pendingGoldText != null)
        {
            pendingGoldText.text = levelManager.pendingGold.ToString();
        }

        // Clics (groupe visible seulement si débloqué)
        bool clickUnlocked = skillTreeManager.IsClickUnlocked();
        if (clickStatsGroup != null)
            clickStatsGroup.SetActive(clickUnlocked);

        if (clickUnlocked)
        {
            if (clickDamageText != null)
                clickDamageText.text = skillTreeManager.GetClickDamage().ToString();
            if (clicksLeftText != null)
                clicksLeftText.text = levelManager.GetAvailableClicks().ToString();
        }
    }
}
