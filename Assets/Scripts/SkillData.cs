using UnityEngine;
using System.Collections.Generic;

public enum CostType { Coin, SkillPoint }

[CreateAssetMenu(fileName = "SkillData", menuName = "BrickFarm/SkillData", order = 1)]
public class SkillData : ScriptableObject
{
    public string skillId; // Unique ID (ex: "damage", "speed")
    public string skillName;
    [TextArea]
    public string description;
    public int maxLevel = 3;
    public int cost = 0; // coût pour chaque niveau
    public CostType costType = CostType.Coin; // type de currency requise
    public Vector2 position; // Pour affichage dans l'arbre
    public List<SkillData> children; // Références directes pour édition, converties en IDs à l'usage
    public Sprite icon; // icône à afficher sur le bouton
}
