using UnityEngine;
using System.Collections;

public enum CurrencyType { leaf, mush, coin }

public class AutoCollect : MonoBehaviour
{
    
    public PlayerData playerData;
    private SpriteRenderer spriteRenderer;
    public CurrencyType currencyType;
    private UIManager uiManager;
    private UpgradeManager upgradeManager;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        uiManager = FindAnyObjectByType<UIManager>();
        upgradeManager = FindAnyObjectByType<UpgradeManager>();
        Collect();
    }

    private void Update()
    {
        transform.position += Vector3.up * Time.deltaTime * 1.5f;
    }

    private void Collect()
    {
        switch (currencyType)
        {
            case CurrencyType.leaf:
                playerData.leaf += 1; 
                break;
            case CurrencyType.mush:
                playerData.mush += 1;
                break;
            case CurrencyType.coin:
                playerData.coin += 1; 
                break;
        }

        StartCoroutine(FadeAndDestroy());

        uiManager.UpdateCurrencyUI();
        if (currencyType == CurrencyType.mush)
        {
            upgradeManager.RefreshAllUpgrades();
        }
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
