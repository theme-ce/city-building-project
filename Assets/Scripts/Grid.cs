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
    [SerializeField] private int m_size = 50;

    private Cell[,] m_grid;
    private DualGridCell[,] m_dualGrid;
    private List<Vector3> m_vertices = new List<Vector3>();
    private List<int> m_triangles = new List<int>();
    private int m_numPoints = 8;
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
            }
        }

        m_dualGrid = new DualGridCell[m_size + 1, m_size + 1];
        for (int y = 0; y < m_size + 1; y++)
        {
            for (int x = 0; x < m_size + 1; x++)
            {
                m_dualGrid[x, y] = new DualGridCell();
                m_dualGrid[x, y].Position = new Vector3(x - 0.5f, 0, y - 0.5f);
                m_dualGrid[x, y].A = x > 0 && y < m_size ? m_grid[x - 1, y].CellType : CellType.Water;
                m_dualGrid[x, y].B = x < m_size && y < m_size ? m_grid[x, y].CellType : CellType.Water;
                m_dualGrid[x, y].C = x > 0 && y > 0 ? m_grid[x - 1, y - 1].CellType : CellType.Water;
                m_dualGrid[x, y].D = x < m_size && y > 0 ? m_grid[x, y - 1].CellType : CellType.Water;
            }
        }

        DrawTerrainMesh(m_grid);
        //DrawEdgeMesh(m_grid);
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

        for (int y = 0; y < m_size + 1; y++)
        {
            for (int x = 0; x < m_size + 1; x++)
            {
                Vector3 pos = m_dualGrid[x, y].Position;
                CellType A = m_dualGrid[x, y].A;
                CellType B = m_dualGrid[x, y].B;
                CellType C = m_dualGrid[x, y].C;
                CellType D = m_dualGrid[x, y].D;
                #region FillCorner
                if (A == CellType.Grass && B == CellType.Water && C == CellType.Water && D == CellType.Water) GenerateMeshTypeFillCorner(pos, 0);
                else if (A == CellType.Water && B == CellType.Grass && C == CellType.Water && D == CellType.Water) GenerateMeshTypeFillCorner(pos, 90);
                else if (A == CellType.Water && B == CellType.Water && C == CellType.Water && D == CellType.Grass) GenerateMeshTypeFillCorner(pos, 180);
                else if (A == CellType.Water && B == CellType.Water && C == CellType.Grass && D == CellType.Water) GenerateMeshTypeFillCorner(pos, 270);
                #endregion
                #region Half
                else if (A == CellType.Grass && B == CellType.Water && C == CellType.Grass && D == CellType.Water) GenerateMeshTypeHalf(pos, 0);
                else if (A == CellType.Grass && B == CellType.Grass && C == CellType.Water && D == CellType.Water) GenerateMeshTypeHalf(pos, 90);
                else if (A == CellType.Water && B == CellType.Grass && C == CellType.Water && D == CellType.Grass) GenerateMeshTypeHalf(pos, 180);
                else if (A == CellType.Water && B == CellType.Water && C == CellType.Grass && D == CellType.Grass) GenerateMeshTypeHalf(pos, 270);
                #endregion
                #region Between
                else if (A == CellType.Grass && B == CellType.Water && C == CellType.Water && D == CellType.Grass) GenerateMeshTypeBetween(pos, 0);
                else if (A == CellType.Water && B == CellType.Grass && C == CellType.Grass && D == CellType.Water) GenerateMeshTypeBetween(pos, 90);
                #endregion
                #region EmptyCorner
                else if (A == CellType.Grass && B == CellType.Grass && C == CellType.Grass && D == CellType.Water) GenerateMeshTypeBlankCorner(pos, 0);
                else if (A == CellType.Grass && B == CellType.Grass && C == CellType.Water && D == CellType.Grass) GenerateMeshTypeBlankCorner(pos, 90);
                else if (A == CellType.Water && B == CellType.Grass && C == CellType.Grass && D == CellType.Grass) GenerateMeshTypeBlankCorner(pos, 180);
                else if (A == CellType.Grass && B == CellType.Water && C == CellType.Grass && D == CellType.Grass) GenerateMeshTypeBlankCorner(pos, 270);
                #endregion
                #region Normal
                else if (A == CellType.Grass && B == CellType.Grass && C == CellType.Grass && D == CellType.Grass) GenerateMeshTypeNormal(pos);
                #endregion
            }
        }

        mesh.vertices = m_vertices.ToArray();
        mesh.triangles = m_triangles.ToArray();
        mesh.RecalculateNormals();

        MeshFilter meshfilter = gameObject.AddComponent<MeshFilter>();
        meshfilter.mesh = mesh;

        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = m_terrainMaterial;
        meshRenderer.material.mainTexture = DrawTexture(m_grid);

        MeshCollider meshCollider = m_waterGrid.AddComponent<MeshCollider>();
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

    public void GenerateMeshTypeNormal(Vector3 pos)
    {
        Vector3[] points = new Vector3[4];
        points[0] = new Vector3(pos.x - 0.5f, 0, pos.z + 0.5f);
        points[1] = new Vector3(pos.x + 0.5f, 0, pos.z + 0.5f);
        points[2] = new Vector3(pos.x - 0.5f, 0, pos.z - 0.5f);
        points[3] = new Vector3(pos.x + 0.5f, 0, pos.z - 0.5f);

        Vector3 a = points[0];
        Vector3 b = points[1];
        Vector3 c = points[2];
        Vector3 d = points[3];

        Vector3[] v = new Vector3[] { a, b, c, b, d, c };
        for (int z = 0; z < v.Length; z++)
        {
            m_vertices.Add(v[z]);
            m_triangles.Add(m_triangles.Count);
        }
    }

    public void GenerateMeshTypeFillCorner(Vector3 pos, float rotate)
    {
        Vector3[] points = new Vector3[m_numPoints + 1];
        points[0] = new Vector3(pos.x - 0.5f, 0, pos.z + 0.5f);
        float angle = Mathf.PI / (2 * (m_numPoints - 1));
        for (int point = 0; point < m_numPoints; point++)
        {
            float x = Mathf.Cos(angle * point) * 0.5f - 0.5f + pos.x;
            float y = -Mathf.Sin(angle * point) * 0.5f + 0.5f + pos.z;
            points[point + 1] = new Vector3(x, 0, y);
        }

        Vector3 a = points[0];
        Vector3 b = points[1];
        Vector3 c = points[2];
        Vector3 d = points[3];
        Vector3 e = points[4];
        Vector3 f = points[5];
        Vector3 g = points[6];
        Vector3 h = points[7];
        Vector3 i = points[8];

        Vector3[] v = new Vector3[] { a, b, c, a, c, d, a, d, e, a, e, f, a, f, g, a, g, h, a, h, i };
        List<Vector3> vertices = new List<Vector3>();
        for (int z = 0; z < v.Length; z++)
        {
            vertices.Add(v[z]);
            m_triangles.Add(m_triangles.Count);
        }

        vertices = RotateTriangles(vertices, pos, new Vector3(0, rotate, 0));
        m_vertices.AddRange(vertices);
    }

    public void GenerateMeshTypeBlankCorner(Vector3 pos, float rotate)
    {
        Vector3[] points = new Vector3[m_numPoints + 5];
        points[0] = new Vector3(pos.x - 0.5f, 0, pos.z + 0.5f);
        points[1] = new Vector3(pos.x, 0, pos.z + 0.5f);
        points[2] = new Vector3(pos.x + 0.5f, 0, pos.z + 0.5f);
        points[3] = new Vector3(pos.x - 0.5f, 0, pos.z - 0.5f);
        points[4] = new Vector3(pos.x, 0, pos.z);
        float angle = Mathf.PI / (2 * (m_numPoints - 1));
        for (int point = 0; point < m_numPoints; point++)
        {
            float x = pos.x - Mathf.Cos(angle * point) * 0.5f + 0.5f;
            float y = pos.z + Mathf.Sin(angle * point) * 0.5f - 0.5f;
            points[point + 5] = new Vector3(x, 0, y);
        }

        Vector3 a = points[0];
        Vector3 b = points[1];
        Vector3 c = points[2];
        Vector3 d = points[3];
        Vector3 e = points[4];
        Vector3 f = points[5];
        Vector3 g = points[6];
        Vector3 h = points[7];
        Vector3 i = points[8];
        Vector3 j = points[9];
        Vector3 k = points[10];
        Vector3 l = points[11];
        Vector3 m = points[12];

        Vector3[] v = new Vector3[] { a, b, d, b, f, d, e, b, c, c, m, e, f, e, g, g, e, h, h, e, i, i, e, j, j, e, k, k, e, l, l, e, m };
        List<Vector3> vertices = new List<Vector3>();
        for (int z = 0; z < v.Length; z++)
        {
            vertices.Add(v[z]);
            m_triangles.Add(m_triangles.Count);
        }

        vertices = RotateTriangles(vertices, pos, new Vector3(0, rotate, 0));
        m_vertices.AddRange(vertices);
    }

    public void GenerateMeshTypeHalf(Vector3 pos, float rotate)
    {
        Vector3[] points = new Vector3[4];
        points[0] = new Vector3(pos.x - 0.5f, 0, pos.z + 0.5f);
        points[1] = new Vector3(pos.x, 0, pos.z + 0.5f);
        points[2] = new Vector3(pos.x - 0.5f, 0, pos.z - 0.5f);
        points[3] = new Vector3(pos.x, 0, pos.z - 0.5f);

        Vector3 a = points[0];
        Vector3 b = points[1];
        Vector3 c = points[2];
        Vector3 d = points[3];

        Vector3[] v = new Vector3[] { a, b, c, b, d, c };
        List<Vector3> vertices = new List<Vector3>();
        for (int z = 0; z < v.Length; z++)
        {
            vertices.Add(v[z]);
            m_triangles.Add(m_triangles.Count);
        }

        vertices = RotateTriangles(vertices, pos, new Vector3(0, rotate, 0));
        m_vertices.AddRange(vertices);
    }

    public void GenerateMeshTypeBetween(Vector3 pos, float rotate)
    {
        Vector3[] points = new Vector3[(int)m_numPoints * 2 + 2];
        points[0] = new Vector3(pos.x - 0.5f, 0, pos.z + 0.5f);
        points[1] = new Vector3(pos.x + 0.5f, 0, pos.z - 0.5f);
        float angle = Mathf.PI / (2 * (m_numPoints - 1));
        for (int point = 0; point < m_numPoints; point++)
        {
            float x = pos.x - Mathf.Cos(angle * point) * 0.5f + 0.5f;
            float y = pos.z - Mathf.Sin(angle * point) * 0.5f + 0.5f;
            points[point + 2] = new Vector3(x, 0, y);
        }

        for (int point = 0; point < m_numPoints; point++)
        {
            float x = pos.x + Mathf.Cos(angle * point) * 0.5f - 0.5f;
            float y = pos.z + Mathf.Sin(angle * point) * 0.5f - 0.5f;
            points[point + 2 + m_numPoints] = new Vector3(x, 0, y);
        }

        Vector3 a = points[0];
        Vector3 b = points[1];
        Vector3 c = points[2];
        Vector3 d = points[3];
        Vector3 e = points[4];
        Vector3 f = points[5];
        Vector3 g = points[6];
        Vector3 h = points[7];
        Vector3 i = points[8];
        Vector3 j = points[9];
        Vector3 k = points[10];
        Vector3 l = points[11];
        Vector3 m = points[12];
        Vector3 n = points[13];
        Vector3 o = points[14];
        Vector3 p = points[15];
        Vector3 q = points[16];
        Vector3 r = points[17];

        Vector3[] v = new Vector3[] { a, c, r, r, c, q, q, c, d, q, d, p, p, d, e, p, e, o, o, e, f, o, f, n, n, f, g, n, g, m, m, g, h, m, h, l, l, h, i, l, i, k, k, i, j, k, j, b };
        List<Vector3> vertices = new List<Vector3>();
        for (int z = 0; z < v.Length; z++)
        {
            vertices.Add(v[z]);
            m_triangles.Add(m_triangles.Count);
        }

        vertices = RotateTriangles(vertices, pos, new Vector3(0, rotate, 0));
        m_vertices.AddRange(vertices);
    }

    public List<Vector3> RotateTriangles(List<Vector3> vertices, Vector3 pivot, Vector3 eulerRotation)
    {
        var rotation = Quaternion.Euler(eulerRotation);

        var updatedVertices = vertices;
        for (int i = 0; i < vertices.Count; i++)
        {
            updatedVertices[i] = rotation * (vertices[i] - pivot) + pivot;
        }

        return updatedVertices;
    }
}

[Serializable]
public class TerrainInfo
{
    public CellType CellType;
    public float Height;
    public Color Color;
}