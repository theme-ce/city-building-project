using UnityEngine;
using UnityEngine.UI;

public class StructureToggle : MonoBehaviour
{
    [SerializeField] private StructureData structureData;
    [SerializeField] private Toggle m_toggle;
    [SerializeField] private Image m_image;
    [SerializeField] private GridSelector m_gridSelector;

    private void Awake()
    {
        m_toggle.onValueChanged.AddListener(x =>
        {
            if (x) m_gridSelector.SetCurrentStructureData = structureData;
            else m_gridSelector.SetCurrentStructureData = null;
        });
    }
}
