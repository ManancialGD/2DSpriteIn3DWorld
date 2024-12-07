using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MovementBehaviour : MonoBehaviour, IPlayerBehaviour
{
    [Header("Input Action")]
    [SerializeField] private InputActionReference moveAction;

    [Header("Movement Setting")]

    [SerializeField] private float maxSpeed = 2.5f;

    [Space]

    [SerializeField] private float accSpeed = 12.5f;
    [SerializeField] private float decelerateSpeed = 12.5f;

    public bool CanMove { get; private set; } = true;
    private Rigidbody rb;
    private Vector2 movementInput;

    public bool Initialize()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = 0;
        rb.freezeRotation = true;

        return moveAction != null && rb != null;
    }

    private void OnEnable()
    {
        moveAction.action.performed += MoveActionChanged;
        moveAction.action.canceled += MoveActionChanged;
    }

    private void OnDisable()
    {
        moveAction.action.performed -= MoveActionChanged;
        moveAction.action.canceled -= MoveActionChanged;
    }

    public void UpdateBehaviour()
    {
        if (CanMove) UpdateVelocity();
    }
    private void Update()
    {
        UpdateDrag();
    }

    private void UpdateDrag()
    {
        // Check if there is no movement input
        if ((Mathf.Abs(movementInput.x) <= 1e-5 && Mathf.Abs(movementInput.y) <= 1e-5) || !CanMove)
        {
            // Get the current linear velocity of the Rigidbody
            Vector3 velocity = rb.linearVelocity;

            // Gradually reduce the velocity to create a deceleration effect
            velocity -= velocity.normalized * decelerateSpeed * Time.fixedDeltaTime;

            // Ensure that the velocity does not reverse direction
            if ((velocity.x < 1e-5 && rb.linearVelocity.x > 1e-5) || (velocity.x > 1e-5 && rb.linearVelocity.x < 1e-5))
            {
                velocity.x = 0;
            }
            if ((velocity.y < 1e-5 && rb.linearVelocity.y > 1e-5) || (velocity.y > 1e-5 && rb.linearVelocity.y < 1e-5))
            {
                velocity.y = 0;
            }
            if ((velocity.z < 1e-5 && rb.linearVelocity.z > 1e-5) || (velocity.z > 1e-5 && rb.linearVelocity.z < 1e-5))
            {
                velocity.z = 0;
            }

            // Apply the updated velocity back to the Rigidbody
            rb.linearVelocity = velocity;
        }
    }

    private void UpdateVelocity()
    {
        Vector3 dir = Vector3.zero;

        // Update movement direction based on input (forward, backward, strafing).
        dir += transform.forward * movementInput.y * accSpeed * Time.fixedDeltaTime;
        dir += transform.right * movementInput.x * accSpeed * Time.fixedDeltaTime;

        // Get current velocity and apply direction changes.
        Vector3 velocity = rb.linearVelocity;
        velocity += dir;

        // Clamp velocity to max speeds.
        if (velocity.magnitude > maxSpeed)
        {
            velocity = velocity.normalized * maxSpeed;
        }

        // Apply updated velocity to the Rigidbody.
        rb.linearVelocity = velocity;
        Log($"Updated velocity: {velocity}");
    }

    public void MoveActionChanged(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    private void Log(string message)
    {

    }
}
