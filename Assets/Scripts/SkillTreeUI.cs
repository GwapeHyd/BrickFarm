using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SkillTreeUI : MonoBehaviour
{
    public SkillTreeManager skillTreeManager;
    public Transform skillButtonParent; // Un GameObject vide dans le Canvas
    public GameObject skillButtonPrefab; // Un prefab de bouton avec TMP_Text et Image

    public TextMeshProUGUI goldText; // à lier dans l'inspecteur
    public TextMeshProUGUI skillPointText; // à lier dans l'inspecteur
    public Button resetButton; // à lier dans l'inspecteur
    public Button playButton; // à lier dans l'inspecteur (NOUVEAU)

    private List<SkillButtonUI> skillButtons = new List<SkillButtonUI>();
    public SkillTreeLinksUI linksUI; // à lier dans l'inspecteur

    void OnEnable()
    {
        RefreshUI();
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlayButton);
        }
    }

    public void RefreshUI()
    {
        // Nettoie l'ancien affichage
        foreach (Transform child in skillButtonParent)
            Destroy(child.gameObject);
        skillButtons.Clear();

        var visibleSkills = skillTreeManager.GetVisibleSkills();
        foreach (var skill in visibleSkills)
        {
            var btnObj = Instantiate(skillButtonPrefab, skillButtonParent);
            var btnUI = btnObj.GetComponent<SkillButtonUI>();
            btnUI.Setup(skill, this, skillTreeManager);
            // Positionne le bouton selon skill.position (en pixels)
            var rect = btnObj.GetComponent<RectTransform>();
            rect.anchoredPosition = skill.position;
            skillButtons.Add(btnUI);
        }

        // Affichage gold actuel / total
        if (goldText != null && skillTreeManager.playerData != null)
        {
            int current = skillTreeManager.playerData.coin;
            int total = skillTreeManager.GetTotalGoldSpent() + current;
            goldText.text = $"{current} / {total}";
        }

        if (skillPointText != null && skillTreeManager.playerData != null)
        {
            int current = skillTreeManager.playerData.skillPoint;
            int total = skillTreeManager.GetTotalSkillPointSpent() + current;
            skillPointText.text = $"{current} / {total}";
        }

        // Bouton reset
        if (resetButton != null)
        {
            resetButton.gameObject.SetActive(skillTreeManager.IsResetUnlocked());
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(OnResetButton);
        }

        // Dessin des liens
        if (linksUI != null)
            linksUI.DrawLinks();
    }

    // Bouton Play pour fermer le skill tree et relancer le jeu
    public void OnPlayButton()
    {
        // Ferme le skill tree
        gameObject.SetActive(false);
        // Met à jour l'affichage des monnaies après un éventuel achat de skill
        if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
            GameManager.Instance.uiManager.UpdateCurrencyUI();
        // Relance le niveau si besoin
        var levelManager = FindAnyObjectByType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.StartLevel();
        }
    }

    // Appelé par SkillButtonUI quand on clique
    public void OnSkillClicked(SkillData skill)
    {
        if (skillTreeManager.TryUpgrade(skill))
        {
            RefreshUI();
        }
    }

    public void OnResetButton()
    {
        skillTreeManager.ResetSkillTree();
        RefreshUI();
    }
}
