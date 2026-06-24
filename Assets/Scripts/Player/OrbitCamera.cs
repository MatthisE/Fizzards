using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class OrbitCamera : NetworkBehaviour
{
    public Transform target;
    public float distance = 10f;

    public float maxRotationSpeed = 80f;
    public float deadZone = 0.6f;

    private float yaw;
    private float pitch;

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

        Vector2 mouse = Mouse.current.position.ReadValue();
        Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 offset = (mouse - center) / center;

        float x = Mathf.Abs(offset.x) > deadZone ? offset.x : 0f;
        float y = Mathf.Abs(offset.y) > deadZone ? offset.y : 0f;

        x = Mathf.Sign(x) * Mathf.Pow(Mathf.Abs(x), 3f);
        y = Mathf.Sign(y) * Mathf.Pow(Mathf.Abs(y), 3f);

        yaw += x * maxRotationSpeed * Time.deltaTime;
        pitch += -y * maxRotationSpeed * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, -10f, 35f);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 position = target.position - rotation * Vector3.forward * distance;

        float minHeight = target.position.y + 0.5f;
        if (position.y < minHeight)
            position.y = minHeight;

        transform.SetPositionAndRotation(position, rotation);
    }
}
