using UnityEngine;

public class Structure : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.GetComponent<IRemoveable>() != null)
        {
            other.transform.GetComponent<IRemoveable>().Remove();
        }
    }
}
