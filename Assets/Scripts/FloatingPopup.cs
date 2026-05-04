using UnityEngine;
using TMPro;

public class FloatingPopup : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float fadeDuration = 1f;

    private TextMeshProUGUI textMesh;
    private CanvasGroup canvasGroup;
    private Transform uiRoot; // Transform du Canvas enfant (UI)
    private float timer = 0f;

    public void Init(string text, Color color)
    {
        // Récupère le TMP sur le Canvas enfant
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        canvasGroup = GetComponentInChildren<CanvasGroup>();
        uiRoot = textMesh?.transform;

        if (textMesh != null)
        {
            textMesh.text = text;
            textMesh.color = color;
        }

        if (canvasGroup == null && uiRoot != null)
        {
            canvasGroup = uiRoot.gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Update()
    {
        if (uiRoot == null || canvasGroup == null) return;

        timer += Time.deltaTime;

        // Déplacement vers le haut dans le monde
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // Fade out
        canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);

        if (timer >= fadeDuration)
        {
            Destroy(gameObject);
        }
    }
    
    public void SetDamage(int damage)
    {
        if (textMesh != null)
        {
            textMesh.text = damage.ToString();
        }
    }
}
