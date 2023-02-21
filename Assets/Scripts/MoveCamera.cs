using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] private Transform m_camTransform;
    [SerializeField] private float m_camMoveSpeed = 10;
    [SerializeField] private float m_camRotateSpeed = 1;

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Quaternion rotation = Quaternion.Euler(0, m_camTransform.transform.localEulerAngles.y == 0 ? 1 : m_camTransform.transform.localEulerAngles.y, 0);
        Vector3 forward = rotation * Vector3.forward;
        Vector3 right = rotation * Vector3.right;
        transform.Translate((forward * vertical + right * horizontal) * m_camMoveSpeed * Time.deltaTime);

        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            transform.Rotate(0, mouseX * m_camRotateSpeed, 0);
        }
    }
}
