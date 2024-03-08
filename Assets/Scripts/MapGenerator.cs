using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.AI;
using UnityEngine;
using Unity.AI.Navigation;
using Photon.Pun;
using System.Text;
using System.Linq;
using System;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private Light directionalLight;

    public List<MapDecor> uniqueDecor;
    public List<MapDecor> decorItems;

    public GameObject wallPrefab;
    public GameObject groundPrefab;
    public GameObject decorObj;
    public int gridSizeX;
    public int gridSizeY;
    public float prefabProbability = 0.15f;

    public bool mapGenerated = false;
    string mapSeed;

    //private List<Cell> cells = new List<Cell>();
    private Dictionary<(int, int), Cell> cells = new Dictionary<(int, int), Cell>();

    private List<GameObject> generatedObjects = new List<GameObject>();

    PhotonView PV;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        if (PhotonNetwork.IsMasterClient) GenerateMap();
        //StartCoroutine(GenerateNewMapPeriodically());
    }

    IEnumerator GenerateNewMapPeriodically()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);
            DestroyMap();
            GenerateMap();
        }
    }

    void GenerateCells()
    {
        float halfSizeX = (gridSizeX - 1) / 2f;
        float halfSizeY = (gridSizeY - 1) / 2f;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 cellPosition = new Vector3(x - halfSizeX, 0f, y - halfSizeY);
                Cell cell = new Cell(x, y, cellPosition);
                cells.Add((x, y), cell);
                if (x == 0 || x == gridSizeX - 1 || y == 0 || y == gridSizeY - 1)
                {
                    cell.OnEdge = true;
                }
            }
        }
    }

    void GenerateGroundAndWalls()
    {
        GameObject ground = Instantiate(groundPrefab, Vector3.zero, Quaternion.identity);
        ground.transform.localScale = new Vector3(gridSizeX, 1f, gridSizeY);
        generatedObjects.Add(ground);

        float halfSizeX = gridSizeX / 2f;
        float halfSizeY = gridSizeY / 2f;
        Vector3[] wallPositions = {
        new Vector3(0f, 0f, halfSizeY + 0.5f),
        new Vector3(0f, 0f, -halfSizeY - 0.5f),
        new Vector3(halfSizeX +0.5f, 0f, 0f),
        new Vector3(-halfSizeX -0.5f, 0f, 0f)
        };
        Vector3[] wallScales = {
        new Vector3(gridSizeX + 2f, 50f, 1f),
        new Vector3(gridSizeX + 2f, 50f, 1f),
        new Vector3(1f, 50f, gridSizeY + 2f),
        new Vector3(1f, 50f, gridSizeY + 2f)
        };

        // Create the walls
        for (int i = 0; i < 4; i++)
        {
            GameObject wall = Instantiate(wallPrefab, wallPositions[i], Quaternion.identity);
            wall.transform.localScale = wallScales[i];
            generatedObjects.Add(wall);
        }
    }


    void GenerateMap()
    {
        cells.Clear();

        GenerateCells();
        GenerateGroundAndWalls();

        StringBuilder seed = new StringBuilder();

        PlaceUniqueDecor(ref seed);
        FillEmptyCellsWithRandomDecor(ref seed);

        BakeNavMesh();
        mapGenerated = true;

        mapSeed = seed.ToString();

        Debug.Log(mapSeed);

        PV.RPC(nameof(ReceiveMapSeed), RpcTarget.Others, mapSeed);
        
    }

    [PunRPC]
    void ReceiveMapSeed(string mapSeed)
    {
        Debug.Log($"Received map seed: {mapSeed}");
        UseSeedToGenerateMap(mapSeed);
    }

    public void BakeNavMesh()
    {
        NavMeshSurface[] surfaces = GetComponents<NavMeshSurface>();
        foreach (NavMeshSurface surface in surfaces)
        {
            surface.BuildNavMesh();
        }
    }

    void DestroyMap()
    {
        foreach (GameObject obj in generatedObjects)
        {
            Destroy(obj);
        }
        generatedObjects.Clear();
        cells.Clear();
    }

    public MapDecor PickRandomDecorItem()
    {
        if (decorItems.Count > 0)
        {
            return decorItems[UnityEngine.Random.Range(0, decorItems.Count)];
        }
        return decorItems[0];
    }

    bool CheckCells(MapDecor decorItem, Cell cell)
    {
        int endX = cell.x + decorItem.sizeX;
        int endY = cell.y + decorItem.sizeY;

        if (endX > gridSizeX || endY > gridSizeY)
        {
            return false;
        }
        for (int x = cell.x; x < endX; x++)
        {
            for (int y = cell.y; y < endY; y++)
            {
                Cell currentCell = GetCellAtPosition(x, y);
                if (currentCell.isBlocked)
                {
                    return false;
                }
            }
        }
        return true;
    }

    void PlaceUniqueDecor(ref StringBuilder seed)
    {
        uniqueDecor.Sort((a, b) => (b.sizeX * b.sizeY).CompareTo(a.sizeX * a.sizeY));

        for (int index = 0; index < uniqueDecor.Count; index++)
        {
            MapDecor decor = uniqueDecor[index];
            bool placed = false;
            int attemptLimit = 999;
            while (!placed && attemptLimit > 0)
            {
                int x = UnityEngine.Random.Range(1, gridSizeX - decor.sizeX - 1);
                int y = UnityEngine.Random.Range(1, gridSizeY - decor.sizeY - 1);

                Cell potentialCell = GetCellAtPosition(x, y);
                if (CheckCells(decor, potentialCell))
                {
                    Vector3 decorPosition = potentialCell.position + new Vector3(0, decor.Prefab.transform.localScale.y / 2, 0);
                    decorObj = Instantiate(decor.Prefab, decorPosition, Quaternion.identity);
                    generatedObjects.Add(decorObj);
                    BlockCells(decor, potentialCell);
                    seed.AppendFormat("U{0:D2}{1:D2}{2:D2}", index, x, y);
                    placed = true;
                }
                attemptLimit--;
            }
        }
    }

    void FillEmptyCellsWithRandomDecor(ref StringBuilder seed)
    {
        foreach (var cell in cells.Values)
        {
            if (!cell.isBlocked && !cell.OnEdge && UnityEngine.Random.value < prefabProbability)
            {
                int decorIndex = UnityEngine.Random.Range(0, decorItems.Count);
                MapDecor decorItem = decorItems[decorIndex];

                Vector3 decorPosition = cell.position + new Vector3(0, 0.5f + decorItem.Prefab.transform.localPosition.y, 0);
                if (UnityEngine.Random.value < prefabProbability)
                {
                    GameObject decor = Instantiate(decorItem.Prefab, decorPosition, Quaternion.identity);
                    generatedObjects.Add(decor);
                    BlockCells(decorItem, cell);
                    seed.AppendFormat("D{0:D2}{1:D2}{2:D2}", decorIndex, cell.x, cell.y);
                }
            }
        }
    }
    void UseSeedToGenerateMap(string mapSeed)
    {
        GenerateCells();
        GenerateGroundAndWalls();

        for (int i = 0; i < mapSeed.Length;)
        {
            char type = mapSeed[i];
            bool isUnique = type == 'U';
            int length = 7;

            if (i + length > mapSeed.Length) break; // Break if remaining string is shorter than expected

            string segment = mapSeed.Substring(i, length);
            i += length; // Move to the next segment

            int decorIndex = int.Parse(segment.Substring(1, 2));
            int cellX = int.Parse(segment.Substring(3, 2));
            int cellY = int.Parse(segment.Substring(5, 2));

            PlaceDecorFromSeed(isUnique, decorIndex, cellX, cellY);
        }

        BakeNavMesh();
        mapGenerated = true;
    }

    void PlaceDecorFromSeed(bool isUnique, int decorIndex, int cellX, int cellY)
    {
        uniqueDecor.Sort((a, b) => (b.sizeX * b.sizeY).CompareTo(a.sizeX * a.sizeY));

        if (cellX < 0 || cellX >= gridSizeX || cellY < 0 || cellY >= gridSizeY)
        {
            Debug.LogError($"Cell coordinates ({cellX}, {cellY}) are out of grid bounds.");
            return;
        }

        Cell cell = GetCellAtPosition(cellX, cellY);
        MapDecor decorItem = isUnique ? uniqueDecor[decorIndex] : decorItems[decorIndex];
        Vector3 decorPosition = cell.position + new Vector3(0, 0.5f + decorItem.Prefab.transform.localPosition.y, 0);
        GameObject decor = Instantiate(decorItem.Prefab, decorPosition, Quaternion.identity);
        generatedObjects.Add(decor);
        BlockCells(decorItem, cell);
    }


    void BlockCells(MapDecor decorItem, Cell cell)
    {
        // Assuming cell is not null here; the check is done before calling this method.
        int maxX = Mathf.Min(cell.x + decorItem.sizeX, gridSizeX);
        int maxY = Mathf.Min(cell.y + decorItem.sizeY, gridSizeY);

        for (int x = cell.x; x < maxX; x++)
        {
            for (int y = cell.y; y < maxY; y++)
            {
                if (!cells.TryGetValue((x, y), out Cell currentCell))
                {
                    Debug.LogError($"Attempted to block a non-existent cell at ({x}, {y}).");
                    continue;
                }
                currentCell.isBlocked = true;
            }
        }
    }


    public Cell GetCellAtPosition(int x, int y)
    {
        if (cells.TryGetValue((x, y), out Cell cell))
        {
            return cell;
        }
        Debug.Log($"Cell {x}:{y} not found");
        return null;
    }

    private void Start()
    {
        directionalLight.enabled = true;
    }

    public List<Vector3> GetEdgeCellPositions()
    {
        List<Vector3> edgeCellPositions = new List<Vector3>();
        foreach (var cell in cells.Values)
        {
            if (cell.OnEdge)
            {

                edgeCellPositions.Add(cell.position);
            }
        }
        return edgeCellPositions;
    }

    public List<Vector3> GetEmptyCellPositions()
    {
        List<Vector3> emptyCellPositions = new List<Vector3>();
        foreach (var cell in cells.Values)
        {
            if (!cell.isBlocked && !cell.OnEdge) emptyCellPositions.Add(cell.position);

        }
        return emptyCellPositions;
    }
}