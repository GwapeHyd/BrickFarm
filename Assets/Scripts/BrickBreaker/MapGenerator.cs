using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private int width = 8;
    [SerializeField] private int height = 4;
    [SerializeField] private float cellSize = 0.5f;

    public Vector2 origin = Vector2.zero;

    [SerializeField] private PlayerData playerData;
    [SerializeField] private float spawnDelay = 0.3f;

    public Transform mapParent;


    public List<Vector2Int> occupiedPositions = new List<Vector2Int>();
    public List<BrickObject> placedBricks = new List<BrickObject>();

    public int ActiveBrickCount => placedBricks.Count(b => b != null);

    private void Start()
    {
        StartCoroutine(GenerateMapWithDelay());
    }

    public IEnumerator GenerateMapWithDelay()
    {
        occupiedPositions.Clear();
        placedBricks.RemoveAll(b => b == null);

        List<BrickData> bricksToSpawn = new List<BrickData>();
        foreach (var entry in playerData.ownedBricks)
        {
            for (int i = 0; i < entry.quantity; i++)
            {
                bricksToSpawn.Add(entry.data);
            }
        }

        while (bricksToSpawn.Count > 0)
        {
            int index = Random.Range(0, bricksToSpawn.Count);
            BrickData brickToSpawn = bricksToSpawn[index];
            bricksToSpawn.RemoveAt(index);

            Vector2Int? freeCell = GetRandomFreeCell();
            if (freeCell.HasValue)
            {
                SpawnBrick(brickToSpawn, freeCell.Value);
                occupiedPositions.Add(freeCell.Value);
                yield return new WaitForSeconds(spawnDelay);
            }
        }
        yield return null;
    }

    private Vector2Int? GetRandomFreeCell()
    {
        List<Vector2Int> allCells = GetAllGridCells(); // Méthode à implémenter si pas déjà faite
        List<Vector2Int> freeCells = allCells.Where(c => !occupiedPositions.Contains(c)).ToList();

        if (freeCells.Count == 0)
            return null;

        return freeCells[Random.Range(0, freeCells.Count)];
    }

    private List<Vector2Int> GetAllGridCells()
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cells.Add(new Vector2Int(x, y));
            }
        }
        return cells;
    }

    private void SpawnBrick(BrickData brickData, Vector2Int position)
    {
        Vector3 worldPos = origin + new Vector2(position.x * cellSize, position.y * cellSize);
        GameObject go = Instantiate(brickData.prefab, worldPos, Quaternion.identity, mapParent);
        BrickObject brickObj = go.GetComponent<BrickObject>();
        if (brickObj != null)
        {
            brickObj.gridX = position.x;
            brickObj.gridY = position.y;
            go.name = $"{brickData.name} ({position.x},{position.y})";
            
            brickObj.data = brickData;
            brickObj.brickType = brickData.brickType;
            brickObj.playerData = playerData;
            placedBricks.Add(brickObj);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        // Dessine les lignes verticales
        for (int x = 0; x <= width; x++)
        {
            Vector3 start = new Vector3(origin.x + x * cellSize, origin.y, 0);
            Vector3 end = new Vector3(origin.x + x * cellSize, origin.y + height * cellSize, 0);
            Gizmos.DrawLine(start, end);
        }

        // Dessine les lignes horizontales
        for (int y = 0; y <= height; y++)
        {
            Vector3 start = new Vector3(origin.x, origin.y + y * cellSize, 0);
            Vector3 end = new Vector3(origin.x + width * cellSize, origin.y + y * cellSize, 0);
            Gizmos.DrawLine(start, end);
        }
    }
}



[System.Serializable]
    public class BrickEntry
    {
        public string id; // Unique identifier (ex: "dirt", "chest", "mushroom")
        public GameObject prefab;
        public int quantity;
        public string name;
    }