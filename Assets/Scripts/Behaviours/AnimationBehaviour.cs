using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationBehaviour : MonoBehaviour, IPlayerBehaviour
{
    [Header("Input Action")]
    [SerializeField] private InputActionReference moveAction;

    private Animator anim;
    private Rigidbody rb;
    private MovementBehaviour movementBehaviour;

    public bool Initialize()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        movementBehaviour = GetComponent<MovementBehaviour>();
        return anim != null && rb != null && movementBehaviour != null;
    }
    public void UpdateBehaviour()
    {
        if (movementBehaviour == null || rb == null || anim == null) return;

        Vector3 dir = movementBehaviour.Direction.normalized;

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        
        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        float cameraRelativeX = Vector3.Dot(dir, cameraRight);
        float cameraRelativeZ = Vector3.Dot(dir, cameraForward);

        anim.SetFloat("X", cameraRelativeX);
        anim.SetFloat("Y", cameraRelativeZ);

        if (rb.linearVelocity.magnitude > 1e-5)
        {
            anim.SetBool("isMoving", true);
        }
        else
        {
            anim.SetBool("isMoving", false);
        }
    }

}
