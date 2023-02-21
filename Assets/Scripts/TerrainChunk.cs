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
    private Grid[,] m_chunks;
    [SerializeField] private TerrainInfo[] m_terrainInfo;

    public Cell[,] GridData => m_grid;

    private void Start()
    {
        float[,] noiseMap = GenerateNoiseMap(m_chunkSize * m_maxChunksX, m_chunkSize * m_maxChunksY, m_scale);
        GenerateGridData(noiseMap);
        GenerateDualGridData();
        m_chunks = new Grid[m_maxChunksX, m_maxChunksY];

        for (int x = 0; x < m_maxChunksX; x++)
        {
            for (int y = 0; y < m_maxChunksY; y++)
            {
                GameObject chunk = Instantiate(m_chunk, transform);
                chunk.transform.localPosition = new Vector3(x * m_chunkSize, 0, y * m_chunkSize);
                Grid grid = chunk.GetComponentInChildren<Grid>();
                m_chunks[x, y] = grid;

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

    public void UpdateMesh(Vector2Int cellPos, CellType cellType)
    {
        int x = Mathf.FloorToInt(cellPos.x / 50);
        int y = Mathf.FloorToInt(cellPos.y / 50);
        Grid grid = m_chunks[x, y];
        Debug.Log($"Position: {cellPos}, At Chunk x: {x} y: {y}, Change from {m_grid[cellPos.x, cellPos.y].CellType} to {cellType}");
        Cell cell = m_grid[cellPos.x, cellPos.y];
        if (cell == null) return;
        cell.UpdateType(cellType);

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

        grid.UpdateChunk(gridDataPerChunk, dualGridDataPerChunk);
    }

    public void GenerateGridData(float[,] noiseMap)
    {
        m_grid = new Cell[m_chunkSize * m_maxChunksX, m_chunkSize * m_maxChunksY];
        for (int y = 0; y < m_chunkSize * m_maxChunksY; y++)
        {
            for (int x = 0; x < m_chunkSize * m_maxChunksX; x++)
            {
                Cell cell = new Cell(CellType.Grass);
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
                if (x > 0 && y < m_chunkSize * m_maxChunksY)
                {
                    m_dualGrid[x, y].A = x > 0 && y < m_chunkSize * m_maxChunksY ? m_grid[x - 1, y].CellType : CellType.Soil;
                    m_grid[x - 1, y].AOfDualGrids = m_dualGrid[x, y];
                }
                if (x < m_chunkSize * m_maxChunksX && y < m_chunkSize * m_maxChunksY)
                {
                    m_dualGrid[x, y].B = x < m_chunkSize * m_maxChunksX && y < m_chunkSize * m_maxChunksY ? m_grid[x, y].CellType : CellType.Soil;
                    m_grid[x, y].BOfDualGrids = m_dualGrid[x, y];
                }
                if (x > 0 && y > 0)
                {
                    m_dualGrid[x, y].C = x > 0 && y > 0 ? m_grid[x - 1, y - 1].CellType : CellType.Soil;
                    m_grid[x - 1, y - 1].COfDualGrids = m_dualGrid[x, y];
                }
                if (x < m_chunkSize * m_maxChunksX && y > 0)
                {
                    m_dualGrid[x, y].D = x < m_chunkSize * m_maxChunksX && y > 0 ? m_grid[x, y - 1].CellType : CellType.Soil;
                    m_grid[x, y - 1].DOfDualGrids = m_dualGrid[x, y];
                }
            }
        }
    }

    public void CreateTree(Vector2Int cellPos)
    {
        int x = Mathf.FloorToInt(cellPos.x / 50);
        int y = Mathf.FloorToInt(cellPos.y / 50);
        Grid grid = m_chunks[x, y];
        float treeX = Mathf.FloorToInt(cellPos.x - m_chunkSize * x);
        float treeY = Mathf.FloorToInt(cellPos.y - m_chunkSize * y);
        grid.CreateTree(new Vector2(treeX, treeY));
        Debug.Log($"Create Tree at Position: x: {treeX} y :{treeY}, At Chunk x: {x} y: {y}");
    }

    public void CreateHouse(Vector2Int cellPos)
    {
        int x = Mathf.FloorToInt(cellPos.x / 50);
        int y = Mathf.FloorToInt(cellPos.y / 50);
        Grid grid = m_chunks[x, y];
        float houseX = Mathf.FloorToInt(cellPos.x - m_chunkSize * x);
        float houseY = Mathf.FloorToInt(cellPos.y - m_chunkSize * y);
        grid.CreateHouse(new Vector2(houseX, houseY));
        Debug.Log($"Create House at Position: x: {houseX} y :{houseY}, At Chunk x: {x} y: {y}");
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
