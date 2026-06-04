using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Panneau de tooltip affiché au survol d'un SkillButton.
/// Placer ce composant sur le panel dédié dans le Canvas (désactivé par défaut).
/// Le panel se repositionne automatiquement près du curseur.
/// </summary>
public class SkillTooltipUI : MonoBehaviour
{
    public static SkillTooltipUI Instance { get; private set; }

    [Header("Références")]
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI levelText;

    [Header("Décalage par rapport au curseur")]
    public Vector2 offset = new Vector2(15f, -15f);

    private RectTransform rectTransform;
    private Canvas rootCanvas;

    void Awake()
    {
        Instance = this;
        rectTransform = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Affiche le tooltip pour le skill donné.
    /// </summary>
    public void Show(SkillData skill, SkillState state, bool canAfford)
    {
        if (skillNameText != null) skillNameText.text = skill.skillName;
        if (descriptionText != null) descriptionText.text = skill.description;

        if (levelText != null)
        {
            int current = state == SkillState.Maxed ? skill.maxLevel : 0; // approximation visuelle
            levelText.text = $"Niveau : {state}";
        }

        if (costText != null)
        {
            string currencyLabel = skill.costType == CostType.SkillPoint ? "SP" : "Coin";
            if (state == SkillState.Maxed)
                costText.text = "Max";
            else if (state == SkillState.Locked)
                costText.text = $"Coût : {skill.cost} {currencyLabel}  (verrouillé)";
            else
                costText.text = canAfford
                    ? $"Coût : {skill.cost} {currencyLabel}"
                    : $"Coût : {skill.cost} {currencyLabel}  (fonds insuffisants)";
        }

        gameObject.SetActive(true);
        FollowCursor();
    }

    /// <summary>
    /// Cache le tooltip.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (gameObject.activeSelf)
            FollowCursor();
    }

    void FollowCursor()
    {
        if (rectTransform == null || rootCanvas == null) return;

        Vector2 pos;
        if (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            pos = (Vector2)Input.mousePosition + offset;
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvas.transform as RectTransform,
                Input.mousePosition,
                rootCanvas.worldCamera,
                out pos);
            pos += offset;
        }

        // Empêche le panel de sortir de l'écran
        Vector2 canvasSize = (rootCanvas.transform as RectTransform).sizeDelta;
        float halfW = rectTransform.rect.width * 0.5f;
        float halfH = rectTransform.rect.height * 0.5f;

        pos.x = Mathf.Clamp(pos.x, -canvasSize.x * 0.5f + halfW, canvasSize.x * 0.5f - halfW);
        pos.y = Mathf.Clamp(pos.y, -canvasSize.y * 0.5f + halfH, canvasSize.y * 0.5f - halfH);

        rectTransform.anchoredPosition = pos;
    }
}
