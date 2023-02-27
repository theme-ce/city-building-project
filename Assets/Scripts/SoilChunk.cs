using System.Collections.Generic;
using UnityEngine;

public class SoilChunk : MonoBehaviour
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

    public void Init(Cell[,] gridData, DualGridCell[,] dualGridData, int size, Vector2Int chunkPos)
    {
        m_size = size;

        m_grid = gridData;
        m_dualGrid = dualGridData;
        m_mesh = new Mesh();
        ChunkPos = chunkPos;

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
                if (A == CellType.Soil && B != CellType.Soil && C != CellType.Soil && D != CellType.Soil) MeshGenerator.Instance.GenerateMeshTypeFillCorner(pos, 0, ref m_vertices, ref m_triangles);
                else if (A != CellType.Soil && B == CellType.Soil && C != CellType.Soil && D != CellType.Soil) MeshGenerator.Instance.GenerateMeshTypeFillCorner(pos, 90, ref m_vertices, ref m_triangles);
                else if (A != CellType.Soil && B != CellType.Soil && C != CellType.Soil && D == CellType.Soil) MeshGenerator.Instance.GenerateMeshTypeFillCorner(pos, 180, ref m_vertices, ref m_triangles);
                else if (A != CellType.Soil && B != CellType.Soil && C == CellType.Soil && D != CellType.Soil) MeshGenerator.Instance.GenerateMeshTypeFillCorner(pos, 270, ref m_vertices, ref m_triangles);
                #endregion
                #region Half
                else if (A == CellType.Soil && B != CellType.Soil && C == CellType.Soil && D != CellType.Soil) MeshGenerator.Instance.GenerateMeshTypeHalf(pos, 0, ref m_vertices, ref m_triangles);
                else if (A == CellType.Soil && B == CellType.Soil && C != CellType.Soil && D != CellType.Soil) MeshGenerator.Instance.GenerateMeshTypeHalf(pos, 90, ref m_vertices, ref m_triangles);
                else if (A != CellType.Soil && B == CellType.Soil && C != CellType.Soil && D == CellType.Soil) MeshGenerator.Instance.GenerateMeshTypeHalf(pos, 180, ref m_vertices, ref m_triangles);
                else if (A != CellType.Soil && B != CellType.Soil && C == CellType.Soil && D == CellType.Soil) MeshGenerator.Instance.GenerateMeshTypeHalf(pos, 270, ref m_vertices, ref m_triangles);
                #endregion
                #region Between
                else if (A == CellType.Soil && B != CellType.Soil && C != CellType.Soil && D == CellType.Soil) MeshGenerator.Instance.GenerateMeshTypeBetween(pos, 0, ref m_vertices, ref m_triangles);
                else if (A != CellType.Soil && B == CellType.Soil && C == CellType.Soil && D != CellType.Soil) MeshGenerator.Instance.GenerateMeshTypeBetween(pos, 90, ref m_vertices, ref m_triangles);
                #endregion
                #region EmptyCorner
                else if (A == CellType.Soil && B == CellType.Soil && C == CellType.Soil && D != CellType.Soil) MeshGenerator.Instance.GenerateMeshTypeBlankCorner(pos, 0, ref m_vertices, ref m_triangles);
                else if (A == CellType.Soil && B == CellType.Soil && C != CellType.Soil && D == CellType.Soil) MeshGenerator.Instance.GenerateMeshTypeBlankCorner(pos, 90, ref m_vertices, ref m_triangles);
                else if (A != CellType.Soil && B == CellType.Soil && C == CellType.Soil && D == CellType.Soil) MeshGenerator.Instance.GenerateMeshTypeBlankCorner(pos, 180, ref m_vertices, ref m_triangles);
                else if (A == CellType.Soil && B != CellType.Soil && C == CellType.Soil && D == CellType.Soil) MeshGenerator.Instance.GenerateMeshTypeBlankCorner(pos, 270, ref m_vertices, ref m_triangles);
                #endregion
                #region Normal
                else if (A == CellType.Soil && B == CellType.Soil && C == CellType.Soil && D == CellType.Soil) MeshGenerator.Instance.GenerateMeshTypeNormal(pos, ref m_vertices, ref m_triangles);
                #endregion
            }
        }

        for (int i = 0; i < m_vertices.Count; i++)
        {
            Vector2 soilUV = new Vector2((256f * 5f - 128) / 2048f, (256f * 8f - 128) / 2048f);
            m_uvs.Add(soilUV);
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
}