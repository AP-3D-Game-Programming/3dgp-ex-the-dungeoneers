using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public int attackDamage = 25;
    public float attackCooldown = 0.6f;
    public float hitRadius = 0.35f;

    [Header("Weapon")]
    [SerializeField] private Transform weaponHitPoint;

    [Header("Optional")]
    [SerializeField] private PlayerPickup playerPickup;

    private Animator animator;
    private float lastAttackTime;

    void Awake()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
            Debug.LogError("PlayerCombat: Animator niet gevonden!");

        if (weaponHitPoint == null)
            Debug.LogError("PlayerCombat: Weapon HitPoint niet ingesteld!");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Gooien heeft prioriteit
            if (playerPickup != null && playerPickup.isCarrying)
                return;

            TryAttack();
        }
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        lastAttackTime = Time.time;
        animator.SetTrigger("Attack");
    }

    // WORDT AANGEROEPEN DOOR PLAYER ATTACK ANIMATIE
    public void OnAttackHit()
    {
        if (weaponHitPoint == null) return;

        Collider[] hits = Physics.OverlapSphere(
            weaponHitPoint.position,
            hitRadius
        );

        foreach (Collider c in hits)
        {
            // Nooit jezelf raken
            if (c.transform.root == transform)
                continue;

            IDamageable dmg = c.GetComponentInParent<IDamageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(attackDamage);
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (weaponHitPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(weaponHitPoint.position, hitRadius);
    }
#endif
}