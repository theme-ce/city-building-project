using UnityEngine;

public class TerrainChunk : MonoBehaviour
{
    [SerializeField] private int m_chunkSize = 50;
    [SerializeField] private int m_maxChunksX = 5;
    [SerializeField] private int m_maxChunksY = 5;
    [SerializeField] private int m_scale = 5;
    [SerializeField] private GameObject m_chunk;

    private DualGridCell[,] m_dualGrid;
    private Cell[,] m_grid;
    [SerializeField] private TerrainInfo[] m_terrainInfo;

    private void Start()
    {
        float[,] noiseMap = GenerateNoiseMap(m_chunkSize * m_maxChunksX, m_chunkSize * m_maxChunksY, m_scale);
        GenerateGridData(noiseMap);
        GenerateDualGridData();

        for (int x = 0; x < m_maxChunksX; x++)
        {
            for (int y = 0; y < m_maxChunksY; y++)
            {
                GameObject chunk = Instantiate(m_chunk, transform);
                chunk.transform.localPosition = new Vector3(x * m_chunkSize, 0, y * m_chunkSize);
                Grid grid = chunk.GetComponentInChildren<Grid>();

                Cell[,] gridDataPerChunk = new Cell[m_chunkSize, m_chunkSize];
                for (int i = 0; i < m_chunkSize; i++)
                {
                    for (int j = 0; j < m_chunkSize; j++)
                    {
                        gridDataPerChunk[i, j] = m_grid[i + (x * m_chunkSize), j + (y * m_chunkSize)];
                    }
                }

                DualGridCell[,] dualGridDataPerChunk = new DualGridCell[m_chunkSize + 1, m_chunkSize + 1];
                for (int i = 0; i < m_chunkSize + 1; i++)
                {
                    for (int j = 0; j < m_chunkSize + 1; j++)
                    {
                        dualGridDataPerChunk[i, j] = m_dualGrid[i + (x * m_chunkSize), j + (y * m_chunkSize)];
                    }
                }

                grid.Init(gridDataPerChunk, dualGridDataPerChunk, m_chunkSize);
            }
        }
    }

    public void GenerateGridData(float[,] noiseMap)
    {
        m_grid = new Cell[m_chunkSize * m_maxChunksX, m_chunkSize * m_maxChunksY];
        for (int y = 0; y < m_chunkSize * m_maxChunksY; y++)
        {
            for (int x = 0; x < m_chunkSize * m_maxChunksX; x++)
            {
                Cell cell = new Cell(GetTerrainInfo(noiseMap[x, y]));
                m_grid[x, y] = cell;
            }
        }
    }

    public void GenerateDualGridData()
    {
        m_dualGrid = new DualGridCell[m_chunkSize * m_maxChunksX + 1, m_chunkSize * m_maxChunksY + 1];
        for (int y = 0; y < m_chunkSize * m_maxChunksY + 1; y++)
        {
            for (int x = 0; x < m_chunkSize * m_maxChunksX + 1; x++)
            {
                m_dualGrid[x, y] = new DualGridCell();
                m_dualGrid[x, y].Position = new Vector3(x - 0.5f, 0, y - 0.5f);
                m_dualGrid[x, y].A = x > 0 && y < m_chunkSize * m_maxChunksY ? m_grid[x - 1, y].CellType : CellType.Water;
                m_dualGrid[x, y].B = x < m_chunkSize * m_maxChunksX && y < m_chunkSize * m_maxChunksY ? m_grid[x, y].CellType : CellType.Water;
                m_dualGrid[x, y].C = x > 0 && y > 0 ? m_grid[x - 1, y - 1].CellType : CellType.Water;
                m_dualGrid[x, y].D = x < m_chunkSize * m_maxChunksX && y > 0 ? m_grid[x, y - 1].CellType : CellType.Water;
            }
        }
    }

    public TerrainInfo GetTerrainInfo(float height)
    {
        for (int i = 0; i < m_terrainInfo.Length; i++)
        {
            if (height < m_terrainInfo[i].Height)
            {
                return m_terrainInfo[i];
            }
        }

        return m_terrainInfo[0];
    }

    public float[,] GenerateNoiseMap(int mapDepth, int mapWidth, float scale)
    {
        // create an empty noise map with the mapDepth and mapWidth coordinates
        float[,] noiseMap = new float[mapDepth, mapWidth];
        for (int zIndex = 0; zIndex < mapDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < mapWidth; xIndex++)
            {
                // calculate sample indices based on the coordinates and the scale
                float sampleX = xIndex / scale;
                float sampleZ = zIndex / scale;
                // generate noise value using PerlinNoise
                float noise = Mathf.PerlinNoise(sampleX, sampleZ);
                noiseMap[zIndex, xIndex] = noise;
            }
        }
        return noiseMap;
    }
}
