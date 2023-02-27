using UnityEngine;
using UnityEngine.EventSystems;

public class GridSelector : MonoBehaviour
{
    [SerializeField] private TerrainChunkManager m_terrainChunk;
    [SerializeField] private LayerMask m_layerMask;
    [SerializeField] private HighlightGrid m_highlightGrid;
    [SerializeField] private CameraShake m_cameraShake;

    private float m_rotateAngle = 0;

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
        }
    }
}
