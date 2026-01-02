using UnityEngine;
using UnityEngine.EventSystems; // <-- Deze is nodig om de UI te checken!

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    // attackDamage halen we nu weg hier, want dat regelt PlayerEquipment!
    public float attackCooldown = 0.6f;
    public float hitRadius = 0.35f;

    [Header("Weapon")]
    [SerializeField] private Transform weaponHitPoint;

    [Header("Optional")]
    [SerializeField] private PlayerPickup playerPickup;

    [Header("Effects")]
    public GameObject floatingTextPrefab;

    private Animator animator;
    private float lastAttackTime;
    
    // NIEUW: Referentie naar je equipment script
    private PlayerEquipment equipment; 

    void Awake()
    {
        animator = GetComponent<Animator>();
        // NIEUW: We zoeken het equipment script op dezelfde speler
        equipment = GetComponent<PlayerEquipment>();

        if (animator == null) Debug.LogError("PlayerCombat: Animator niet gevonden!");
        if (weaponHitPoint == null) Debug.LogError("PlayerCombat: Weapon HitPoint niet ingesteld!");
    }

    void Update()
    {
        // 1. Check of we op een UI element klikken (Inventory, Pauze menu, etc.)
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return; // Stop direct! Ga niet verder met slaan.
        }

        // 2. Normale aanval logica
        if (Input.GetMouseButtonDown(0))
        {
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

    // WORDT AANGEROEPEN DOOR PLAYER ATTACK ANIMATIE EVENT
    public void OnAttackHit()
    {
        if (weaponHitPoint == null) return;

        Collider[] hits = Physics.OverlapSphere(weaponHitPoint.position, hitRadius);

        foreach (Collider c in hits)
        {
            if (c.transform.root == transform) continue;

            IDamageable dmg = c.GetComponentInParent<IDamageable>();
            if (dmg != null)
            {
                // NIEUW: Hier vragen we de damage op!
                int damageDealen = 5; // Standaard vuist damage

                if (equipment != null)
                {
                    // Haal de berekende damage op uit het andere script (Base + Wapen)
                    damageDealen = equipment.currentDamage;
                }

                dmg.TakeDamage(damageDealen);
                if (dmg != null)
            {
                // ... damage berekening ... (heb je al)
                
                dmg.TakeDamage(damageDealen); // (heb je al)

                // NIEUW: Toon zwevende tekst
                if (floatingTextPrefab != null)
                {
                    // Maak de tekst op de plek van de vijand (c.transform.position)
                    GameObject ft = Instantiate(floatingTextPrefab, c.transform.position, Quaternion.identity);
                    
                    // Stel de tekst en kleur in
                    ft.GetComponent<FloatingText>().SetText(damageDealen.ToString(), Color.yellow);
                }
            }
                Debug.Log("Hit! Dealt " + damageDealen + " damage.");
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