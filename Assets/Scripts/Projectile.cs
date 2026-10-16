
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public float lifetime = 3f;
    public float damage = 25f;

    private Vector3 direction;
    private int ownerTeamID;
    private Transform owner;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Initialize(
        Vector3 shootDirection,
        int teamID,
        Transform shooter)
    {
        direction = shootDirection.normalized;
        ownerTeamID = teamID;
        owner = shooter;
    }

    void Update()
    {
        Vector3 movement =
            direction * speed * Time.deltaTime;

        RaycastHit[] hits = Physics.RaycastAll(
            transform.position,
            direction,
            movement.magnitude
        );

        RaycastHit? closestHit = null;
        float closestDistance = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            if (owner != null &&
                hit.collider.transform.IsChildOf(owner))
                continue;

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                closestHit = hit;
            }
        }

        if (closestHit.HasValue)
        {
            Collider targetCollider =
                closestHit.Value.collider;

            TeamMember targetTeam =
                targetCollider.GetComponentInParent<TeamMember>();

            Health targetHealth =
                targetCollider.GetComponentInParent<Health>();

            bool isTeammate =
                targetTeam != null &&
                targetTeam.teamID == ownerTeamID;

            if (!isTeammate && targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
            }

            Destroy(gameObject);
            return;
        }

        transform.position += movement;
    }
}
