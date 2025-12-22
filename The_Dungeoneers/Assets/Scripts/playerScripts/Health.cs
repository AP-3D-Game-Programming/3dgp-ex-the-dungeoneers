using UnityEngine;
using UnityEngine.AI;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Death Settings")]
    public bool destroyOnDeath = false;
    public float destroyDelay = 2f;

    [Header("References")]
    public Animator animator;

    private bool isDead = false;
    public ItemData lootDrop;

    void Awake()
    {
        currentHealth = maxHealth;

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void TakeDamage(int amount)
    {
        // Geen damage meer als hij dood is
        if (isDead) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Alleen hit animatie als hij nog leeft
            if (animator != null)
                animator.SetTrigger("Hit");
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Death animatie
        if (animator != null)
            animator.SetTrigger("Die");

        // Collider uitzetten
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        // NavMeshAgent uitzetten
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.enabled = false;

        // Optioneel destroy
        if (destroyOnDeath)
            Destroy(gameObject, destroyDelay);
        
        if (lootDrop != null)
        {
            bool succes = Inventory.instance.Add(lootDrop);
        
            if (succes)
            {
                Debug.Log("Enemy dropt " + lootDrop.name + " in je tas!");
            }
            else
            {
                Debug.Log("Enemy had loot, maar je tas zit vol!");
                // Optioneel: Spawn het item dan alsnog op de grond (komen we later op)
            }
        }

        Destroy(gameObject);
    }
}