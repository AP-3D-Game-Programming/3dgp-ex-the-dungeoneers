using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public int damage = 15;
    public float attackCooldown = 1.5f; // Dit voorkomt dat je in 1 seconde doodgaat
    public float attackRange = 1.5f;
    public float hitRadius = 1.0f;
    public LayerMask playerLayer;

    [Header("Boss Settings")]
    public bool isBoss = false;
    public float bossDamageMultiplier = 1.5f;

    private Animator animator;

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        // We passen de stats aan als dit een Boss is
        if (isBoss)
        {
            damage = Mathf.RoundToInt(damage * bossDamageMultiplier);
            attackRange *= 1.3f;
            hitRadius *= 1.5f;
        }
    }
    public void OnAttackHit()
    {
        // Laat dit leeg
    }

    // Deze methode wordt nu aangeroepen door de timer in EnemyAI.cs
    public void PerformAttackDamage()
    {
        // Debug om te zien in de console of de aanval start
        Debug.Log(gameObject.name + " voert aanval uit!");

        // Check voor de speler in de hit-zone
        Collider[] hitPlayers = Physics.OverlapSphere(transform.position + transform.forward * attackRange, hitRadius, playerLayer);

        foreach (Collider player in hitPlayers)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log("Speler geraakt door enemy! Schade: " + damage);
            }
        }
    }

    // Teken de hit-zone in de Scene view voor hulp bij instellen
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * attackRange, hitRadius);
    }
}