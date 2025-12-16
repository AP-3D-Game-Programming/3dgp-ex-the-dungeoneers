using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public float aggroRange = 10f;
    public float attackRange = 2f;
    public LayerMask playerLayer;

    private NavMeshAgent agent;
    private Transform target;
    private Animator animator;
    private EnemyCombat combat;
    private Health health;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        combat = GetComponent<EnemyCombat>();
        health = GetComponent<Health>();
    }

    void Update()
    {
        if (!agent.isOnNavMesh) return;
        if (health != null && health.currentHealth <= 0) return;

        if (target == null)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, aggroRange, playerLayer);
            if (hits.Length > 0)
                target = hits[0].transform;
            else
                return;
        }

        float dist = Vector3.Distance(transform.position, target.position);

        if (dist > attackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(target.position);
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
        else
        {
            agent.isStopped = true;
            animator.SetFloat("Speed", 0);
            combat.TryAttack(target);
        }
    }
}
