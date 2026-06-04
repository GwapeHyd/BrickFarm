using UnityEngine;
using System.Collections.Generic;

public enum SkillState { Locked, Available, Maxed }

public class SkillTreeManager : MonoBehaviour
{
    // Skills déjà vus (affichés au moins une fois)
    private HashSet<string> skillsEverVisible = new HashSet<string>();
    [Header("Définition de l'arbre")]
    public List<SkillData> rootSkills; // Les racines de l'arbre (ScriptableObjects)
    public List<SkillData> allSkills;  // Tous les skills (pour recherche rapide)

    // État courant du joueur : skillId -> niveau
    private Dictionary<string, int> skillLevels = new Dictionary<string, int>();

    // Pour la gestion des ressources
    public PlayerData playerData; // à assigner dans l'inspecteur
    private int totalGoldSpent = 0;
    private int totalSkillPointSpent = 0;

    // Initialisation (à appeler dans Start)
    void Awake()
    {
        foreach (var skill in allSkills)
        {
            if (!skillLevels.ContainsKey(skill.skillId))
                skillLevels[skill.skillId] = 0;
        }
        LoadSkillTree();
        ComputeTotalGoldSpent();
        ComputeTotalSkillPointSpent();
    }

    // Tente d'upgrader un skill
    public bool TryUpgrade(SkillData skill)
    {
        var state = GetSkillState(skill);
        if (state != SkillState.Available) return false;
        int current = skillLevels[skill.skillId];
        if (current >= skill.maxLevel) return false;
        // Vérification du coût selon le type de currency
        if (playerData == null) return false;
        if (skill.costType == CostType.SkillPoint)
        {
            if (playerData.skillPoint < skill.cost) return false;
            playerData.skillPoint -= skill.cost;
        }
        else
        {
            if (playerData.coin < skill.cost) return false;
            playerData.coin -= skill.cost;
        }
        skillLevels[skill.skillId] = current + 1;
        SaveSkillTree();
        ComputeTotalGoldSpent();
        ComputeTotalSkillPointSpent();
        return true;
    }

    // Calcule le total coin dépensé dans l'arbre
    public void ComputeTotalGoldSpent()
    {
        totalGoldSpent = 0;
        foreach (var skill in allSkills)
        {
            if (skill.costType != CostType.Coin) continue;
            int level = skillLevels.ContainsKey(skill.skillId) ? skillLevels[skill.skillId] : 0;
            totalGoldSpent += level * skill.cost;
        }
    }

    // Calcule le total SkillPoint dépensé dans l'arbre
    public void ComputeTotalSkillPointSpent()
    {
        totalSkillPointSpent = 0;
        foreach (var skill in allSkills)
        {
            if (skill.costType != CostType.SkillPoint) continue;
            int level = skillLevels.ContainsKey(skill.skillId) ? skillLevels[skill.skillId] : 0;
            totalSkillPointSpent += level * skill.cost;
        }
    }

    public int GetTotalGoldSpent() => totalGoldSpent;
    public int GetTotalSkillPointSpent() => totalSkillPointSpent;

    // Reset du skill tree (remet tout à zéro et rembourse le joueur)
    public void ResetSkillTree()
    {
        int refundCoin = 0;
        int refundSP = 0;
        foreach (var skill in allSkills)
        {
            int level = skillLevels.ContainsKey(skill.skillId) ? skillLevels[skill.skillId] : 0;
            if (skill.costType == CostType.SkillPoint)
                refundSP += level * skill.cost;
            else
                refundCoin += level * skill.cost;
            skillLevels[skill.skillId] = 0;
            // On ne touche pas à skillsEverVisible ici : ils restent marqués comme "vus"
        }
        if (playerData != null)
        {
            playerData.coin += refundCoin;
            playerData.skillPoint += refundSP;
        }
        SaveSkillTree();
        ComputeTotalGoldSpent();
        ComputeTotalSkillPointSpent();
    }

    // Donne l'état d'un skill (Locked, Available, Maxed)
    public SkillState GetSkillState(SkillData skill)
    {
        int current = skillLevels.ContainsKey(skill.skillId) ? skillLevels[skill.skillId] : 0;
        if (current >= skill.maxLevel) return SkillState.Maxed;
        if (IsSkillAvailable(skill)) return SkillState.Available;
        return SkillState.Locked;
    }

    // Un skill est disponible si c'est une racine ou si un parent est au moins niveau 1
    public bool IsSkillAvailable(SkillData skill)
    {
        if (rootSkills.Contains(skill)) return true;
        foreach (var parent in allSkills)
        {
            if (parent.children != null && parent.children.Contains(skill))
            {
                int parentLevel = skillLevels.ContainsKey(parent.skillId) ? skillLevels[parent.skillId] : 0;
                if (parentLevel > 0) return true;
            }
        }
        return false;
    }

    // Retourne tous les skills visibles (débloqués ou accessibles)
    public List<SkillData> GetVisibleSkills()
    {
        List<SkillData> visible = new List<SkillData>();
        foreach (var root in rootSkills)
            CollectVisible(root, visible);
        // Ajoute tous les skills déjà vus (même s'ils ne sont plus accessibles)
        foreach (var skill in allSkills)
        {
            if (skillsEverVisible.Contains(skill.skillId) && !visible.Contains(skill))
                visible.Add(skill);
        }
        return visible;
    }

