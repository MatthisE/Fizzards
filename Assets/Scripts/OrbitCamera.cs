using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 10f;

    public float maxRotationSpeed = 80f;
    public float deadZone = 0.6f;

    private float yaw;
    private float pitch;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void LateUpdate()
    {
        if (!target) return;

        Vector2 mouse = Mouse.current.position.ReadValue();
        Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 offset = (mouse - center) / center; // -1..1

        // Apply dead-zone
        float x = Mathf.Abs(offset.x) > deadZone ? offset.x : 0f;
        float y = Mathf.Abs(offset.y) > deadZone ? offset.y : 0f;

        // Ultra-smooth cubic falloff curve
        x = Mathf.Sign(x) * Mathf.Pow(Mathf.Abs(x), 3f);
        y = Mathf.Sign(y) * Mathf.Pow(Mathf.Abs(y), 3f);

        // Apply rotation
        yaw += x * maxRotationSpeed * Time.deltaTime;
        pitch += -y * maxRotationSpeed * Time.deltaTime;

        // Tighter upward clamp (prevents looking too far up)
        pitch = Mathf.Clamp(pitch, -10f, 35f);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 position = target.position - rotation * Vector3.forward * distance;

        // Prevent camera from going below ground
        float minHeight = target.position.y + 0.5f;
        if (position.y < minHeight)
            position.y = minHeight;

        transform.SetPositionAndRotation(position, rotation);
    }
}