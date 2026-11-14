
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
        if (target == null || firePoint == null)
            return false;

        // Prevent firing when the muzzle overlaps a wall.
        Collider[] nearby = Physics.OverlapSphere(
            firePoint.position,
            0.15f,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider collider in nearby)
        {
            Transform other = collider.transform;

            // Ignore the bot's own colliders.
            if (other == transform ||
                other.IsChildOf(transform))
                continue;

            // Ignore the target's colliders.
            if (other == target ||
                other.IsChildOf(target))
                continue;

            return false;
        }

        Collider targetCollider =
            target.GetComponentInChildren<Collider>();

        Vector3 targetPoint = targetCollider != null
            ? targetCollider.bounds.center
            : target.position;

        // Check visibility from the bot's body.
        Vector3 bodyEye =
            transform.position + Vector3.up * 1.2f;

        // Both the body and the muzzle must have
        // an unobstructed line to the target.
        return HasClearLine(bodyEye, targetPoint) &&
               HasClearLine(firePoint.position, targetPoint);
    }

    private bool HasClearLine(
        Vector3 origin,
        Vector3 destination)
    {
        Vector3 direction = destination - origin;
        float distance = direction.magnitude;

        if (distance < 0.001f)
            return false;

        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            direction.normalized,
            distance + 0.1f,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        float nearestDistance = float.MaxValue;
        Collider nearestCollider = null;

        foreach (RaycastHit hit in hits)
        {
            Transform hitTransform = hit.collider.transform;

            // Ignore the shooting bot.
            if (hitTransform == transform ||
                hitTransform.IsChildOf(transform))
                continue;

            if (hit.distance < nearestDistance)
            {
                nearestDistance = hit.distance;
                nearestCollider = hit.collider;
            }
        }

        if (nearestCollider == null)
            return false;

        Transform closest =
            nearestCollider.transform;

        return closest == target ||
               closest.IsChildOf(target);
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
