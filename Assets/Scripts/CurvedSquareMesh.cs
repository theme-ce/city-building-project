using System.Collections.Generic;
using UnityEngine;

public class CurvedSquareMesh : MonoBehaviour
{
    [SerializeField] private Material m_material;
    private int m_numPoints = 8;
    private List<Vector3> m_vertices = new List<Vector3>();
    private List<int> m_triangles = new List<int>();

    void Start()
    {
        Mesh mesh = new Mesh();

        GenerateMeshTypeNormal(Vector3.zero);
        GenerateMeshTypeFillCorner(new Vector3(2, 0, 0), 90);
        GenerateMeshTypeBlankCorner(new Vector3(4, 0, 0), 180);
        GenerateMeshTypeHalf(new Vector3(6, 0, 0), 270);
        GenerateMeshTypeBetween(new Vector3(8, 0, 0), 90);

        mesh.vertices = m_vertices.ToArray();
        mesh.triangles = m_triangles.ToArray();
        mesh.RecalculateNormals();

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = m_material;
    }

    void DrawTerrainMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = m_vertices.ToArray();
        mesh.triangles = m_triangles.ToArray();
        mesh.RecalculateNormals();

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = m_material;
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
        points[3] = new Vector3(pos.x - 0.5f, 0, pos.y - 0.5f);
        points[4] = new Vector3(pos.x, 0, pos.z);
        float angle = Mathf.PI / (2 * (m_numPoints - 1));
        for (int point = 0; point < m_numPoints; point++)
        {
            float x = -Mathf.Cos(angle * point) * 0.5f + (0.5f + pos.x);
            float y = Mathf.Sin(angle * point) * 0.5f - (0.5f + pos.z);
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
        points[0] = new Vector3(pos.x, 0, pos.z + 0.5f);
        points[1] = new Vector3(pos.x + 0.5f, 0, pos.z + 0.5f);
        points[2] = new Vector3(pos.x, 0, pos.z - 0.5f);
        points[3] = new Vector3(pos.x + 0.5f, 0, pos.z - 0.5f);

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

public enum CurvedType : byte
{
    Normal = 0,
    FillCorner = 1,
    Half = 2,
    FillBetween = 3,
    EmptyCorner = 4,
}