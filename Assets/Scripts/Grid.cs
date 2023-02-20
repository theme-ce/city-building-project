using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Grid : MonoBehaviour
{
    [SerializeField] private GameObject[] m_treePrefabs;
    [SerializeField] private Material m_terrainMaterial;

    [SerializeField] private float m_treeNoiseScale = 5f;
    [SerializeField] private float m_treeDensity = .3f;

    private int m_size = 50;
    private DualGridCell[,] m_dualGrid;
    private Cell[,] m_grid;
    private List<Vector3> m_vertices = new List<Vector3>();
    private List<int> m_triangles = new List<int>();
    private int m_numPoints = 8;

    public void Init(Cell[,] gridData, DualGridCell[,] dualGridData, int size)
    {
        m_size = size;

        m_grid = gridData;
        m_dualGrid = dualGridData;

        DrawTerrainMesh(m_grid);
        GenerateTrees(m_grid);
    }

    void DrawTerrainMesh(Cell[,] grid)
    {
        Mesh mesh = new Mesh();

        for (int y = 0; y < m_size + 1; y++)
        {
            for (int x = 0; x < m_size + 1; x++)
            {
                Vector3 pos = new Vector3(x - 0.5f, 0, y - 0.5f);
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
                        tree.transform.localPosition = new Vector3(x, 0, y);
                        tree.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
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
        Vector3[] points = new Vector3[m_numPoints * 2 + 7];
        points[0] = new Vector3(pos.x - 0.5f, 0, pos.z + 0.5f);
        points[1] = new Vector3(pos.x - 0.5f, 0, pos.z + 0.25f);
        points[2] = new Vector3(pos.x - 0.25f, 0, pos.z + 0.25f);
        points[3] = new Vector3(pos.x - 0.5f, 0, pos.z);
        points[4] = new Vector3(pos.x, 0, pos.z + 0.5f);
        points[5] = new Vector3(pos.x - 0.5f, -1, pos.z);
        points[6] = new Vector3(pos.x, -1, pos.z + 0.5f);
        float angle = Mathf.PI / (2 * (m_numPoints - 1));
        for (int point = 0; point < m_numPoints; point++)
        {
            float x = pos.x + Mathf.Cos(angle * point) * 0.25f - 0.25f;
            float y = pos.z - Mathf.Sin(angle * point) * 0.25f + 0.25f;
            points[point + 7] = new Vector3(x, 0, y);
            points[point + m_numPoints + 7] = new Vector3(x, -1, y);
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
        Vector3 s = points[18];
        Vector3 t = points[19];
        Vector3 u = points[20];
        Vector3 v = points[21];
        Vector3 w = points[22];

        Vector3[] vs = new Vector3[] { a, e, b, e, h, b, d, b, c, c, o, d, c, n, o, c, m, n, c, l, m, c, k, l, c, j, k, c, i, j, c, h, i, f, d, o, f, o, w, p, h, e, p, e, g, w, o, n, w, n, v, v, n, m, v, m, u, u, m, l, u, l, t, t, l, k, t, k, s, s, k, j, s, j, r, r, j, i, r, i, q, q, i, h, q, h, p };
        List<Vector3> vertices = new List<Vector3>();
        for (int z = 0; z < vs.Length; z++)
        {
            vertices.Add(vs[z]);
            m_triangles.Add(m_triangles.Count);
        }

        vertices = RotateTriangles(vertices, pos, new Vector3(0, rotate, 0));
        m_vertices.AddRange(vertices);
    }

    public void GenerateMeshTypeBlankCorner(Vector3 pos, float rotate)
    {
        Vector3[] points = new Vector3[m_numPoints * 2 + 9];
        points[0] = new Vector3(pos.x - 0.5f, 0, pos.z + 0.5f);
        points[1] = new Vector3(pos.x + 0.5f, 0, pos.z + 0.5f);
        points[2] = new Vector3(pos.x - 0.5f, 0, pos.z);
        points[3] = new Vector3(pos.x + 0.5f, 0, pos.z);
        points[4] = new Vector3(pos.x - 0.5f, 0, pos.z - 0.5f);
        points[5] = new Vector3(pos.x, 0, pos.z - 0.5f);
        points[6] = new Vector3(pos.x, 0, pos.z);
        points[7] = new Vector3(pos.x, -1, pos.z - 0.5f);
        points[8] = new Vector3(pos.x + 0.5f, -1, pos.z);
        float angle = Mathf.PI / (2 * (m_numPoints - 1));
        for (int point = 0; point < m_numPoints; point++)
        {
            float x = pos.x - Mathf.Cos(angle * point) * 0.25f + 0.25f;
            float y = pos.z + Mathf.Sin(angle * point) * 0.25f - 0.25f;
            points[point + 9] = new Vector3(x, 0, y);
            points[point + m_numPoints + 9] = new(x, -1, y);
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
        Vector3 s = points[18];
        Vector3 t = points[19];
        Vector3 u = points[20];
        Vector3 v = points[21];
        Vector3 w = points[22];
        Vector3 xx = points[23];
        Vector3 yy = points[24];

        Vector3[] vs = new Vector3[] { a, b, c, b, d, c, c, g, e, g, f, e, g, k, j, g, l, k, g, m, l, g, n, m, g, o, n, g, p, o, g, q, p, h, f, j, h, j, r, r, j, k, r, k, s, s, k, l, s, l, t, t, l, m, t, m, u, u, m, n, u, n, v, v, n, o, v, o, w, w, o, p, w, p, xx, xx, p, q, xx, q, yy, yy, q, d, yy, d, i };
        List<Vector3> vertices = new List<Vector3>();
        for (int z = 0; z < vs.Length; z++)
        {
            vertices.Add(vs[z]);
            m_triangles.Add(m_triangles.Count);
        }

        vertices = RotateTriangles(vertices, pos, new Vector3(0, rotate, 0));
        m_vertices.AddRange(vertices);
    }

    public void GenerateMeshTypeHalf(Vector3 pos, float rotate)
    {
        Vector3[] points = new Vector3[6];
        points[0] = new Vector3(pos.x - 0.5f, 0, pos.z + 0.5f);
        points[1] = new Vector3(pos.x, 0, pos.z + 0.5f);
        points[2] = new Vector3(pos.x - 0.5f, 0, pos.z - 0.5f);
        points[3] = new Vector3(pos.x, 0, pos.z - 0.5f);
        points[4] = new Vector3(pos.x, -1, pos.z + 0.5f);
        points[5] = new Vector3(pos.x, -1, pos.z - 0.5f);

        Vector3 a = points[0];
        Vector3 b = points[1];
        Vector3 c = points[2];
        Vector3 d = points[3];
        Vector3 e = points[4];
        Vector3 f = points[5];

        Vector3[] v = new Vector3[] { a, b, c, b, d, c, b, e, d, e, f, d };
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