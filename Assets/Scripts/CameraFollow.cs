using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;

    private void FixedUpdate()
    {
        if (target == null) return;

        transform.position = target.position + offset;
    }
}