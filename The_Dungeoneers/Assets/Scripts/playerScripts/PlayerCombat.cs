using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public int attackDamage = 25;
    public float attackRange = 1.5f; // melee range
    public float attackCooldown = 0.6f;
    public LayerMask enemyLayer; // zet layer(s) van vijanden
    public Animator animator;

    float lastAttackTime = -999f;

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryAttack();
        }
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;
        animator.SetTrigger("Attack");
        // damage wordt op het juiste animatieframe toegepast via Animation Event -> OnAttackHit()
    }

    // Roep deze functie vanuit een Animation Event op het moment van impact in de attack animatie
    public void OnAttackHit()
    {
        // vind alle vijanden in range
        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * (attackRange * 0.5f), attackRange, enemyLayer);
        foreach (var hit in hits)
        {
            IDamageable dmg = hit.GetComponent<IDamageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(attackDamage);
            }
            else
            {
                // fallback: probeer Health component
                var health = hit.GetComponent<Health>();
                if (health != null) health.TakeDamage(attackDamage);
            }
        }
    }

    // debug gizmo
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * (attackRange * 0.5f), attackRange);
    }
}
