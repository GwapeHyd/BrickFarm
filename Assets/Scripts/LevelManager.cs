using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("Level Settings")]
    public GameObject brickPrefab;
    public int brickCount = 10;
    public int gridWidth = 8;
    public int gridHeight = 5;
    public float cellSize = 0.5f;

    public float levelTime = 10f;
    public float brickSpawnDelay = 0.1f;


    private float timer;
    private List<GameObject> bricks = new List<GameObject>();
    private bool levelActive = false;
    private bool bricksSpawning = false;
    private int currentLevel = 1;

    public BallController ballController;
    public System.Action OnLevelWin;

    public PlayerData playerData; // à assigner dans l'inspecteur
    public SkillTreeManager skillTreeManager;
    public LevelTransitionUI levelTransition; // à assigner dans l'inspecteur
    public WinScreenUI winScreen;             // à assigner dans l'inspecteur
    [HideInInspector] public int pendingGold = 0;
    [HideInInspector] public bool pendingLevelAdvance = false;
    private int availableClicks = 0;
    private int baseBrickCount;

    public const string MaxLevelKey = "MaxLevel";


    void Start()
    {
        baseBrickCount = brickCount;
        // Reprend au niveau max atteint
        int savedLevel = PlayerPrefs.GetInt(MaxLevelKey, 1);
        currentLevel = savedLevel;
        brickCount = baseBrickCount + (currentLevel - 1) * 5;
        StartLevel();
    }


    void Update()
    {
        if (!levelActive || bricksSpawning) return;
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = 0f;
            LoseLevel();
        }
    }


    public void StartLevel()
    {
        ClearBricks();
        float timerBonus = skillTreeManager != null ? skillTreeManager.GetTimerBonus() : 0f;
        timer = levelTime + timerBonus;
        levelActive = false;
        pendingGold = 0;
        availableClicks = skillTreeManager != null ? skillTreeManager.GetMaxClicks() : 0;
        if (ballController != null)
        {
            ballController.ResetBall();
        }
        StartCoroutine(SpawnBricksWithDelay());
    }

    Vector3 GetGridOffset()
    {
        float totalWidth = (gridWidth - 1) * cellSize;
        float totalHeight = (gridHeight - 1) * cellSize;
        return new Vector3(-totalWidth / 2f, -totalHeight / 2f, 0f);
    }


    System.Collections.IEnumerator SpawnBricksWithDelay()
    {
        bricksSpawning = true;
        HashSet<Vector2Int> usedPositions = new HashSet<Vector2Int>();
        int placed = 0;
        Vector3 offset = GetGridOffset();
        int maxBricks = Mathf.Min(brickCount, gridWidth * gridHeight);
        if (brickCount > gridWidth * gridHeight)
        {
            Debug.LogWarning($"Trop de briques demandées ({brickCount}) pour la grille ({gridWidth}x{gridHeight}) : limitation à {maxBricks}");
        }
        while (placed < maxBricks)
        {
            int x = Random.Range(0, gridWidth);
            int y = Random.Range(0, gridHeight);
            Vector2Int pos = new Vector2Int(x, y);
            if (usedPositions.Contains(pos)) continue;
            usedPositions.Add(pos);
            Vector3 worldPos = new Vector3(x * cellSize, y * cellSize, 0) + offset;
            GameObject brick = Instantiate(brickPrefab, worldPos, Quaternion.identity);
            brick.GetComponent<Brick>().Init(this);
            bricks.Add(brick);
            placed++;
            yield return new WaitForSeconds(brickSpawnDelay);
        }
        bricksSpawning = false;
        // Autoriser le lancement de la balle
        if (ballController != null)
        {
            ballController.EnableLaunch();
        }
    }
    // Appelée par BallController quand la balle est lancée
    public void StartLevelTimer()
    {
        levelActive = true;
    }


    public void OnBrickDestroyed(GameObject brick)
    {
        bricks.Remove(brick);

        // N'ajoute du gold que si le niveau est actif
        // (évite le gold fantôme des briques détruites en fin/début de niveau via ClearBricks)
        if (!levelActive) return;

        // Gain exponentiel : 1, 2, 4, 8, ...
        int baseGold = 1 << (currentLevel - 1); // 2^(niveau-1)
        int skillBonus = skillTreeManager != null ? skillTreeManager.GetGoldBonus() : 0;
        int totalGold = baseGold + skillBonus;
        // Chance de doubler le gold (skill Double Gold)
        float doubleChance = skillTreeManager != null ? skillTreeManager.GetDoubleGoldChance() : 0f;
        if (doubleChance > 0f && Random.value < doubleChance)
        {
            Debug.Log("Double Gold !");
            totalGold *= 2;
        }
        pendingGold += totalGold;

        if (bricks.Count == 0)
        {
            WinLevel();
        }
    }


    void WinLevel()
    {
        levelActive = false;
        // Gain d'un SkillPoint (avant l'affichage de l'écran)
        if (playerData != null)
            playerData.skillPoint++;

        OnLevelWin?.Invoke();

        if (winScreen != null)
        {
            winScreen.ShowWinScreen(pendingGold, sp: 1);
            pendingGold = 0;
        }
        else
        {
            // Fallback sans win screen
            pendingGold = 0;
            StartCoroutine(NextLevelCoroutine());
        }
    }

    /// <summary>Déclenché par WinScreenUI (bouton Continuer) ou à la fermeture du skill tree.</summary>
    public void ProceedToNextLevel()
    {
        pendingLevelAdvance = false;
        StartCoroutine(NextLevelCoroutine());
    }

    System.Collections.IEnumerator NextLevelCoroutine()
    {
        if (levelTransition != null)
        {
            int spTotal = playerData != null ? playerData.skillPoint : 0;
            yield return StartCoroutine(levelTransition.PlayTransition(() =>
            {
                currentLevel++;
                brickCount += 5;
                if (currentLevel > PlayerPrefs.GetInt(MaxLevelKey, 1))
                {
                    PlayerPrefs.SetInt(MaxLevelKey, currentLevel);
                    PlayerPrefs.Save();
                }
                StartLevel();
            }, spGained: 1, spTotal: spTotal));
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
            currentLevel++;
            brickCount += 5;
            if (currentLevel > PlayerPrefs.GetInt(MaxLevelKey, 1))
            {
                PlayerPrefs.SetInt(MaxLevelKey, currentLevel);
                PlayerPrefs.Save();
            }
            StartLevel();
        }
    }


    void LoseLevel()
    {
        levelActive = false;
        // Arrêter la balle
        if (ballController != null)
        {
            ballController.ResetBall();
        }
        // Afficher l'écran de lose et transmettre le gain
        var loseScreen = FindAnyObjectByType<LoseScreenUI>();
        if (loseScreen != null)
        {
            loseScreen.ShowLoseScreen(pendingGold);
        }
        // Reset le compteur pour le prochain niveau
        pendingGold = 0;
    }


    void ClearBricks()
    {
        foreach (var b in bricks)
        {
            if (b != null) Destroy(b);
        }
        bricks.Clear();
    }

    public float GetTimer() => timer;
    public float GetMaxTimer() => levelTime + (skillTreeManager != null ? skillTreeManager.GetTimerBonus() : 0f);
    public bool IsLevelActive() => levelActive;
    public int GetCurrentLevel() => currentLevel;
    public bool HasClicksLeft() => availableClicks > 0;
    public int GetAvailableClicks() => availableClicks;
    public void UseClick() { if (availableClicks > 0) availableClicks--; }
    public int GetGoldPerBrick()
    {
        int baseGold = 1 << (currentLevel - 1);
        int bonus = skillTreeManager != null ? skillTreeManager.GetGoldBonus() : 0;
        return baseGold + bonus;
    }
}
