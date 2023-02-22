using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TerrainChunkManager : MonoBehaviour
{
    [SerializeField] private GameObject m_chunk;
    [SerializeField] private int m_chunkSize = 50;
    [SerializeField] private int m_maxChunksX = 5;
    [SerializeField] private int m_maxChunksY = 5;

    private DualGridCell[,] m_grounddualGrid;
    private Cell[,] m_groundChunk;
    private Chunk[,] m_chunks;

    [Space]
    [SerializeField] private int m_resourceScale = 5;
    [SerializeField] private Resource[] m_resources;

    public Cell[,] GroundChunkData => m_groundChunk;

    private void Start()
    {
        GenerateGridData();
        GenerateResource();
        GenerateDualGridData();

        m_chunks = new Chunk[m_maxChunksX, m_maxChunksY];

        for (int x = 0; x < m_maxChunksX; x++)
        {
            for (int y = 0; y < m_maxChunksY; y++)
            {
                GameObject chunk = Instantiate(m_chunk, transform);
                chunk.transform.localPosition = new Vector3(x * m_chunkSize, 0, y * m_chunkSize);
                Chunk grid = chunk.GetComponentInChildren<Chunk>();
                m_chunks[x, y] = grid;

                Cell[,] gridDataPerChunk = new Cell[m_chunkSize, m_chunkSize];
                for (int i = 0; i < m_chunkSize; i++)
                {
                    for (int j = 0; j < m_chunkSize; j++)
                    {
                        gridDataPerChunk[i, j] = m_groundChunk[i + (x * m_chunkSize), j + (y * m_chunkSize)];
                    }
                }

                DualGridCell[,] dualGridDataPerChunk = new DualGridCell[m_chunkSize + 1, m_chunkSize + 1];
                for (int i = 0; i < m_chunkSize + 1; i++)
                {
                    for (int j = 0; j < m_chunkSize + 1; j++)
                    {
                        dualGridDataPerChunk[i, j] = m_grounddualGrid[i + (x * m_chunkSize), j + (y * m_chunkSize)];
                    }
                }

                grid.Init(gridDataPerChunk, dualGridDataPerChunk, m_chunkSize, new Vector2Int(x, y));
            }
        }
    }

    public void UpdateMesh(Vector2Int cellPos, float sizeX, float sizeY, CellType cellType)
    {
        List<Chunk> needUpdateChunk = new List<Chunk>();
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                try
                {
                    Cell cell = m_groundChunk[cellPos.x + x - Mathf.FloorToInt(sizeX / 2), cellPos.y + y - Mathf.FloorToInt(sizeY / 2)];
                    cell.UpdateType(cellType);
                    if (!needUpdateChunk.Contains(m_chunks[cell.BelongToChunk.x, cell.BelongToChunk.y])) needUpdateChunk.Add(m_chunks[cell.BelongToChunk.x, cell.BelongToChunk.y]);
                }
                catch (IndexOutOfRangeException)
                {
                    continue;
                }
            }
        }

        foreach (var chunk in needUpdateChunk)
        {
            Cell[,] gridDataPerChunk = new Cell[m_chunkSize, m_chunkSize];
            for (int i = 0; i < m_chunkSize; i++)
            {
                for (int j = 0; j < m_chunkSize; j++)
                {
                    gridDataPerChunk[i, j] = m_groundChunk[i + (chunk.ChunkPos.x * m_chunkSize), j + (chunk.ChunkPos.y * m_chunkSize)];
                }
            }

            DualGridCell[,] dualGridDataPerChunk = new DualGridCell[m_chunkSize + 1, m_chunkSize + 1];
            for (int i = 0; i < m_chunkSize + 1; i++)
            {
                for (int j = 0; j < m_chunkSize + 1; j++)
                {
                    dualGridDataPerChunk[i, j] = m_grounddualGrid[i + (chunk.ChunkPos.x * m_chunkSize), j + (chunk.ChunkPos.y * m_chunkSize)];
                }
            }

            chunk.UpdateChunk(gridDataPerChunk, dualGridDataPerChunk);
        }
    }

    public void GenerateGridData()
    {
        m_groundChunk = new Cell[m_chunkSize * m_maxChunksX, m_chunkSize * m_maxChunksY];
        for (int y = 0; y < m_chunkSize * m_maxChunksY; y++)
        {
            for (int x = 0; x < m_chunkSize * m_maxChunksX; x++)
            {
                Cell cell = new Cell(new Vector2Int(Mathf.FloorToInt(x / m_chunkSize), Mathf.FloorToInt(y / m_chunkSize)), CellType.Grass);
                m_groundChunk[x, y] = cell;
            }
        }
    }

    public void GenerateDualGridData()
    {
        m_grounddualGrid = new DualGridCell[m_chunkSize * m_maxChunksX + 1, m_chunkSize * m_maxChunksY + 1];
        for (int y = 0; y < m_chunkSize * m_maxChunksY + 1; y++)
        {
            for (int x = 0; x < m_chunkSize * m_maxChunksX + 1; x++)
            {
                m_grounddualGrid[x, y] = new DualGridCell();
                m_grounddualGrid[x, y].Position = new Vector3(x - 0.5f, 0, y - 0.5f);
                if (x > 0 && y < m_chunkSize * m_maxChunksY)
                {
                    m_grounddualGrid[x, y].A = m_groundChunk[x - 1, y].CellType;
                    m_groundChunk[x - 1, y].AOfDualGrids = m_grounddualGrid[x, y];
                }
                if (x < m_chunkSize * m_maxChunksX && y < m_chunkSize * m_maxChunksY)
                {
                    m_grounddualGrid[x, y].B = m_groundChunk[x, y].CellType;
                    m_groundChunk[x, y].BOfDualGrids = m_grounddualGrid[x, y];
                }
                if (x > 0 && y > 0)
                {
                    m_grounddualGrid[x, y].C = m_groundChunk[x - 1, y - 1].CellType;
                    m_groundChunk[x - 1, y - 1].COfDualGrids = m_grounddualGrid[x, y];
                }
                if (x < m_chunkSize * m_maxChunksX && y > 0)
                {
                    m_grounddualGrid[x, y].D = m_groundChunk[x, y - 1].CellType;
                    m_groundChunk[x, y - 1].DOfDualGrids = m_grounddualGrid[x, y];
                }
            }
        }
    }

    public void CreateStructure(StructureData structureData, Vector2Int cellPos, float angle)
    {
        int x = Mathf.FloorToInt(cellPos.x / 50);
        int y = Mathf.FloorToInt(cellPos.y / 50);
        Chunk grid = m_chunks[x, y];
        float houseX = Mathf.FloorToInt(cellPos.x - m_chunkSize * x);
        float houseY = Mathf.FloorToInt(cellPos.y - m_chunkSize * y);
        grid.CreateStructure(structureData, new Vector2(houseX, houseY), angle);
        Debug.Log($"Create House at Position: x: {houseX} y :{houseY}, At Chunk x: {x} y: {y}");
    }

    void GenerateResource()
    {
        float[,] noiseMap = GenerateNoiseMap(m_chunkSize * m_maxChunksX, m_chunkSize * m_maxChunksY, m_resourceScale);

        for (int y = 0; y < m_chunkSize * m_maxChunksY; y++)
        {
            for (int x = 0; x < m_chunkSize * m_maxChunksX; x++)
            {
                Resource resourceData = m_resources[Random.Range(0, m_resources.Length)];
                int sizeX = resourceData.SizeX;
                int sizeY = resourceData.SizeY;

                if (x - sizeX < 0) continue;
                if (x + sizeX > m_chunkSize * m_maxChunksX) continue;
                if (y - sizeY < 0) continue;
                if (y + sizeY > m_chunkSize * m_maxChunksY) continue;

                bool checkNext = true;
                for (int i = 0; i < sizeX; i++)
                {
                    for (int j = 0; j < sizeY; j++)
                    {
                        Cell checkCell = m_groundChunk[x + i - Mathf.FloorToInt(sizeX / 2), y + j - Mathf.FloorToInt(sizeY / 2)];
                        if (checkCell.CellType == CellType.Resource)
                        {
                            checkNext = false;
                            break;
                        }
                    }
                    if (!checkNext) break;
                }
                if (!checkNext) continue;

                float v = Random.Range(0f, resourceData.Density);
                if (noiseMap[x, y] < v)
                {
                    Cell mainResourceCell = m_groundChunk[x, y];
                    mainResourceCell.ResourceData = resourceData;
                    for (int i = 0; i < sizeX; i++)
                    {
                        for (int j = 0; j < sizeY; j++)
                        {
                            Cell childResourceCell = m_groundChunk[x + i - Mathf.FloorToInt(sizeX / 2), y + j - Mathf.FloorToInt(sizeY / 2)];
                            childResourceCell.CellType = CellType.Resource;
                            mainResourceCell.ResourceChildCell.Add(childResourceCell);
                        }
                    }
                }

            }
        }
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
