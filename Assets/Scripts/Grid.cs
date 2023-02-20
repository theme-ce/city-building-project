using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Grid : MonoBehaviour
{
    [SerializeField] private GameObject[] m_treePrefabs;
    [SerializeField] private Material m_terrainMaterial;
    [SerializeField] private Material m_edgeMaterials;
    [SerializeField] private Material m_waterMaterial;

    [SerializeField] private GameObject m_waterGrid;
    [SerializeField] private float m_scale = 5f;
    [SerializeField] private float m_treeNoiseScale = 5f;
    [SerializeField] private float m_treeDensity = .3f;
    [SerializeField] private int m_size = 100;

    private Cell[,] m_grid;
    private List<List<Vector2>> m_squares = new List<List<Vector2>>();

    [SerializeField] private TerrainInfo[] m_terrainInfo;

    private void Start()
    {
        float[,] noiseMap = GenerateNoiseMap(m_size, m_size, m_scale);

        m_grid = new Cell[m_size, m_size];
        for (int y = 0; y < m_size; y++)
        {
            for (int x = 0; x < m_size; x++)
            {
                Cell cell = new Cell(GetTerrainInfo(noiseMap[x, y]));
                m_grid[x, y] = cell;
                if (cell.CellType != CellType.Water)
                {
                    m_squares.Add(new List<Vector2>() { new Vector2(x - .5f, y + .5f), new Vector2(x + .5f, y + .5f), new Vector2(x - .5f, y - .5f), new Vector2(x + .5f, y - .5f) });
                }
            }
        }

        for (int y = 0; y < m_size; y++)
        {
            for (int x = 0; x < m_size; x++)
            {
                bool isLeftWater = (x > 0 && m_grid[x - 1, y].CellType == CellType.Water);
                bool isRightWater = (x < m_size - 1 && m_grid[x + 1, y].CellType == CellType.Water);
                bool isUpWater = (y < m_size - 1 && m_grid[x, y + 1].CellType == CellType.Water);
                bool isDownWater = (y > 0 && m_grid[x, y - 1].CellType == CellType.Water);
                bool isUpLeftWater = (x > 0 && y < m_size - 1 && m_grid[x - 1, y + 1].CellType == CellType.Water);
                bool isUpRightWater = (x < m_size - 1 && y < m_size - 1 && m_grid[x + 1, y + 1].CellType == CellType.Water);
                bool isDownLeftWater = (x > 0 && y > 0 && m_grid[x - 1, y - 1].CellType == CellType.Water);
                bool isDownRightWater = (x < m_size - 1 && y > 0 && m_grid[x + 1, y - 1].CellType == CellType.Water);
                if (m_grid[x, y].CellType != CellType.Water && (isLeftWater || isRightWater || isUpWater || isDownWater || isUpLeftWater || isUpRightWater || isDownLeftWater || isDownRightWater))
                {
                    m_grid[x, y].CellType = CellType.Sand;
                }
            }
        }

        DrawTerrainMesh(m_grid);
        DrawEdgeMesh(m_grid);
        GenerateTrees(m_grid);
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

    void DrawTerrainMesh(Cell[,] grid)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        Mesh waterMesh = new Mesh();
        List<Vector3> waterVertices = new List<Vector3>();
        List<int> waterTriangles = new List<int>();
        List<Vector2> waterUvs = new List<Vector2>();

        for (int y = 0; y < m_size; y++)
        {
            for (int x = 0; x < m_size; x++)
            {
                Cell cell = grid[x, y];
                if (cell.CellType == CellType.Grass || cell.CellType == CellType.Sand)
                {
                    Vector3 a = new Vector3(x - .5f, 0, y + .5f);
                    Vector3 b = new Vector3(x + .5f, 0, y + .5f);
                    Vector3 c = new Vector3(x - .5f, 0, y - .5f);
                    Vector3 d = new Vector3(x + .5f, 0, y - .5f);
                    Vector2 uvA = new Vector2(x / (float)m_size, y / (float)m_size);
                    Vector2 uvB = new Vector2((x + 1) / (float)m_size, y / (float)m_size);
                    Vector2 uvC = new Vector2(x / (float)m_size, (y + 1) / (float)m_size);
                    Vector2 uvD = new Vector2((x + 1) / (float)m_size, (y + 1) / (float)m_size);
                    Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                    Vector2[] uv = new Vector2[] { uvA, uvB, uvC, uvB, uvD, uvC };
                    for (int k = 0; k < 6; k++)
                    {
                        vertices.Add(v[k]);
                        triangles.Add(triangles.Count);
                        uvs.Add(uv[k]);
                    }
                }
                else if (cell.CellType == CellType.Grass2)
                {
                    Vector3 a = new Vector3(x - .5f, 1, y + .5f);
                    Vector3 b = new Vector3(x + .5f, 1, y + .5f);
                    Vector3 c = new Vector3(x - .5f, 1, y - .5f);
                    Vector3 d = new Vector3(x + .5f, 1, y - .5f);
                    Vector2 uvA = new Vector2(x / (float)m_size, y / (float)m_size);
                    Vector2 uvB = new Vector2((x + 1) / (float)m_size, y / (float)m_size);
                    Vector2 uvC = new Vector2(x / (float)m_size, (y + 1) / (float)m_size);
                    Vector2 uvD = new Vector2((x + 1) / (float)m_size, (y + 1) / (float)m_size);
                    Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                    Vector2[] uv = new Vector2[] { uvA, uvB, uvC, uvB, uvD, uvC };
                    for (int k = 0; k < 6; k++)
                    {
                        vertices.Add(v[k]);
                        triangles.Add(triangles.Count);
                        uvs.Add(uv[k]);
                    }
                }
                else if (cell.CellType == CellType.Water)
                {
                    Vector3 a = new Vector3(x - .5f, 0, y + .5f);
                    Vector3 b = new Vector3(x + .5f, 0, y + .5f);
                    Vector3 c = new Vector3(x - .5f, 0, y - .5f);
                    Vector3 d = new Vector3(x + .5f, 0, y - .5f);
                    Vector2 uvA = new Vector2(x / (float)m_size, y / (float)m_size);
                    Vector2 uvB = new Vector2((x + 1) / (float)m_size, y / (float)m_size);
                    Vector2 uvC = new Vector2(x / (float)m_size, (y + 1) / (float)m_size);
                    Vector2 uvD = new Vector2((x + 1) / (float)m_size, (y + 1) / (float)m_size);
                    Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                    Vector2[] uv = new Vector2[] { uvA, uvB, uvC, uvB, uvD, uvC };
                    for (int k = 0; k < 6; k++)
                    {
                        waterVertices.Add(v[k]);
                        waterTriangles.Add(waterTriangles.Count);
                        waterUvs.Add(uv[k]);
                    }
                }
            }
        }
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        MeshFilter meshfilter = gameObject.AddComponent<MeshFilter>();
        meshfilter.mesh = mesh;

        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = m_terrainMaterial;
        meshRenderer.material.mainTexture = DrawTexture(m_grid);

        MeshCollider meshCollider = m_waterGrid.AddComponent<MeshCollider>();

        waterMesh.vertices = waterVertices.ToArray();
        waterMesh.triangles = waterTriangles.ToArray();
        waterMesh.uv = waterUvs.ToArray();
        waterMesh.RecalculateNormals();

        MeshFilter waterMeshFilter = m_waterGrid.AddComponent<MeshFilter>();
        waterMeshFilter.mesh = waterMesh;

        MeshRenderer waterMeshRenderer = m_waterGrid.AddComponent<MeshRenderer>();
        waterMeshRenderer.material = m_waterMaterial;

        MeshCollider waterMeshCollider = m_waterGrid.AddComponent<MeshCollider>();
    }

    Texture2D DrawTexture(Cell[,] grid)
    {
        Color[] colorMap = new Color[m_size * m_size];

        for (int y = 0; y < m_size; y++)
        {
            for (int x = 0; x < m_size; x++)
            {
                int colorIndex = y * m_size + x;
                colorMap[colorIndex] = m_grid[x, y].Color;
            }
        }

        Texture2D tileTexture = new Texture2D(m_size, m_size, TextureFormat.RGBA32, false, false);
        tileTexture.wrapMode = TextureWrapMode.Clamp;
        tileTexture.filterMode = FilterMode.Point;
        tileTexture.SetPixels(colorMap);
        tileTexture.Apply();

        return tileTexture;
    }

    void DrawEdgeMesh(Cell[,] grid)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        for (int y = 0; y < m_size; y++)
        {
            for (int x = 0; x < m_size; x++)
            {
                Cell cell = grid[x, y];
                if (cell.CellType == CellType.Grass || cell.CellType == CellType.Sand)
                {
                    if (x > 0)
                    {
                        Cell left = grid[x - 1, y];
                        if (left.CellType == CellType.Water)
                        {
                            Vector3 a = new Vector3(x - .5f, 0, y + .5f);
                            Vector3 b = new Vector3(x - .5f, 0, y - .5f);
                            Vector3 c = new Vector3(x - .5f, -1, y + .5f);
                            Vector3 d = new Vector3(x - .5f, -1, y - .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for (int k = 0; k < 6; k++)
                            {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                    if (x < m_size - 1)
                    {
                        Cell right = grid[x + 1, y];
                        if (right.CellType == CellType.Water)
                        {
                            Vector3 a = new Vector3(x + .5f, 0, y - .5f);
                            Vector3 b = new Vector3(x + .5f, 0, y + .5f);
                            Vector3 c = new Vector3(x + .5f, -1, y - .5f);
                            Vector3 d = new Vector3(x + .5f, -1, y + .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for (int k = 0; k < 6; k++)
                            {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                    if (y > 0)
                    {
                        Cell down = grid[x, y - 1];
                        if (down.CellType == CellType.Water)
                        {
                            Vector3 a = new Vector3(x - .5f, 0, y - .5f);
                            Vector3 b = new Vector3(x + .5f, 0, y - .5f);
                            Vector3 c = new Vector3(x - .5f, -1, y - .5f);
                            Vector3 d = new Vector3(x + .5f, -1, y - .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for (int k = 0; k < 6; k++)
                            {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                    if (y < m_size - 1)
                    {
                        Cell up = grid[x, y + 1];
                        if (up.CellType == CellType.Water)
                        {
                            Vector3 a = new Vector3(x + .5f, 0, y + .5f);
                            Vector3 b = new Vector3(x - .5f, 0, y + .5f);
                            Vector3 c = new Vector3(x + .5f, -1, y + .5f);
                            Vector3 d = new Vector3(x - .5f, -1, y + .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for (int k = 0; k < 6; k++)
                            {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                    if (x == 0)
                    {
                        Vector3 a = new Vector3(x - .5f, 0, y + .5f);
                        Vector3 b = new Vector3(x - .5f, 0, y - .5f);
                        Vector3 c = new Vector3(x - .5f, -1, y + .5f);
                        Vector3 d = new Vector3(x - .5f, -1, y - .5f);
                        Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                        for (int k = 0; k < 6; k++)
                        {
                            vertices.Add(v[k]);
                            triangles.Add(triangles.Count);
                        }
                    }
                    if (x == m_size - 1)
                    {
                        Vector3 a = new Vector3(x + .5f, 0, y - .5f);
                        Vector3 b = new Vector3(x + .5f, 0, y + .5f);
                        Vector3 c = new Vector3(x + .5f, -1, y - .5f);
                        Vector3 d = new Vector3(x + .5f, -1, y + .5f);
                        Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                        for (int k = 0; k < 6; k++)
                        {
                            vertices.Add(v[k]);
                            triangles.Add(triangles.Count);
                        }
                    }
                    if (y == 0)
                    {
                        Vector3 a = new Vector3(x - .5f, 0, y - .5f);
                        Vector3 b = new Vector3(x + .5f, 0, y - .5f);
                        Vector3 c = new Vector3(x - .5f, -1, y - .5f);
                        Vector3 d = new Vector3(x + .5f, -1, y - .5f);
                        Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                        for (int k = 0; k < 6; k++)
                        {
                            vertices.Add(v[k]);
                            triangles.Add(triangles.Count);
                        }
                    }
                    if (y == m_size - 1)
                    {
                        Vector3 a = new Vector3(x + .5f, 0, y + .5f);
                        Vector3 b = new Vector3(x - .5f, 0, y + .5f);
                        Vector3 c = new Vector3(x + .5f, -1, y + .5f);
                        Vector3 d = new Vector3(x - .5f, -1, y + .5f);
                        Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                        for (int k = 0; k < 6; k++)
                        {
                            vertices.Add(v[k]);
                            triangles.Add(triangles.Count);
                        }
                    }
                }
                else if (cell.CellType == CellType.Grass2)
                {
                    if (x > 0)
                    {
                        Cell left = grid[x - 1, y];
                        if (left.CellType != CellType.Grass2)
                        {
                            Vector3 a = new Vector3(x - .5f, 1, y + .5f);
                            Vector3 b = new Vector3(x - .5f, 1, y - .5f);
                            Vector3 c = new Vector3(x - .5f, 0, y + .5f);
                            Vector3 d = new Vector3(x - .5f, 0, y - .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for (int k = 0; k < 6; k++)
                            {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                    if (x < m_size - 1)
                    {
                        Cell right = grid[x + 1, y];
                        if (right.CellType != CellType.Grass2)
                        {
                            Vector3 a = new Vector3(x + .5f, 1, y - .5f);
                            Vector3 b = new Vector3(x + .5f, 1, y + .5f);
                            Vector3 c = new Vector3(x + .5f, 0, y - .5f);
                            Vector3 d = new Vector3(x + .5f, 0, y + .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for (int k = 0; k < 6; k++)
                            {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                    if (y > 0)
                    {
                        Cell down = grid[x, y - 1];
                        if (down.CellType != CellType.Grass2)
                        {
                            Vector3 a = new Vector3(x - .5f, 1, y - .5f);
                            Vector3 b = new Vector3(x + .5f, 1, y - .5f);
                            Vector3 c = new Vector3(x - .5f, 0, y - .5f);
                            Vector3 d = new Vector3(x + .5f, 0, y - .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for (int k = 0; k < 6; k++)
                            {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                    if (y < m_size - 1)
                    {
                        Cell up = grid[x, y + 1];
                        if (up.CellType != CellType.Grass2)
                        {
                            Vector3 a = new Vector3(x + .5f, 1, y + .5f);
                            Vector3 b = new Vector3(x - .5f, 1, y + .5f);
                            Vector3 c = new Vector3(x + .5f, 0, y + .5f);
                            Vector3 d = new Vector3(x - .5f, 0, y + .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for (int k = 0; k < 6; k++)
                            {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                    if (x == 0)
                    {
                        Vector3 a = new Vector3(x - .5f, 1, y + .5f);
                        Vector3 b = new Vector3(x - .5f, 1, y - .5f);
                        Vector3 c = new Vector3(x - .5f, 0, y + .5f);
                        Vector3 d = new Vector3(x - .5f, 0, y - .5f);
                        Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                        for (int k = 0; k < 6; k++)
                        {
                            vertices.Add(v[k]);
                            triangles.Add(triangles.Count);
                        }
                    }
                    if (x == m_size - 1)
                    {
                        Vector3 a = new Vector3(x + .5f, 1, y - .5f);
                        Vector3 b = new Vector3(x + .5f, 1, y + .5f);
                        Vector3 c = new Vector3(x + .5f, 0, y - .5f);
                        Vector3 d = new Vector3(x + .5f, 0, y + .5f);
                        Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                        for (int k = 0; k < 6; k++)
                        {
                            vertices.Add(v[k]);
                            triangles.Add(triangles.Count);
                        }
                    }
                    if (y == 0)
                    {
                        Vector3 a = new Vector3(x - .5f, 1, y - .5f);
                        Vector3 b = new Vector3(x + .5f, 1, y - .5f);
                        Vector3 c = new Vector3(x - .5f, 0, y - .5f);
                        Vector3 d = new Vector3(x + .5f, 0, y - .5f);
                        Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                        for (int k = 0; k < 6; k++)
                        {
                            vertices.Add(v[k]);
                            triangles.Add(triangles.Count);
                        }
                    }
                    if (y == m_size - 1)
                    {
                        Vector3 a = new Vector3(x + .5f, 1, y + .5f);
                        Vector3 b = new Vector3(x - .5f, 1, y + .5f);
                        Vector3 c = new Vector3(x + .5f, 0, y + .5f);
                        Vector3 d = new Vector3(x - .5f, 0, y + .5f);
                        Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                        for (int k = 0; k < 6; k++)
                        {
                            vertices.Add(v[k]);
                            triangles.Add(triangles.Count);
                        }
                    }
                }
            }
        }
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        GameObject edgeObj = new GameObject("Edge");
        edgeObj.transform.SetParent(transform);

        MeshFilter meshFilter = edgeObj.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = edgeObj.AddComponent<MeshRenderer>();
        meshRenderer.material = m_edgeMaterials;
    }

    void GenerateTrees(Cell[,] grid)
    {
        float[,] noiseMap = new float[m_size, m_size];
        float xOffset = Random.Range(-10000f, 10000f);
        float yOffset = Random.Range(-10000f, 10000f);
        for (int y = 0; y < m_size; y++)
        {
            for (int x = 0; x < m_size; x++)
            {
                float noiseValue = Mathf.PerlinNoise(x / m_treeNoiseScale + xOffset, y / m_treeNoiseScale + yOffset);
                noiseMap[x, y] = noiseValue;
            }
        }

        for (int y = 0; y < m_size; y++)
        {
            for (int x = 0; x < m_size; x++)
            {
                Cell cell = grid[x, y];
                if (cell.CellType == CellType.Grass)
                {
                    float v = Random.Range(0f, m_treeDensity);
                    if (noiseMap[x, y] < v)
                    {
                        GameObject prefab = m_treePrefabs[Random.Range(0, m_treePrefabs.Length)];
                        GameObject tree = Instantiate(prefab, transform);
                        tree.transform.position = new Vector3(x, 0, y);
                        tree.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                        tree.transform.localScale = Vector3.one * Random.Range(.8f, 1.2f);
                    }
                }
            }
        }
    }

    public Vector2 FindSquareForPosition(Vector2 position)
    {
        for (int i = 0; i < m_squares.Count; i++)
        {
            if (IsPositionInSquareArea(position, m_squares[i]))
            {
                float centerX = (m_squares[i][0].x + m_squares[i][1].x + m_squares[i][2].x + m_squares[i][3].x) / 4f;
                float centerY = (m_squares[i][0].y + m_squares[i][1].y + m_squares[i][2].y + m_squares[i][3].y) / 4f;
                Vector2 center = new Vector2(centerX, centerY);
                return center;
            }
        }
        return Vector2.zero;
    }

    private bool IsPositionInSquareArea(Vector2 position, List<Vector2> edgePositions)
    {
        float xMin = Mathf.Min(edgePositions[0].x, edgePositions[1].x, edgePositions[2].x, edgePositions[3].x);
        float xMax = Mathf.Max(edgePositions[0].x, edgePositions[1].x, edgePositions[2].x, edgePositions[3].x);
        float yMin = Mathf.Min(edgePositions[0].y, edgePositions[1].y, edgePositions[2].y, edgePositions[3].y);
        float yMax = Mathf.Max(edgePositions[0].y, edgePositions[1].y, edgePositions[2].y, edgePositions[3].y);

        if (xMin <= position.x && position.x <= xMax && yMin <= position.y && position.y <= yMax)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

[Serializable]
public class TerrainInfo
{
    public CellType CellType;
    public float Height;
    public Color Color;
}