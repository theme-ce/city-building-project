using UnityEngine;

[CreateAssetMenu(fileName = "Resource-0", menuName = "Resources/Resource Data")]
public class Resource : ScriptableObject
{
    public string Name;
    public string Description;
    public int SizeX;
    public int SizeY;
    public float Density;
    public GameObject Prefab;
}
