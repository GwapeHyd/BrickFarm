using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public PlayerData playerData;
    public SaveManager saveManager;
    public UIManager uiManager;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        Application.runInBackground = true;
    }

    private void Start()
    {
        // Charge la sauvegarde avant que MapGenerator ne spawne les bricks
        saveManager.Load();

        // Rafraîchit l'UI avec les valeurs chargées
        if (uiManager != null)
        {
            uiManager.UpdateCurrencyUI();
            uiManager.UpdateBrickUI();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) saveManager.Save();
    }

    private void OnApplicationQuit()
    {
        saveManager.Save();
    }

    public static void SaveNow()
    {
        if (Instance != null)
            Instance.saveManager.Save();
    }
}
