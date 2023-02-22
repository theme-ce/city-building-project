using UnityEngine;

public class HighlightGrid : MonoBehaviour
{
    private bool m_canPlace = true;
    public bool canPlace => m_canPlace;

    [SerializeField] private Material m_highlightGridMat;

    private void Start()
    {
        m_canPlace = true;
        m_highlightGridMat.color = Color.white;
    }

    private void OnTriggerStay(Collider other)
    {
        int structureLayer = LayerMask.NameToLayer("Structure");
        int resourceLayer = LayerMask.NameToLayer("Resource");
        if (other.gameObject.layer == structureLayer || other.gameObject.layer == resourceLayer)
        {
            m_canPlace = false;
            m_highlightGridMat.color = Color.red;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        m_canPlace = true;
        m_highlightGridMat.color = Color.white;
    }
}
