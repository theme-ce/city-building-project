using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public static MeshGenerator Instance { get; private set; }

    [SerializeField] private int m_numPoints = 8;

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

    #region Mesh Generator
    public void GenerateMeshTypeNormal(Vector3 pos, ref List<Vector3> vertices, ref List<int> triangles)
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
            vertices.Add(v[z]);
            triangles.Add(triangles.Count);
        }
    }

    public void GenerateMeshTypeFillCorner(Vector3 pos, float rotate, ref List<Vector3> vertices, ref List<int> triangles)
    {
        List<Vector3> curverPoints1 = new List<Vector3>();
        List<Vector3> bottomCurvedPoints1 = new List<Vector3>();
        float angle = Mathf.PI / (2 * (m_numPoints - 1));
        for (int point = 0; point < m_numPoints; point++)
        {
            float x = pos.x + Mathf.Cos(angle * point) * 0.25f - 0.25f;
            float y = pos.z - Mathf.Sin(angle * point) * 0.25f + 0.25f;
            curverPoints1.Add(new Vector3(x, 0, y));
            bottomCurvedPoints1.Add(new Vector3(x, -1, y));
        }

        Vector3 a = new Vector3(pos.x - 0.5f, 0, pos.z + 0.5f);
        Vector3 b = new Vector3(pos.x - 0.5f, 0, pos.z + 0.25f);
        Vector3 c = new Vector3(pos.x - 0.25f, 0, pos.z + 0.25f);
        Vector3 d = new Vector3(pos.x - 0.5f, 0, pos.z);
        Vector3 e = new Vector3(pos.x, 0, pos.z + 0.5f);
        Vector3 f = new Vector3(pos.x - 0.5f, -1, pos.z);
        Vector3 g = new Vector3(pos.x, -1, pos.z + 0.5f);
        Vector3 h = new Vector3(pos.x - 0.25f, 0, pos.z);
        Vector3 i = new Vector3(pos.x, 0, pos.z + 0.25f);
        Vector3 j = new Vector3(pos.x - 0.25f, -1, pos.z);
        Vector3 k = new Vector3(pos.x, -1, pos.z + 0.25f);

        List<Vector3> curvedVertices1 = new List<Vector3>();
        List<Vector3> edgeCurvedVertices1 = new List<Vector3>();
        for (int cp = 0; cp < curverPoints1.Count; cp++)
        {
            if (cp > 0)
            {
                curvedVertices1.Add(c);
                curvedVertices1.Add(curverPoints1[cp - 1]);
                curvedVertices1.Add(curverPoints1[cp]);
            }

            if (cp < curverPoints1.Count - 1)
            {
                edgeCurvedVertices1.Add(curverPoints1[cp]);
                edgeCurvedVertices1.Add(bottomCurvedPoints1[cp]);
                edgeCurvedVertices1.Add(bottomCurvedPoints1[cp + 1]);

                edgeCurvedVertices1.Add(curverPoints1[cp]);
                edgeCurvedVertices1.Add(bottomCurvedPoints1[cp + 1]);
                edgeCurvedVertices1.Add(curverPoints1[cp + 1]);
            }
        }

        List<Vector3> v = new List<Vector3> { a, e, b, b, e, i, d, b, c, c, h, d, k, i, e, e, g, k, f, d, h, h, j, f };
        v.AddRange(curvedVertices1);
        v.AddRange(edgeCurvedVertices1);
        for (int z = 0; z < v.Count; z++)
        {
            triangles.Add(triangles.Count);
        }

        v = RotateTriangles(v, pos, new Vector3(0, rotate, 0));
        vertices.AddRange(v);
    }

    public void GenerateMeshTypeBlankCorner(Vector3 pos, float rotate, ref List<Vector3> vertices, ref List<int> triangles)
    {
        Vector3 a = new Vector3(pos.x - 0.5f, 0, pos.z + 0.5f);
        Vector3 b = new Vector3(pos.x + 0.5f, 0, pos.z + 0.5f);
        Vector3 c = new Vector3(pos.x - 0.5f, 0, pos.z);
        Vector3 d = new Vector3(pos.x + 0.5f, 0, pos.z);
        Vector3 e = new Vector3(pos.x - 0.5f, 0, pos.z - 0.5f);
        Vector3 f = new Vector3(pos.x, 0, pos.z - 0.5f);
        Vector3 g = new Vector3(pos.x, 0, pos.z);
        Vector3 h = new Vector3(pos.x, 0, pos.z - 0.25f);
        Vector3 i = new Vector3(pos.x + 0.25f, 0, pos.z);
        Vector3 j = new Vector3(pos.x, -1, pos.z - 0.5f);
        Vector3 k = new Vector3(pos.x, -1, pos.z - 0.25f);
        Vector3 l = new Vector3(pos.x + 0.25f, -1, pos.z);
        Vector3 m = new Vector3(pos.x + 0.5f, -1, pos.z);

        List<Vector3> curverPoints1 = new List<Vector3>();
        List<Vector3> bottomCurvedPoints1 = new List<Vector3>();
        float angle = Mathf.PI / (2 * (m_numPoints - 1));
        for (int point = 0; point < m_numPoints; point++)
        {
            float x = pos.x - Mathf.Cos(angle * point) * 0.25f + 0.25f;
            float y = pos.z + Mathf.Sin(angle * point) * 0.25f - 0.25f;
            curverPoints1.Add(new Vector3(x, 0, y));
            bottomCurvedPoints1.Add(new(x, -1, y));
        }

        List<Vector3> curvedVertices1 = new List<Vector3>();
        List<Vector3> edgeCurvedVertices1 = new List<Vector3>();
        curverPoints1.Reverse();
        bottomCurvedPoints1.Reverse();
        for (int cp = 0; cp < curverPoints1.Count; cp++)
        {
            if (cp > 0)
            {
                curvedVertices1.Add(g);
                curvedVertices1.Add(curverPoints1[cp - 1]);
                curvedVertices1.Add(curverPoints1[cp]);
            }

            if (cp < curverPoints1.Count - 1)
            {
                edgeCurvedVertices1.Add(curverPoints1[cp]);
                edgeCurvedVertices1.Add(bottomCurvedPoints1[cp]);
                edgeCurvedVertices1.Add(bottomCurvedPoints1[cp + 1]);

                edgeCurvedVertices1.Add(curverPoints1[cp]);
                edgeCurvedVertices1.Add(bottomCurvedPoints1[cp + 1]);
                edgeCurvedVertices1.Add(curverPoints1[cp + 1]);
            }
        }

        List<Vector3> v = new List<Vector3> { a, b, c, b, d, c, e, c, g, g, f, e, j, f, h, h, k, j, i, d, m, m, l, i };
        v.AddRange(curvedVertices1);
        v.AddRange(edgeCurvedVertices1);
        for (int z = 0; z < v.Count; z++)
        {
            triangles.Add(triangles.Count);
        }

        v = RotateTriangles(v, pos, new Vector3(0, rotate, 0));
        vertices.AddRange(v);
    }

    public void GenerateMeshTypeHalf(Vector3 pos, float rotate, ref List<Vector3> vertices, ref List<int> triangles)
    {
        Vector3 a = new Vector3(pos.x - 0.5f, 0, pos.z + 0.5f);
        Vector3 b = new Vector3(pos.x, 0, pos.z + 0.5f);
        Vector3 c = new Vector3(pos.x - 0.5f, 0, pos.z - 0.5f);
        Vector3 d = new Vector3(pos.x, 0, pos.z - 0.5f);
        Vector3 e = new Vector3(pos.x, -1, pos.z + 0.5f);
        Vector3 f = new Vector3(pos.x, -1, pos.z - 0.5f);

        List<Vector3> v = new List<Vector3> { a, b, c, b, d, c, b, e, d, e, f, d };
        for (int z = 0; z < v.Count; z++)
        {
            triangles.Add(triangles.Count);
        }

        v = RotateTriangles(v, pos, new Vector3(0, rotate, 0));
        vertices.AddRange(v);
    }

    public void GenerateMeshTypeBetween(Vector3 pos, float rotate, ref List<Vector3> vertices, ref List<int> triangles)
    {
        List<Vector3> curverPoints1 = new List<Vector3>();
        List<Vector3> bottomCurvedPoints1 = new List<Vector3>();
        float angle = Mathf.PI / (2 * (m_numPoints - 1));
        for (int point = 0; point < m_numPoints; point++)
        {
            float x = pos.x - Mathf.Cos(angle * point) * 0.25f + 0.25f;
            float y = pos.z - Mathf.Sin(angle * point) * 0.25f + 0.25f;
            curverPoints1.Add(new Vector3(x, 0, y));
            bottomCurvedPoints1.Add(new Vector3(x, -1, y));
        }

        Vector3 a = new Vector3(pos.x, 0, pos.z);
        Vector3 b = new Vector3(pos.x - 0.5f, 0, pos.z + 0.5f);
        Vector3 c = new Vector3(pos.x, 0, pos.z + 0.5f);
        Vector3 d = new Vector3(pos.x - 0.5f, 0, pos.z);
        Vector3 e = new Vector3(pos.x, 0, pos.z - 0.5f);
        Vector3 f = new Vector3(pos.x + 0.5f, 0, pos.z);
        Vector3 g = new Vector3(pos.x + 0.5f, 0, pos.z - 0.5f);
        Vector3 h = new Vector3(pos.x - 0.5f, -1, pos.z);
        Vector3 i = new Vector3(pos.x - 0.25f, -1, pos.z);
        Vector3 j = new Vector3(pos.x + 0.25f, -1, pos.z);
        Vector3 k = new Vector3(pos.x + 0.5f, -1, pos.z);
        Vector3 l = new Vector3(pos.x, 0, pos.z + 0.25f);
        Vector3 m = new Vector3(pos.x, 0, pos.z - 0.25f);
        Vector3 n = new Vector3(pos.x, -1, pos.z + 0.25f);
        Vector3 o = new Vector3(pos.x, -1, pos.z + 0.5f);
        Vector3 p = new Vector3(pos.x, -1, pos.z - 0.5f);
        Vector3 q = new Vector3(pos.x, -1, pos.z - 0.25f);
        Vector3 r = new Vector3(pos.x - 0.25f, 0, pos.z);
        Vector3 s = new Vector3(pos.x + 0.25f, 0, pos.z);

        List<Vector3> curvedVertices1 = new List<Vector3>();
        List<Vector3> edgeCurvedVertices1 = new List<Vector3>();
        for (int cp = 0; cp < curverPoints1.Count; cp++)
        {
            if (cp > 0)
            {
                curvedVertices1.Add(a);
                curvedVertices1.Add(curverPoints1[cp - 1]);
                curvedVertices1.Add(curverPoints1[cp]);
            }

            if (cp < curverPoints1.Count - 1)
            {
                edgeCurvedVertices1.Add(curverPoints1[cp]);
                edgeCurvedVertices1.Add(bottomCurvedPoints1[cp]);
                edgeCurvedVertices1.Add(bottomCurvedPoints1[cp + 1]);

                edgeCurvedVertices1.Add(curverPoints1[cp]);
                edgeCurvedVertices1.Add(bottomCurvedPoints1[cp + 1]);
                edgeCurvedVertices1.Add(curverPoints1[cp + 1]);
            }
        }

        List<Vector3> curverPoints2 = new List<Vector3>();
        List<Vector3> bottomCurvedPoints2 = new List<Vector3>();
        for (int point = 0; point < m_numPoints; point++)
        {
            float x = pos.x + Mathf.Cos(angle * point) * 0.25f - 0.25f;
            float y = pos.z + Mathf.Sin(angle * point) * 0.25f - 0.25f;
            curverPoints2.Add(new Vector3(x, 0, y));
            bottomCurvedPoints2.Add(new Vector3(x, -1, y));
        }

        List<Vector3> curvedVertices2 = new List<Vector3>();
        List<Vector3> edgeCurvedVertices2 = new List<Vector3>();
        for (int cp = 0; cp < curverPoints2.Count; cp++)
        {
            if (cp > 0)
            {
                curvedVertices2.Add(a);
                curvedVertices2.Add(curverPoints2[cp - 1]);
                curvedVertices2.Add(curverPoints2[cp]);
            }

            if (cp < curverPoints1.Count - 1)
            {
                edgeCurvedVertices2.Add(curverPoints2[cp]);
                edgeCurvedVertices2.Add(bottomCurvedPoints2[cp]);
                edgeCurvedVertices2.Add(bottomCurvedPoints2[cp + 1]);

                edgeCurvedVertices2.Add(curverPoints2[cp]);
                edgeCurvedVertices2.Add(bottomCurvedPoints2[cp + 1]);
                edgeCurvedVertices2.Add(curverPoints2[cp + 1]);
            }
        }


        List<Vector3> v = new List<Vector3> { b, a, d, b, c, a, a, f, g, a, g, e, h, d, r, h, r, i, n, l, c, n, c, o, q, m, e, q, e, p, k, f, s, k, s, j };
        v.AddRange(curvedVertices1);
        v.AddRange(edgeCurvedVertices1);
        v.AddRange(curvedVertices2);
        v.AddRange(edgeCurvedVertices2);

        for (int z = 0; z < v.Count; z++)
        {
            triangles.Add(triangles.Count);
        }

        v = RotateTriangles(v, pos, new Vector3(0, rotate, 0));
        vertices.AddRange(v);
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
    #endregion
}
