using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class TerrainChunkManager : MonoBehaviour
{
    public static TerrainChunkManager Instance { get; private set; }

    [SerializeField] private Texture2D m_heightMap;
    [SerializeField] private GameObject m_chunk;
    [SerializeField] private int m_chunkSize = 50;
    [SerializeField] private int m_maxChunksX = 5;
    [SerializeField] private int m_maxChunksY = 5;
    [SerializeField] private float m_scale = 5f;

    private DualGridCell[,] m_dualGridDataData;
    private Cell[,] m_groundData;
    private GrassChunk[,] m_chunks;

    public DualGridCell[,] DualGridDataData => m_dualGridDataData;
    public Cell[,] GroundData => m_groundData;

    [Space]
    [Header("Draw Chunk")]
    [SerializeField] private MoveCamera m_camera;
    private Vector2Int m_currentChunk = new Vector2Int(0, 0);
    private Dictionary<Vector2Int, GameObject> m_activeChunks = new Dictionary<Vector2Int, GameObject>();
    private List<Vector2Int> m_keepChunks = new List<Vector2Int>();
    private List<Vector2Int> m_needGenerateChunks = new List<Vector2Int>();
    private List<Vector2Int> m_toDestroyChunks = new List<Vector2Int>();

    private int m_terrainOffsetX;
    private int m_terrainOffsetY;

    public Cell[,] GroundChunkData => m_groundData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        GenerateLoadedData();
    }

    private void GenerateLoadedData()
    {
        LoadData();

        GenerateGridData(true);
        GenerateDualGridData();

        m_chunks = new GrassChunk[m_maxChunksX, m_maxChunksY];
    }

    private void GenerateNewData()
    {
        GenerateGridData();
        GenerateDualGridData();

        m_chunks = new GrassChunk[m_maxChunksX, m_maxChunksY];

        SaveData();
    }

    private void Update()
    {
        Vector2Int currentChunk = new Vector2Int(Mathf.FloorToInt(m_camera.transform.position.x / m_chunkSize), Mathf.FloorToInt(m_camera.transform.position.z / m_chunkSize));
        if (m_currentChunk != currentChunk)
        {
            m_currentChunk = currentChunk;

            m_keepChunks.Clear();
            m_needGenerateChunks.Clear();
            m_toDestroyChunks.Clear();

            Vector2Int center = new Vector2Int(m_currentChunk.x, m_currentChunk.y);
            Vector2Int left = new Vector2Int(m_currentChunk.x - 1, m_currentChunk.y);
            Vector2Int right = new Vector2Int(m_currentChunk.x + 1, m_currentChunk.y);
            Vector2Int up = new Vector2Int(m_currentChunk.x, m_currentChunk.y + 1);
            Vector2Int down = new Vector2Int(m_currentChunk.x, m_currentChunk.y - 1);
            Vector2Int upLeft = new Vector2Int(m_currentChunk.x - 1, m_currentChunk.y + 1);
            Vector2Int upRight = new Vector2Int(m_currentChunk.x + 1, m_currentChunk.y + 1);
            Vector2Int downLeft = new Vector2Int(m_currentChunk.x - 1, m_currentChunk.y - 1);
            Vector2Int downRight = new Vector2Int(m_currentChunk.x + 1, m_currentChunk.y - 1);

            CheckForKeepChunk(center);
            CheckForKeepChunk(left);
            CheckForKeepChunk(right);
            CheckForKeepChunk(up);
            CheckForKeepChunk(down);
            CheckForKeepChunk(upLeft);
            CheckForKeepChunk(downLeft);
            CheckForKeepChunk(upRight);
            CheckForKeepChunk(downRight);

            foreach (var chunk in m_activeChunks)
            {
                if (!m_keepChunks.Contains(chunk.Key)) m_toDestroyChunks.Add(chunk.Key);
            }

            foreach (var coord in m_toDestroyChunks)
            {
                Destroy(m_activeChunks[new Vector2Int(coord.x, coord.y)]);
                m_activeChunks.Remove(new Vector2Int(coord.x, coord.y));
            }

            foreach (var coord in m_needGenerateChunks)
            {
                StartCoroutine(DrawChunk(coord.x, coord.y));
            }
        }
    }

    void CheckForKeepChunk(Vector2Int direction)
    {
        if (m_activeChunks.ContainsKey(direction)) m_keepChunks.Add(direction);
        else m_needGenerateChunks.Add(direction);
    }

    IEnumerator DrawChunk(int x, int y)
    {
        GameObject chunk = Instantiate(m_chunk, transform);
        chunk.transform.localPosition = new Vector3(x * m_chunkSize, 0, y * m_chunkSize);
        GrassChunk grassChunk = chunk.GetComponentInChildren<GrassChunk>();
        SoilChunk corruptedChunk = chunk.GetComponentInChildren<SoilChunk>();

        try
        {
            m_chunks[x, y] = grassChunk;
        }
        catch
        {
            yield break;
        }

        Cell[,] gridDataPerChunk = new Cell[m_chunkSize, m_chunkSize];
        for (int i = 0; i < m_chunkSize; i++)
        {
            for (int j = 0; j < m_chunkSize; j++)
            {
                gridDataPerChunk[i, j] = m_groundData[i + (x * m_chunkSize), j + (y * m_chunkSize)];
            }
        }

        DualGridCell[,] dualGridDataPerChunk = new DualGridCell[m_chunkSize + 1, m_chunkSize + 1];
        for (int i = 0; i < m_chunkSize + 1; i++)
        {
            for (int j = 0; j < m_chunkSize + 1; j++)
            {
                dualGridDataPerChunk[i, j] = m_dualGridDataData[i + (x * m_chunkSize), j + (y * m_chunkSize)];
            }
        }

        grassChunk.Init(gridDataPerChunk, dualGridDataPerChunk, m_chunkSize, new Vector2Int(x, y));
        corruptedChunk.Init(gridDataPerChunk, dualGridDataPerChunk, m_chunkSize, new Vector2Int(x, y));

        m_activeChunks.Add(new Vector2Int(x, y), chunk);
        yield return null;
    }

    public void GenerateGridData(bool fromLoad = false)
    {
        m_groundData = new Cell[m_chunkSize * m_maxChunksX, m_chunkSize * m_maxChunksY];
        if (!fromLoad)
        {
            m_terrainOffsetX = Random.Range(-10000, 10000);
            m_terrainOffsetY = Random.Range(-10000, 10000);
        }
        float[,] noiseMap = GenerateNoiseMap(m_chunkSize * m_maxChunksX, m_chunkSize * m_maxChunksY, m_scale, m_terrainOffsetX, m_terrainOffsetY);

        for (int y = 0; y < m_chunkSize * m_maxChunksY; y++)
        {
            for (int x = 0; x < m_chunkSize * m_maxChunksX; x++)
            {
                Cell cell = new Cell();
                if (noiseMap[x, y] < 0.15)
                {
                    cell.CellType = CellType.Water;
                }
                else if (noiseMap[x, y] < 0.5)
                {
                    cell.CellType = CellType.Soil;
                }
                else if (noiseMap[x, y] <= 1)
                {
                    cell.CellType = CellType.Grass;
                }
                m_groundData[x, y] = cell;
            }
        }
    }

    public void GenerateDualGridData()
    {
        m_dualGridDataData = new DualGridCell[m_chunkSize * m_maxChunksX + 1, m_chunkSize * m_maxChunksY + 1];
        for (int y = 0; y < m_chunkSize * m_maxChunksY + 1; y++)
        {
            for (int x = 0; x < m_chunkSize * m_maxChunksX + 1; x++)
            {
                m_dualGridDataData[x, y] = new DualGridCell();
                m_dualGridDataData[x, y].Position = new SerializableVector2(new Vector2(x - 0.5f, y - 0.5f));
                if (x > 0 && y < m_chunkSize * m_maxChunksY)
                {
                    m_dualGridDataData[x, y].A = m_groundData[x - 1, y].CellType;
                }
                if (x < m_chunkSize * m_maxChunksX && y < m_chunkSize * m_maxChunksY)
                {
                    m_dualGridDataData[x, y].B = m_groundData[x, y].CellType;
                }
                if (x > 0 && y > 0)
                {
                    m_dualGridDataData[x, y].C = m_groundData[x - 1, y - 1].CellType;
                }
                if (x < m_chunkSize * m_maxChunksX && y > 0)
                {
                    m_dualGridDataData[x, y].D = m_groundData[x, y - 1].CellType;
                }
            }
        }
    }

    public float[,] GenerateNoiseMap(int mapDepth, int mapWidth, float scale, int offsetX, int offsetY)
    {
        // create an empty noise map with the mapDepth and mapWidth coordinates
        float[,] noiseMap = new float[mapDepth, mapWidth];
        for (int zIndex = 0; zIndex < mapDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < mapWidth; xIndex++)
            {
                // calculate sample indices based on the coordinates and the scale
                float sampleX = xIndex / scale + offsetX;
                float sampleZ = zIndex / scale + offsetY;
                // generate noise value using PerlinNoise
                float noise = Mathf.PerlinNoise(sampleX, sampleZ);
                noiseMap[zIndex, xIndex] = noise;
            }
        }
        return noiseMap;
    }

    public void SaveData()
    {
        TerrainData terrainData = new TerrainData(m_terrainOffsetX, m_terrainOffsetY);

        using (FileStream fileStream = new FileStream(Application.persistentDataPath + "/terrainGenerate.dat", FileMode.Create))
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fileStream, terrainData);
        }
    }

    public void LoadData()
    {
        TerrainData terrainData = new TerrainData();

        using (FileStream fileStream = new FileStream(Application.persistentDataPath + "/terrainGenerate.dat", FileMode.Open))
        {
            BinaryFormatter bf = new BinaryFormatter();
            terrainData = (TerrainData)bf.Deserialize(fileStream);
        }

        m_terrainOffsetX = terrainData.TerrainOffsetX;
        m_terrainOffsetY = terrainData.TerrainOffsetY;
    }
}
