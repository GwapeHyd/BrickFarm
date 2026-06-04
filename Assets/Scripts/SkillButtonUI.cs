using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public Image bgImage;
    public Image iconImage; // à lier dans l'inspecteur
    [HideInInspector] public SkillData skill;
    private SkillTreeUI treeUI;
    private SkillTreeManager skillTreeManager;
    private SkillState currentState;
    private bool currentCanAfford;

    public Color lockedColor = Color.red;
    public Color affordableColor = Color.green;
    public Color unaffordableColor = Color.white;
    public Color maxedColor = Color.yellow;

    public void Setup(SkillData skill, SkillTreeUI treeUI, SkillTreeManager skillTreeManager)
    {
        this.skill = skill;
        this.treeUI = treeUI;
        this.skillTreeManager = skillTreeManager;
        nameText.text = skill.skillName;
        int currentLevel = skillTreeManager.GetSkillLevel(skill);
        levelText.text = $"{currentLevel}/{skill.maxLevel}";
        currentState = skillTreeManager.GetSkillState(skill);
        currentCanAfford = skillTreeManager.playerData != null && (
            skill.costType == CostType.SkillPoint
                ? skillTreeManager.playerData.skillPoint >= skill.cost
                : skillTreeManager.playerData.coin >= skill.cost);
        switch (currentState)
        {
            case SkillState.Locked:
                bgImage.color = lockedColor;
                break;
            case SkillState.Available:
                bgImage.color = currentCanAfford ? affordableColor : unaffordableColor;
                break;
            case SkillState.Maxed:
                bgImage.color = maxedColor;
                break;
        }
        if (iconImage != null)
            iconImage.sprite = skill.icon;
        GetComponent<Button>().interactable = currentState == SkillState.Available;
        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(() => treeUI.OnSkillClicked(skill));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (SkillTooltipUI.Instance != null)
            SkillTooltipUI.Instance.Show(skill, currentState, currentCanAfford);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (SkillTooltipUI.Instance != null)
            SkillTooltipUI.Instance.Hide();
    }

    void OnDisable()
    {
        if (SkillTooltipUI.Instance != null)
            SkillTooltipUI.Instance.Hide();
    }
}

