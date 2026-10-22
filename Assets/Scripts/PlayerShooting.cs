
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Transform firePoint;
    public GameObject projectilePrefab;

    [Header("Shooting")]
    public float fireRate = 0.3f;
    public float aimDistance = 100f;

    [Header("Aiming")]
    public LayerMask aimLayers = ~(1 << 2);

    [Header("Temporary Crosshair")]
    public bool showCrosshair = true;
    public float crosshairSize = 8f;
    public float crosshairGap = 5f;
    public float crosshairThickness = 2f;
    public Color crosshairColor = Color.white;

    private float nextFireTime;
    private TeamMember teamMember;

    void Start()
    {
        teamMember = GetComponent<TeamMember>();
    }

    void Update()
    {
        if (Mouse.current == null ||
            playerCamera == null ||
            firePoint == null ||
            projectilePrefab == null ||
            teamMember == null)
        {
            return;
        }

        if (Mouse.current.leftButton.isPressed &&
            Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        // Ray starts at the center of the camera.
        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        Vector3 targetPoint = ray.GetPoint(aimDistance);

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            aimDistance,
            aimLayers,
            QueryTriggerInteraction.Ignore
        );

        float closestDistance = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            // Ignore the shooter's own colliders.
            if (hit.collider.transform == transform ||
                hit.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                targetPoint = hit.point;
            }
        }

        Vector3 toTarget = targetPoint - firePoint.position;

        if (toTarget.sqrMagnitude < 0.0001f)
            return;

        Vector3 shootDirection = toTarget.normalized;

        GameObject bullet = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.LookRotation(shootDirection)
        );

        Projectile projectile =
            bullet.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Initialize(
                shootDirection,
                teamMember.teamID,
                transform
            );
        }
    }

    private void OnGUI()
    {
        if (!showCrosshair ||
            playerCamera == null ||
            !playerCamera.isActiveAndEnabled)
        {
            return;
        }

        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.5f;

        float size = Mathf.Max(1f, crosshairSize);
        float gap = Mathf.Max(0f, crosshairGap);
        float thickness = Mathf.Max(1f, crosshairThickness);

        Color previousColor = GUI.color;
        GUI.color = crosshairColor;

        Texture2D pixel = Texture2D.whiteTexture;

        // Left line.
        GUI.DrawTexture(
            new Rect(
                centerX - gap - size,
                centerY - thickness * 0.5f,
                size,
                thickness
            ),
            pixel
        );

        // Right line.
        GUI.DrawTexture(
            new Rect(
                centerX + gap,
                centerY - thickness * 0.5f,
                size,
                thickness
            ),
            pixel
        );

        // Upper line.
        GUI.DrawTexture(
            new Rect(
                centerX - thickness * 0.5f,
                centerY - gap - size,
                thickness,
                size
            ),
            pixel
        );

        // Lower line.
        GUI.DrawTexture(
            new Rect(
                centerX - thickness * 0.5f,
                centerY + gap,
                thickness,
                size
            ),
            pixel
        );

        GUI.color = previousColor;
    }
}
