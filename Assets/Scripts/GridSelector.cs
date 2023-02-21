using UnityEngine;

public class GridSelector : MonoBehaviour
{
    [SerializeField] private GameObject m_houseDrag;
    [SerializeField] private TerrainChunk m_terrainChunk;
    [SerializeField] private LayerMask m_layerMask;
    [SerializeField] private HighlightGrid m_highlightGrid;

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, m_layerMask))
        {
            Vector2Int dataPos = new Vector2Int(Mathf.RoundToInt(hit.point.x), Mathf.RoundToInt(hit.point.z));
            m_highlightGrid.transform.position = new Vector3(dataPos.x, 0.02f, dataPos.y);
            m_highlightGrid.transform.localScale = new Vector3(2 * 0.1f, 1, 2 * 0.1f);
            m_houseDrag.transform.position = new Vector3(dataPos.x, 0.02f, dataPos.y);
            if (m_highlightGrid.canPlace)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    //Cell cell = m_terrainChunk.GridData[dataPos.x, dataPos.y];
                    //if (cell.CellType == CellType.Water)
                    //{
                    //    m_terrainChunk.UpdateMesh(dataPos, CellType.Grass);
                    //}
                    //else if (cell.CellType == CellType.Grass)
                    //{
                    //    m_terrainChunk.UpdateMesh(dataPos, CellType.Water);
                    //}
                    m_terrainChunk.CreateHouse(dataPos);
                }
            }

        }
    }
}
