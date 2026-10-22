
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Distance")]
    public float distance = 5f;

    [Header("Shoulder Camera")]
    public float shoulderOffset = 1.4f;
    public float heightOffset = 1.5f;

    [Header("Mouse Control")]
    public float mouseSensitivity = 0.15f;

    [Header("Vertical Rotation Limits")]
    public float minPitch = -20f;
    public float maxPitch = 60f;

    [Header("Camera Collision")]
    public bool enableCameraCollision = true;
    public LayerMask collisionLayers = ~0;
    public float collisionRadius = 0.2f;
    public float collisionPadding = 0.15f;

    private float yaw = 0f;
    private float pitch = 20f;

    void LateUpdate()
    {
        if (target == null || Mouse.current == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        yaw += mouseDelta.x * mouseSensitivity;
        pitch -= mouseDelta.y * mouseSensitivity;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(
            pitch,
            yaw,
            0f
        );

        // The point the camera aims around.
        Vector3 pivotPosition =
            target.position + Vector3.up * heightOffset;

        // Move the camera slightly to the player's right.
        Vector3 shoulder =
            rotation * Vector3.right * shoulderOffset;

        Vector3 backwards =
            rotation * Vector3.back * distance;

        Vector3 desiredPosition =
            pivotPosition + shoulder + backwards;

        // Optional camera collision.
        if (enableCameraCollision)
        {
            Vector3 direction =
                desiredPosition - pivotPosition;

            float desiredDistance = direction.magnitude;

            if (desiredDistance > 0.001f)
            {
                RaycastHit[] hits = Physics.SphereCastAll(
                    pivotPosition,
                    collisionRadius,
                    direction.normalized,
                    desiredDistance,
                    collisionLayers,
                    QueryTriggerInteraction.Ignore
                );

                float nearestDistance = desiredDistance;

                foreach (RaycastHit hit in hits)
                {
                    // Ignore the player and child colliders.
                    if (hit.collider.transform == target ||
                        hit.collider.transform.IsChildOf(target))
                        continue;

                    if (hit.distance < nearestDistance)
                        nearestDistance = hit.distance;
                }

                if (nearestDistance < desiredDistance)
                {
                    float safeDistance = Mathf.Max(
                        0f,
                        nearestDistance - collisionPadding
                    );

                    desiredPosition =
                        pivotPosition +
                        direction.normalized * safeDistance;
                }
            }
        }

        transform.position = desiredPosition;

        // Use camera rotation rather than LookAt(target).
        // This keeps the player away from screen center.
        transform.rotation = rotation;
    }
}
