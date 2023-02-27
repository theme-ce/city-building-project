using System.Collections.Generic;
using UnityEngine;

public class CorruptedChunk : MonoBehaviour
{
    public Vector2Int ChunkPos;

    [SerializeField] private Material m_terrainMaterial;

    private Mesh m_mesh;
    private int m_size = 50;
    private DualGridCell[,] m_dualGrid;
    private Cell[,] m_grid;
    private List<Vector3> m_vertices = new List<Vector3>();
    private List<int> m_triangles = new List<int>();
    private List<Vector2> m_uvs = new List<Vector2>();
    private int m_numPoints = 8;

    public void Init(Cell[,] gridData, DualGridCell[,] dualGridData, int size, Vector2Int chunkPos)
    {
        m_size = size;

        m_grid = gridData;
        m_dualGrid = dualGridData;
        m_mesh = new Mesh();
        ChunkPos = chunkPos;

        DrawTerrainMesh(m_grid);
    }

    public void UpdateChunk(Cell[,] gridData, DualGridCell[,] dualGridData)
    {
        m_grid = gridData;
        m_dualGrid = dualGridData;

        DrawTerrainMesh(m_grid);
    }

    public void DrawTerrainMesh(Cell[,] grid)
    {
        m_mesh = new Mesh();
        m_vertices.Clear();
        m_triangles.Clear();
        m_uvs.Clear();

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
                if (A == CellType.Corrupted && B != CellType.Corrupted && C != CellType.Corrupted && D != CellType.Corrupted) GenerateMeshTypeFillCorner(pos, 0);
                else if (A != CellType.Corrupted && B == CellType.Corrupted && C != CellType.Corrupted && D != CellType.Corrupted) GenerateMeshTypeFillCorner(pos, 90);
                else if (A != CellType.Corrupted && B != CellType.Corrupted && C != CellType.Corrupted && D == CellType.Corrupted) GenerateMeshTypeFillCorner(pos, 180);
                else if (A != CellType.Corrupted && B != CellType.Corrupted && C == CellType.Corrupted && D != CellType.Corrupted) GenerateMeshTypeFillCorner(pos, 270);
                #endregion
                #region Half
                else if (A == CellType.Corrupted && B != CellType.Corrupted && C == CellType.Corrupted && D != CellType.Corrupted) GenerateMeshTypeHalf(pos, 0);
                else if (A == CellType.Corrupted && B == CellType.Corrupted && C != CellType.Corrupted && D != CellType.Corrupted) GenerateMeshTypeHalf(pos, 90);
                else if (A != CellType.Corrupted && B == CellType.Corrupted && C != CellType.Corrupted && D == CellType.Corrupted) GenerateMeshTypeHalf(pos, 180);
                else if (A != CellType.Corrupted && B != CellType.Corrupted && C == CellType.Corrupted && D == CellType.Corrupted) GenerateMeshTypeHalf(pos, 270);
                #endregion
                #region Between
                else if (A == CellType.Corrupted && B != CellType.Corrupted && C != CellType.Corrupted && D == CellType.Corrupted) GenerateMeshTypeBetween(pos, 0);
                else if (A != CellType.Corrupted && B == CellType.Corrupted && C == CellType.Corrupted && D != CellType.Corrupted) GenerateMeshTypeBetween(pos, 90);
                #endregion
                #region EmptyCorner
                else if (A == CellType.Corrupted && B == CellType.Corrupted && C == CellType.Corrupted && D != CellType.Corrupted) GenerateMeshTypeBlankCorner(pos, 0);
                else if (A == CellType.Corrupted && B == CellType.Corrupted && C != CellType.Corrupted && D == CellType.Corrupted) GenerateMeshTypeBlankCorner(pos, 90);
                else if (A != CellType.Corrupted && B == CellType.Corrupted && C == CellType.Corrupted && D == CellType.Corrupted) GenerateMeshTypeBlankCorner(pos, 180);
                else if (A == CellType.Corrupted && B != CellType.Corrupted && C == CellType.Corrupted && D == CellType.Corrupted) GenerateMeshTypeBlankCorner(pos, 270);
                #endregion
                #region Normal
                else if (A == CellType.Corrupted && B == CellType.Corrupted && C == CellType.Corrupted && D == CellType.Corrupted) GenerateMeshTypeNormal(pos);
                #endregion
            }
        }

        for (int i = 0; i < m_vertices.Count; i++)
        {
            Vector2 grassUV = new Vector2((256f * 2f - 128) / 2048f, (256f * 8f - 128) / 2048f);
            m_uvs.Add(grassUV);
        }
        m_mesh.vertices = m_vertices.ToArray();
        m_mesh.triangles = m_triangles.ToArray();
        m_mesh.uv = m_uvs.ToArray();
        m_mesh.RecalculateNormals();
        m_mesh.RecalculateBounds();
        m_mesh.RecalculateTangents();

        MeshFilter meshfilter = gameObject.GetComponent<MeshFilter>();
        if (meshfilter == null) meshfilter = gameObject.AddComponent<MeshFilter>();
        meshfilter.mesh = m_mesh;

        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = m_terrainMaterial;

        MeshCollider collider = gameObject.GetComponent<MeshCollider>();
        if (collider == null) collider = gameObject.AddComponent<MeshCollider>();

        int groundLayer = LayerMask.NameToLayer("Ground");
        gameObject.layer = groundLayer;
    }

    #region Mesh Generator
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

        List<Vector3> vertices = new List<Vector3>();
        for (int z = 0; z < v.Count; z++)
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
    #endregion
}