using UnityEngine;
using UnityEngine.EventSystems;

public class GridSelector : MonoBehaviour
{
    private StructureData m_currentStructureData;
    private GameObject m_currentStructureDraggingObject;

    [SerializeField] private TerrainChunkManager m_terrainChunk;
    [SerializeField] private LayerMask m_layerMask;
    [SerializeField] private HighlightGrid m_highlightGrid;
    [SerializeField] private CameraShake m_cameraShake;

    private float m_rotateAngle = 0;

    public StructureData SetCurrentStructureData
    {
        set
        {
            m_currentStructureData = value;
            m_rotateAngle = 0;
            if (m_currentStructureData != null)
            {
                m_currentStructureDraggingObject = Instantiate(m_currentStructureData.DraggingPrefab);
                m_highlightGrid.transform.localScale = new Vector3(m_currentStructureData.SizeX * 0.1f, 0.02f, m_currentStructureData.SizeY * 0.1f);
            }
            else
            {
                Destroy(m_currentStructureDraggingObject);
                m_currentStructureDraggingObject = null;
                m_highlightGrid.transform.localScale = new Vector3(1 * 0.1f, 0.02f, 1 * 0.1f);
            }
        }
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, m_layerMask))
        {
            Vector2Int dataPos = new Vector2Int(Mathf.RoundToInt(hit.point.x), Mathf.RoundToInt(hit.point.z));
            m_highlightGrid.transform.position = new Vector3(dataPos.x, 0.02f, dataPos.y);
            if (m_currentStructureDraggingObject == null) return;
            m_currentStructureDraggingObject.transform.position = new Vector3(dataPos.x, 0.02f, dataPos.y);
            if (m_highlightGrid.canPlace)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    m_terrainChunk.CreateStructure(m_currentStructureData, dataPos, m_rotateAngle);
                    m_terrainChunk.UpdateMesh(dataPos, m_currentStructureData.SizeX, m_currentStructureData.SizeY, CellType.Structure);
                    StartCoroutine(m_cameraShake.Shake(0.1f, 0.05f));
                }
            }

        }

        if (Input.GetKeyDown("r"))
        {
            if (m_currentStructureDraggingObject)
            {
                m_currentStructureDraggingObject.transform.RotateAround(m_currentStructureDraggingObject.transform.position, m_currentStructureDraggingObject.transform.up, 90f);
                m_rotateAngle = m_currentStructureDraggingObject.transform.localEulerAngles.y;
            }
        }
    }
}
