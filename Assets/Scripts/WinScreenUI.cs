using UnityEngine;
using TMPro;
using System.Collections;

public class WinScreenUI : MonoBehaviour
{
    public GameObject winPanel;

    [Header("Coins")]
    public TextMeshProUGUI coinGainText;
    public TextMeshProUGUI coinTotalText;

    [Header("Skill Points")]
    public TextMeshProUGUI spGainText;
    public TextMeshProUGUI spTotalText;

    [Header("Paramètres")]
    public float delayBeforeTotal = 0.4f;
    public float transferDuration  = 0.5f;

    public PlayerData playerData;

    private int gainedCoin;
    private int gainedSP;
    private LevelManager levelManager;

    void Awake()
    {
        winPanel.SetActive(false);
    }

    /// <summary>
    /// Appelée par LevelManager à la victoire.
    /// SP est déjà incrémenté dans playerData avant cet appel.
    /// </summary>
    public void ShowWinScreen(int gold, int sp)
    {
        gainedCoin    = gold;
        gainedSP      = sp;
        levelManager  = FindAnyObjectByType<LevelManager>();
        winPanel.SetActive(true);
        StartCoroutine(AnimateResourceGain());
    }

    IEnumerator AnimateResourceGain()
    {
        // Affiche les gains immédiats
        coinGainText.text = "+" + gainedCoin;
        spGainText.text   = "+" + gainedSP + " SP";
        coinTotalText.text = "";
        spTotalText.text   = "";

        yield return new WaitForSeconds(delayBeforeTotal);

        // Totaux avant application des coins (SP déjà appliqué)
        int startCoin = playerData.coin;
        int totalSP   = playerData.skillPoint; // déjà incrémenté dans WinLevel

        coinTotalText.text = "→ " + startCoin;
        spTotalText.text   = "→ " + totalSP + " SP";

        yield return new WaitForSeconds(0.5f);

        // Animation de transfert des coins
        float t = 0f;
        while (t < transferDuration)
        {
            t += Time.deltaTime;
            float progress    = Mathf.Clamp01(t / transferDuration);
            int displayCoin   = Mathf.RoundToInt(Mathf.Lerp(gainedCoin, 0f, progress));
            int currentTotal  = startCoin + (gainedCoin - displayCoin);
            coinGainText.text  = "+" + displayCoin;
            coinTotalText.text = "→ " + currentTotal;
            yield return null;
        }

        // Application définitive
        playerData.coin += gainedCoin;
        coinGainText.text  = "+0";
        coinTotalText.text = "→ " + playerData.coin;

        GameManager.SaveNow();

        var uiManager = FindAnyObjectByType<UIManager>();
        if (uiManager != null) uiManager.UpdateCurrencyUI();
    }

    // ── Boutons ─────────────────────────────────────────────────────────────

    /// <summary>Continuer → transition vers le niveau suivant.</summary>
    public void OnContinueButton()
    {
        winPanel.SetActive(false);
        if (levelManager != null)
            levelManager.ProceedToNextLevel();
    }

    /// <summary>Upgrade → ouvre le skill tree ; la fermeture du skill tree relancera le niveau.</summary>
    public void OnUpgradeButton()
    {
        winPanel.SetActive(false);
        if (levelManager != null)
            levelManager.pendingLevelAdvance = true;
        var uiManager = FindAnyObjectByType<UIManager>();
        if (uiManager != null) uiManager.OpenSkillTree();
    }
}
