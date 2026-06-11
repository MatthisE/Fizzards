using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    private Vector2 moveInput;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpHoldForce = 10f;
    [SerializeField] private float jumpHoldTime = 500f;

    // Makes the whole jump faster without changing height
    [SerializeField] private float jumpSpeedMultiplier = 3f;

    [Header("Camera")]
    public Transform cameraTransform;

    private bool jumpHeld;
    private float jumpTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        bool pressed = value.Get<float>() > 0.5f;

        if (pressed)
        {
            if (IsGrounded())
            {
                jumpTimer = 0f;

                // Faster ascent, same height
                float scaledJumpForce = jumpForce * jumpSpeedMultiplier;
                rb.AddForce(Vector3.up * scaledJumpForce, ForceMode.Impulse);
            }

            jumpHeld = true;
        }
        else
        {
            jumpHeld = false;
        }
    }

    private void FixedUpdate()
    {
        Move();
        HandleJumpHold();
        ApplyScaledGravity();
    }

    private void Move()
    {
        // Camera-relative movement
        Vector3 camForward = cameraTransform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = cameraTransform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 moveDir = camForward * moveInput.y + camRight * moveInput.x;

        // Preserve vertical velocity
        float yVel = rb.linearVelocity.y;

        // Apply horizontal movement
        Vector3 vel = moveDir * moveSpeed;
        vel.y = yVel;

        rb.linearVelocity = vel;
    }

    private void HandleJumpHold()
    {
        if (!jumpHeld || jumpTimer >= jumpHoldTime)
            return;

        rb.AddForce(Vector3.up * jumpHoldForce, ForceMode.Acceleration);
        jumpTimer += Time.fixedDeltaTime;
    }

    // Faster jump, same height: scale gravity too
    private void ApplyScaledGravity()
    {
        if (!IsGrounded())
        {
            rb.AddForce(Physics.gravity * (jumpSpeedMultiplier - 1f), ForceMode.Acceleration);
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.05f);
    }

    private void LateUpdate()
    {
        // Rotate player to face camera direction
        Vector3 camForward = cameraTransform.forward;
        camForward.y = 0f;

        if (camForward.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(camForward);
    }
}