    void CollectVisible(SkillData skill, List<SkillData> list)
    {
        if (!list.Contains(skill) && IsSkillAvailable(skill))
        {
            list.Add(skill);
            // Marque ce skill comme déjà vu
            skillsEverVisible.Add(skill.skillId);
        }
        int current = skillLevels.ContainsKey(skill.skillId) ? skillLevels[skill.skillId] : 0;
        if (current > 0 && skill.children != null)
        {
            foreach (var child in skill.children)
            {
                if (child != null)
                    CollectVisible(child, list);
            }
        }
    }

    // Sauvegarde dans PlayerPrefs (par skillId)
    public void SaveSkillTree()
    {
        foreach (var skill in allSkills)
        {
            PlayerPrefs.SetInt($"Skill_{skill.skillId}_level", skillLevels[skill.skillId]);
        }
        PlayerPrefs.Save();
    }

    // Chargement
    public void LoadSkillTree()
    {
        foreach (var skill in allSkills)
        {
            skillLevels[skill.skillId] = PlayerPrefs.GetInt($"Skill_{skill.skillId}_level", 0);
        }
    }

    // Pour l'UI : obtenir le niveau courant
    public int GetSkillLevel(SkillData skill)
    {
        return skillLevels.ContainsKey(skill.skillId) ? skillLevels[skill.skillId] : 0;
    }

    // Le reset est-il débloqué ? (exemple: skillId == "reset" niveau >= 1)
    public bool IsResetUnlocked()
    {
        return skillLevels.ContainsKey("reset") && skillLevels["reset"] >= 1;
    }

    // Bonus de damage total (damage+ ×1, damage++ ×2, damage+++ ×3)
    public int GetDamageBonus()
    {
        int bonus = 0;
        bonus += skillLevels.ContainsKey("damage+")   ? skillLevels["damage+"]   * 1 : 0;
        bonus += skillLevels.ContainsKey("damage++")  ? skillLevels["damage++"]  * 2 : 0;
        bonus += skillLevels.ContainsKey("damage+++") ? skillLevels["damage+++"] * 3 : 0;
        return bonus;
    }

    // Bonus de gold par drop (gold+ ×1, gold++ ×2, gold+++ ×3)
    public int GetGoldBonus()
    {
        int bonus = 0;
        bonus += skillLevels.ContainsKey("gold+")   ? skillLevels["gold+"]   * 1 : 0;
        bonus += skillLevels.ContainsKey("gold++")  ? skillLevels["gold++"]  * 2 : 0;
        bonus += skillLevels.ContainsKey("gold+++") ? skillLevels["gold+++"] * 3 : 0;
        return bonus;
    }

    // Probabilité de doubler le gold par brique (15% par niveau)
    public float GetDoubleGoldChance()
    {
        int level = skillLevels.ContainsKey("doubleGold") ? skillLevels["doubleGold"] : 0;
        return level * 0.15f;
    }

    // Bonus de temps par niveau via Timer+ (skillId "2") — chaque niveau ajoute 1 seconde
    public float GetTimerBonus()
    {
        int bonus = 0;
        bonus += skillLevels.ContainsKey("timer+") ? skillLevels["timer+"] * 1 : 0;
        bonus += skillLevels.ContainsKey("timer++") ? skillLevels["timer++"] * 2 : 0;
        bonus += skillLevels.ContainsKey("timer+++") ? skillLevels["timer+++"] * 3 : 0;
        return bonus;
    }

    // Le click est-il débloqué ? (skill Click, skillId "3", maxLevel 1)
    public bool IsClickUnlocked()
    {
        return skillLevels.ContainsKey("click") && skillLevels["click"] >= 1;
    }

    // Dommage infligé par un clic (1 base + bonus ClickDamage, skillId "4")
    public int GetClickDamage()
    {
        int bonus = skillLevels.ContainsKey("clickDamage") ? skillLevels["clickDamage"] : 0;
        return 1 + bonus;
    }

    // Nombre max de clics disponibles par niveau (Click donne 1, Click+ ajoute autant que son niveau)
    public int GetMaxClicks()
    {
        int clickBase = skillLevels.ContainsKey("click") ? skillLevels["click"] : 0;
        int clickPlus = skillLevels.ContainsKey("click+") ? skillLevels["click+"] : 0;
        return clickBase + clickPlus;
    }

    // Reset complet du jeu (skills, gold, stats)
    [ContextMenu("Reset All Game Data")]
    public void ResetAllGameData()
    {
        // Reset skill tree
        foreach (var skill in allSkills)
            skillLevels[skill.skillId] = 0;
        SaveSkillTree();
        ComputeTotalGoldSpent();
        ComputeTotalSkillPointSpent();
        // Reset PlayerData
        if (playerData != null)
        {
            playerData.coin = 0;
            playerData.skillPoint = 0;
            playerData.leaf = 0;
            playerData.mush = 0;
            playerData.damage = 1;
            playerData.damageMultiplier = 1f;
            // Reset bricks possédées
            if (playerData.ownedBricks != null)
                foreach (var ownedBrick in playerData.ownedBricks)
                    ownedBrick.quantity = 0;
        }
        // Reset niveau max
        PlayerPrefs.SetInt(LevelManager.MaxLevelKey, 1);
        PlayerPrefs.Save();
    }
}
