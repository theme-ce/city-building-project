using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField] private GameObject[] m_treePrefabs;
    [SerializeField] private Material m_terrainMaterial;
    [SerializeField] private Material m_edgeMaterials;

    [SerializeField] private float m_waterLevel = .4f;
    [SerializeField] private float m_scale = .1f;
    [SerializeField] private float m_treeNoiseScale = .05f;
    [SerializeField] private float m_treeDensity = .3f;
    [SerializeField] private int m_size = 100;

    private Cell[,] m_grid;
    private List<List<Vector2>> m_squares = new List<List<Vector2>>();

    private void Start()
    {
        float[,] noiseMap = new float[m_size, m_size];
        float xOffset = Random.Range(-10000f, 10000f);
        float yOffset = Random.Range(-10000f, 10000f);
        for (int y = 0; y < m_size; y++)
        {
            for (int x = 0; x < m_size; x++)
            {
                float noiseValue = Mathf.PerlinNoise(x * m_scale + xOffset, y * m_scale + yOffset);
                noiseMap[x, y] = noiseValue;
            }
        }

        float[,] falloffMap = new float[m_size, m_size];
        for (int y = 0; y < m_size; y++)
        {
            for (int x = 0; x < m_size; x++)
            {
                float xv = x / (float)m_size * 2 - 1;
                float yv = y / (float)m_size * 2 - 1;
                float v = Mathf.Max(Mathf.Abs(xv), Mathf.Abs(yv));
                falloffMap[x, y] = Mathf.Pow(v, 3f) / (Mathf.Pow(v, 3f) + Mathf.Pow(2.2f - 2.2f * v, 3f));
            }
        }

        m_grid = new Cell[m_size, m_size];
        for (int y = 0; y < m_size; y++)
        {
            for (int x = 0; x < m_size; x++)
            {
                float noiseValue = noiseMap[x, y];
                noiseValue -= falloffMap[x, y];
                bool isWater = noiseValue < m_waterLevel;
                Cell cell = new Cell(isWater);
                m_grid[x, y] = cell;
                if (!cell.IsWater)
                {
                    m_squares.Add(new List<Vector2>() { new Vector2(x - .5f, y + .5f), new Vector2(x + .5f, y + .5f), new Vector2(x - .5f, y - .5f), new Vector2(x + .5f, y - .5f) });
                }
            }
        }
        DrawTerrainMesh(m_grid);
        DrawEdgeMesh(m_grid);
        GenerateTrees(m_grid);
    }

    void DrawTerrainMesh(Cell[,] grid)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        for (int y = 0; y < m_size; y++)
        {
            for (int x = 0; x < m_size; x++)
            {
                Cell cell = grid[x, y];
                if (!cell.IsWater)
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

        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
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
                if (!cell.IsWater)
                {
                    if (x > 0)
                    {
                        Cell left = grid[x - 1, y];
                        if (left.IsWater)
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
                        if (right.IsWater)
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
                        if (down.IsWater)
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
                        if (up.IsWater)
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
                float noiseValue = Mathf.PerlinNoise(x * m_treeNoiseScale + xOffset, y * m_treeNoiseScale + yOffset);
                noiseMap[x, y] = noiseValue;
            }
        }

        for (int y = 0; y < m_size; y++)
        {
            for (int x = 0; x < m_size; x++)
            {
                Cell cell = grid[x, y];
                if (!cell.IsWater)
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

