using UnityEngine;
using UnityEngine.AI;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Visuals")]
    public GameObject bloodEffectPrefab; // Sleep hier je BloodSplatter_FX prefab naartoe

    [Header("Death Settings")]
    public bool destroyOnDeath = false;
    public float destroyDelay = 2f;

    [Header("Boss Settings")]
    public bool isBoss = false;
    public float bossHealthMultiplier = 5f;

    [Header("References")]
    public Animator animator;

    private bool isDead = false;

    [Header("Loot Settings")]
    public ItemData lootDrop;         // Het item (Data)
    public GameObject lootPrefab;     // De 3D Prefab (Die LootDrop cube die je net maakte)

    void Awake()
    {
        // 1. Eerst de animator zoeken
        if (animator == null)
            animator = GetComponent<Animator>();

        // 2. Check of dit een boss is en verhoog de max HP
        if (isBoss)
        {
            maxHealth = Mathf.RoundToInt(maxHealth * bossHealthMultiplier);
        }

        // 3. Nu pas de huidige HP gelijk zetten aan de (nieuwe) maxHealth
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        // Geen damage meer als hij dood is
        if (isDead) return;

        currentHealth -= amount;

        // --- BLOED EFFECT LOGICA ---
        if (bloodEffectPrefab != null)
        {
            // Spawn het effect iets boven de grond (bij de romp van het personage)
            Instantiate(bloodEffectPrefab, transform.position + Vector3.up, Quaternion.identity);
        }

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

        // Collider uitzetten zodat de speler erdoorheen kan lopen
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        // NavMeshAgent uitzetten zodat hij stopt met bewegen
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            if (agent.isOnNavMesh) agent.isStopped = true;
            agent.enabled = false;
        }

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

        // Vernietig het object na de ingestelde delay
        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
        else
        {
            // Als we niet destroyen, vernietigen we het alsnog om de scene op te ruimen (of laat de code staan zoals je wilt)
            Destroy(gameObject, destroyDelay);
        }
    }

    // Deze functie wordt aangeroepen door de Potion
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        Debug.Log("Genezings-effect! Huidige HP: " + currentHealth);
    }
}