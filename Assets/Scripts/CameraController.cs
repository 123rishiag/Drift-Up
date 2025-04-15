using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Car Transform")]
    public Transform carTransform;

    [Header("Camera Variables")]
    public Vector3 cameraPositionOffset = new Vector3(0f, 5f, -6f);

    private void LateUpdate()
    {
        transform.position = carTransform.position + cameraPositionOffset;
    }
}
