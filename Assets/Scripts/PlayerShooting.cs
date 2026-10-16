
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    public Camera playerCamera;
    public Transform firePoint;
    public GameObject projectilePrefab;

    public float fireRate = 0.3f;
    public float aimDistance = 100f;

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
            return;

        if (Mouse.current.leftButton.isPressed &&
            Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        Vector3 targetPoint =
            ray.GetPoint(aimDistance);

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            aimDistance
        );

        float closestDistance = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            // Ignore the player's own colliders.
            if (hit.collider.transform.IsChildOf(transform))
                continue;

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                targetPoint = hit.point;
            }
        }

        Vector3 shootDirection =
            (targetPoint - firePoint.position).normalized;

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
}
