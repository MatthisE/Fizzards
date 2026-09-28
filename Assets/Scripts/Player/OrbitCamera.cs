using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class OrbitCamera : NetworkBehaviour
{
    public Transform target;
    public float distance = 10f;

    public float maxRotationSpeed = 30f;
    public float sensitivity = 0.2f;
    public float deadZone = 0.6f;

    private float yaw;
    private float pitch;
    private bool wasRotating = false;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            // Disable camera and audio listener for remote players
            GetComponent<Camera>().enabled = false;

            AudioListener listener = GetComponent<AudioListener>();
            if (listener) listener.enabled = false;

            return;
        }

        // Owner camera initialization
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

   void LateUpdate()
    {
        if (!IsOwner) return;
        if (!target) return;

        bool rotating = Mouse.current.rightButton.isPressed;

        Vector2 delta = Vector2.zero;

        if (rotating)
        {
            // Read delta only while rotating
            delta = Mouse.current.delta.ReadValue();

            // Clamp spikes (Unity sometimes sends huge values)
            delta = Vector2.ClampMagnitude(delta, 20f);

            // --- EDGE SCROLL ADDITION ---
            Vector2 mouse = Mouse.current.position.ReadValue();
            Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Vector2 offset = (mouse - center) / center;

            float edgeX = Mathf.Abs(offset.x) > deadZone ? offset.x : 0f;
            float edgeY = Mathf.Abs(offset.y) > deadZone ? offset.y : 0f;

            edgeX = Mathf.Sign(edgeX) * Mathf.Pow(Mathf.Abs(edgeX), 3f);
            edgeY = Mathf.Sign(edgeY) * Mathf.Pow(Mathf.Abs(edgeY), 3f);

            // Add edge movement to delta
            delta += new Vector2(edgeX * 50f, edgeY * 50f);

            // --- NEW: MAX ROTATION SPEED ---
            delta = Vector2.ClampMagnitude(delta, maxRotationSpeed);
        }
        else if (wasRotating)
        {
            // Right-click was just released → clear leftover movement
            delta = Vector2.zero;
        }

        wasRotating = rotating;

        yaw += delta.x * sensitivity;
        pitch += -delta.y * sensitivity;

        pitch = Mathf.Clamp(pitch, -10f, 35f);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 position = target.position - rotation * Vector3.forward * distance;

        float minHeight = target.position.y + 0.5f;
        if (position.y < minHeight)
            position.y = minHeight;

        transform.SetPositionAndRotation(position, rotation);
    }


}
