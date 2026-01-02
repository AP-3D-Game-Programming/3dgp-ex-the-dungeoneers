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
    
    [Header("Loot Settings")]
    public ItemData lootDrop;         // Het item (Data)
    public GameObject lootPrefab;     // De 3D Prefab (Die LootDrop cube die je net maakte)

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
        
        // NIEUWE LOOT CODE:
        if (lootDrop != null && lootPrefab != null)
        {
            // 1. Maak het object in de wereld (op de positie van de enemy + klein beetje omhoog)
            GameObject droppedObject = Instantiate(lootPrefab, transform.position + Vector3.up, Quaternion.identity);

            // 2. Vertel het object welk item het is
            ItemPickup pickupScript = droppedObject.GetComponent<ItemPickup>();
            if (pickupScript != null)
            {
                pickupScript.item = lootDrop;
            }
        }

        Destroy(gameObject);
    }

    // Deze functie wordt aangeroepen door de Potion
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        Debug.Log("Speler genezen! Huidige HP: " + currentHealth);
    }
}