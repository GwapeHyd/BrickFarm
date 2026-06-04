using UnityEngine;
using System.Collections;

public class Currency : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        StartCoroutine(FadeAndDestroy());
    }

    private void Update()
    {
        transform.position += Vector3.up * Time.deltaTime * 1.5f;
    }

    private IEnumerator FadeAndDestroy()
    {
        float fadeDuration = 1f;
        float elapsedTime = 0f;
        Color originalColor = spriteRenderer.color;

        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
