
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public Transform firePoint;
    public GameObject projectilePrefab;

    public float detectionRange = 20f;
    public float attackRange = 8f;
    public float stoppingDistance = 6f;
    public float fireInterval = 1.2f;
    public float rotationSpeed = 8f;

    private NavMeshAgent agent;
    private TeamMember teamMember;
    private Transform target;

    private float nextFireTime;
    private float nextTargetSearchTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        teamMember = GetComponent<TeamMember>();

        agent.stoppingDistance = stoppingDistance;
    }

    void Update()
    {
        if (teamMember == null ||
            firePoint == null ||
            projectilePrefab == null ||
            !agent.isOnNavMesh)
            return;

        // Search for enemies every 0.4 seconds.
        if (Time.time >= nextTargetSearchTime)
        {
            FindClosestEnemy();
            nextTargetSearchTime = Time.time + 0.4f;
        }

        if (target == null)
        {
            agent.ResetPath();
            agent.updateRotation = true;
            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            target.position
        );

        if (distance > detectionRange)
        {
            agent.ResetPath();
            agent.updateRotation = true;
            return;
        }

        agent.SetDestination(target.position);

        if (distance <= attackRange)
        {
            agent.updateRotation = false;
            FaceTarget();

            if (Time.time >= nextFireTime && CanSeeTarget())
            {
                Shoot();
                nextFireTime = Time.time + fireInterval;
            }
        }
        else
        {
            agent.updateRotation = true;
        }
    }

    private void FindClosestEnemy()
    {
        TeamMember[] characters =
            FindObjectsByType<TeamMember>(
                FindObjectsSortMode.None
            );

        float closestDistance = detectionRange;
        target = null;

        foreach (TeamMember character in characters)
        {
            if (character == teamMember)
                continue;

            if (character.teamID == teamMember.teamID)
                continue;

            if (character.GetComponent<Health>() == null)
                continue;

            // Only target the player or another AI bot.
            bool isPlayer =
                character.GetComponent<CharacterController>() != null;

            bool isBot =
                character.GetComponent<NavMeshAgent>() != null;

            if (!isPlayer && !isBot)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                character.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                target = character.transform;
            }
        }
    }

    private void FaceTarget()
    {
        Vector3 direction =
            target.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private bool CanSeeTarget()
    {
        Vector3 direction =
            target.position - firePoint.position;

        if (Physics.Raycast(
            firePoint.position,
            direction.normalized,
            out RaycastHit hit,
            direction.magnitude + 0.5f))
        {
            return hit.collider.transform.IsChildOf(target);
        }

        return false;
    }

    private void Shoot()
    {
        Vector3 direction =
            (target.position - firePoint.position).normalized;

        GameObject bullet = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );

        Projectile projectile =
            bullet.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Initialize(
                direction,
                teamMember.teamID,
                transform
            );
        }
    }
}
