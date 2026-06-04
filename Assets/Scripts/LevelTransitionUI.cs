using UnityEngine;
using System.Collections;

/// <summary>
/// Gère le fondu enchaîné entre deux niveaux.
/// Attacher à un GameObject avec un CanvasGroup (Image noire plein écran, Raycast Target = false).
/// </summary>
public class LevelTransitionUI : MonoBehaviour
{
    [Tooltip("CanvasGroup de l'overlay noir plein écran")]
    public CanvasGroup overlay;

    [Tooltip("Durée du fondu entrant/sortant (secondes)")]
    public float fadeDuration = 0.25f;

    [Header("Affichage SP")]
    [Tooltip("Texte affichant le gain de SP (ex : \"+1 SP\")")]
    public TMPro.TextMeshProUGUI spGainedText;

    [Tooltip("Texte affichant le total de SP (ex : \"Total : 5 SP\")")]
    public TMPro.TextMeshProUGUI spTotalText;

    [Tooltip("Durée d'affichage des SP avant le chargement du niveau suivant (secondes)")]
    public float spDisplayDuration = 1.2f;

    void Awake()
    {
        if (overlay != null)
            overlay.alpha = 0f;
        HideSP();
    }

    /// <summary>
    /// Lance la transition complète : fondu noir → callback → fondu transparent.
    /// </summary>
    public IEnumerator PlayTransition(System.Action onBlack)
    {
        yield return StartCoroutine(Fade(0f, 1f));
        onBlack?.Invoke();
        yield return new WaitForSeconds(0.05f);
        yield return StartCoroutine(Fade(1f, 0f));
    }

    /// <summary>
    /// Transition avec affichage du gain de SP : fondu noir → affiche SP → callback → fondu transparent.
    /// </summary>
    public IEnumerator PlayTransition(System.Action onBlack, int spGained, int spTotal)
    {
        yield return StartCoroutine(Fade(0f, 1f));
        ShowSP(spGained, spTotal);
        yield return new WaitForSeconds(spDisplayDuration);
        HideSP();
        onBlack?.Invoke();
        yield return new WaitForSeconds(0.05f);
        yield return StartCoroutine(Fade(1f, 0f));
    }

    void ShowSP(int gained, int total)
    {
        if (spGainedText != null)
        {
            spGainedText.text = $"+{gained} SP";
            spGainedText.gameObject.SetActive(true);
        }
        if (spTotalText != null)
        {
            spTotalText.text = $"Total : {total} SP";
            spTotalText.gameObject.SetActive(true);
        }
    }

    void HideSP()
    {
        if (spGainedText != null) spGainedText.gameObject.SetActive(false);
        if (spTotalText != null) spTotalText.gameObject.SetActive(false);
    }

    IEnumerator Fade(float from, float to)
    {
        if (overlay == null) yield break;
        float t = 0f;
        overlay.alpha = from;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            overlay.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / fadeDuration));
            yield return null;
        }
        overlay.alpha = to;
    }
}
