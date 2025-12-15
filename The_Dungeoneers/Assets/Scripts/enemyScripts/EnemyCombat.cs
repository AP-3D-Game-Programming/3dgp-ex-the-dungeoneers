using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    public int damage = 15;
    public float cooldown = 1f;
    public float hitRadius = 1.5f;
    public LayerMask playerLayer;

    private float lastAttack;
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void TryAttack(Transform target)
    {
        if (Time.time - lastAttack < cooldown) return;
        lastAttack = Time.time;

        animator.SetTrigger("Attack");
    }

    // Animation Event
    public void OnAttackHit()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, hitRadius, playerLayer);
        foreach (Collider c in hits)
        {
            Health hp = c.GetComponent<Health>();
            if (hp != null)
                hp.TakeDamage(damage);
        }
    }
}
