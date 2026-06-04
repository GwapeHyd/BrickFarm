using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LoseScreenUI : MonoBehaviour
{
    public GameObject losePanel;
    public TextMeshProUGUI leafGainText;
    public TextMeshProUGUI leafTotalText;
    public TextMeshProUGUI coinGainText;
    public TextMeshProUGUI coinTotalText;
    public TextMeshProUGUI mushGainText;
    public TextMeshProUGUI mushTotalText;
    public float delayBeforeTotal = 0.3f;
    public float transferDuration = 0.3f;

    public PlayerData playerData;
    private int gainedLeaf, gainedCoin, gainedMush;

    void Awake()
    {
        losePanel.SetActive(false);
    }

    // Appelée par LevelManager, reçoit le gain de gold
    public void ShowLoseScreen(int gold)
    {
        gainedLeaf = 0; // À adapter si tu ajoutes d'autres ressources
        gainedCoin = gold;
        gainedMush = 0;
        losePanel.SetActive(true);
        StartCoroutine(AnimateResourceGain());
    }

    IEnumerator AnimateResourceGain()
    {
        // Affiche les gains
        leafGainText.text = "+" + gainedLeaf;
        coinGainText.text = "+" + gainedCoin;
        mushGainText.text = "+" + gainedMush;
        // Cache les totaux au début
        leafTotalText.text = "";
        coinTotalText.text = "";
        mushTotalText.text = "";
        
        yield return new WaitForSeconds(delayBeforeTotal);
        // Affiche les totaux actuels
        int startLeaf = playerData.leaf;
        int startCoin = playerData.coin;
        int startMush = playerData.mush;
        leafTotalText.text = "→ " + startLeaf;
        coinTotalText.text = "→ " + startCoin;
        mushTotalText.text = "→ " + startMush;
        yield return new WaitForSeconds(0.5f);
        // Animation de transfert
        float t = 0f;
        int displayLeaf = gainedLeaf, displayCoin = gainedCoin, displayMush = gainedMush;
        int totalLeaf = startLeaf, totalCoin = startCoin, totalMush = startMush;
        while (t < transferDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / transferDuration);
            displayLeaf = Mathf.RoundToInt(Mathf.Lerp(gainedLeaf, 0, progress));
            totalLeaf = startLeaf + (gainedLeaf - displayLeaf);
            displayCoin = Mathf.RoundToInt(Mathf.Lerp(gainedCoin, 0, progress));
            totalCoin = startCoin + (gainedCoin - displayCoin);
            displayMush = Mathf.RoundToInt(Mathf.Lerp(gainedMush, 0, progress));
            totalMush = startMush + (gainedMush - displayMush);
            leafGainText.text = "+" + displayLeaf;
            coinGainText.text = "+" + displayCoin;
            mushGainText.text = "+" + displayMush;
            leafTotalText.text = "→ " + totalLeaf;
            coinTotalText.text = "→ " + totalCoin;
            mushTotalText.text = "→ " + totalMush;
            yield return null;
        }
        // Fin d'animation, applique les gains directement au total
        playerData.leaf += gainedLeaf;
        playerData.coin += gainedCoin;
        playerData.mush += gainedMush;
        leafGainText.text = "+0";
        coinGainText.text = "+0";
        mushGainText.text = "+0";
        leafTotalText.text = "→ " + playerData.leaf;
        coinTotalText.text = "→ " + playerData.coin;
        mushTotalText.text = "→ " + playerData.mush;
        // Met à jour l'UI globale
        var uiManager = FindAnyObjectByType<UIManager>();
        if (uiManager != null) uiManager.UpdateCurrencyUI();
    }

    // Appelé par le bouton Relancer
    public void OnRetryButton()
    {
        losePanel.SetActive(false);
        var levelManager = FindAnyObjectByType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.StartLevel();
        }
    }

    // Appelé par le bouton Skill Tree
    public void OnSkillTreeButton()
    {
        losePanel.SetActive(false);
        var uiManager = FindAnyObjectByType<UIManager>();
        if (uiManager != null)
        {
            uiManager.OpenSkillTree();
        }
    }
}
