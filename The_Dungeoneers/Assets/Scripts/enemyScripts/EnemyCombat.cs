using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public int damage = 15;
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

        // De Boss doet meer schade en heeft een groter bereik
        if (isBoss)
        {
            damage = Mathf.RoundToInt(damage * bossDamageMultiplier);
            attackRange *= 1.3f;
            hitRadius *= 1.5f;
        }
    }

    // Deze methode wordt aangeroepen door je EnemyAI of een Animation Event
    public void PerformAttackDamage()
    {
        // Maak een onzichtbare cirkel voor de enemy om te checken of de speler geraakt wordt
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

    // Teken de hit-zone in de Scene view zodat je het kunt debuggen
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * attackRange, hitRadius);
    }
}