using UnityEngine;
using System.Collections;

public enum CurrencyType { leaf, coin 
}
public class AutoCollect : MonoBehaviour
{
    
    public PlayerData playerData;
    private SpriteRenderer spriteRenderer;
    public CurrencyType currencyType;
    private UIManager uiManager;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        uiManager = FindFirstObjectByType<UIManager>();
        Collect();
    }

    private void Collect()
    {
        switch (currencyType)
        {
            case CurrencyType.leaf:
                playerData.leaf += 1; 
                break;
            case CurrencyType.coin:
                playerData.coin += 1; 
                break;
        }

        StartCoroutine(FadeAndDestroy());

        uiManager.UpdateCurrencyUI();
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
