using UnityEngine;

public class GridSelector : MonoBehaviour
{
    [SerializeField] private Grid m_grid;
    [SerializeField] private LayerMask m_layerMask;
    [SerializeField] private GameObject m_highlightGrid;

    private float m_updateTime = 0.1f;
    private float m_timeSinceLastUpdate = 0f;

    private void Update()
    {
        m_timeSinceLastUpdate += Time.deltaTime;
        if (m_timeSinceLastUpdate > m_updateTime)
        {
            m_timeSinceLastUpdate = 0;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, m_layerMask))
            {
                float x = hit.point.x;
                float z = hit.point.z;
            }
        }
    }
}
