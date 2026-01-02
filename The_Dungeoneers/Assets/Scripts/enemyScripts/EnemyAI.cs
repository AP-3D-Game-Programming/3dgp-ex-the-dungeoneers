using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("AI Settings")]
    public float aggroRange = 10f;
    public float attackRange = 2f;
    public LayerMask playerLayer;

    [Header("Boss Settings")]
    public bool isBoss = false;
    public float bossScaleMultiplier = 2f;
    public float bossAggroMultiplier = 2f;

    private NavMeshAgent agent;
    private Transform target;
    private Animator animator;
    private EnemyCombat combat;
    private Health health;

    // Timer voor de aanval
    private float nextAttackTime = 0f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        combat = GetComponent<EnemyCombat>();
        health = GetComponent<Health>();

        if (isBoss)
        {
            transform.localScale *= bossScaleMultiplier;
            aggroRange *= bossAggroMultiplier;

            agent.speed *= 0.8f;
            agent.stoppingDistance = attackRange - 0.5f;
        }
    }

    void Update()
    {
        // 1. Check of de enemy nog leeft en op de navmesh staat
        if (health != null && health.currentHealth <= 0)
        {
            if (agent.enabled) agent.isStopped = true;
            return;
        }

        if (!agent.isOnNavMesh) return;

        // 2. Zoek doelwit als we dat nog niet hebben
        if (target == null)
        {
            FindTarget();
            return;
        }

        // 3. Bereken afstand naar doelwit
        float dist = Vector3.Distance(transform.position, target.position);

        // 4. Bepaal gedrag (Aanvallen of Volgen)
        if (dist <= attackRange)
        {
            AttackTarget();
        }
        else if (dist <= aggroRange)
        {
            FollowTarget();
        }
        else
        {
            StopFollowing();
        }

        // 5. Update Animator
        if (animator != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

    void FindTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, aggroRange, playerLayer);
        if (hits.Length > 0)
        {
            target = hits[0].transform;
            Debug.Log(gameObject.name + " heeft speler gezien!");
        }
    }

    void FollowTarget()
    {
        agent.isStopped = false;
        agent.SetDestination(target.position);
    }

    void AttackTarget()
    {
        agent.isStopped = true;

        // Zorg dat de enemy naar de speler kijkt tijdens aanval
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        // COOLDOWN CHECK: Alleen schade doen als de tijd voorbij is
        if (Time.time >= nextAttackTime)
        {
            if (combat != null)
            {
                combat.PerformAttackDamage();
                // Gebruik de cooldown uit het Combat script
                nextAttackTime = Time.time + combat.attackCooldown;
            }

            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
        }
    }

    void StopFollowing()
    {
        target = null;
        agent.isStopped = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}