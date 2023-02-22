using UnityEngine;

[CreateAssetMenu(fileName = "StructureData-0", menuName = "Structure/Structure Data")]
public class StructureData : ScriptableObject
{
    public string Name;
    public string Description;
    public int SizeX;
    public int SizeY;
    public GameObject NormalPrefab;
    public GameObject DraggingPrefab;
}
