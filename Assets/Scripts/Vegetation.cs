using UnityEngine;

public class Vegetation : MonoBehaviour, IRemoveable
{
    public void Remove()
    {
        Destroy(this.gameObject);
    }
}
