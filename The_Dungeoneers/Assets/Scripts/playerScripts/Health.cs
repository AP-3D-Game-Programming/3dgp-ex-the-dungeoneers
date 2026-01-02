using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI; // Verplicht voor Slider ondersteuning

public class Health : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Visuals")]
    public GameObject bloodEffectPrefab; // Gekoppeld in de prefab asset

    [Header("UI References")]
    public Slider healthSlider; // Wordt nu automatisch gezocht bij de speler

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
    public ItemData lootDrop;
    public GameObject lootPrefab;

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (isBoss)
        {
            maxHealth = Mathf.RoundToInt(maxHealth * bossHealthMultiplier);
        }

        currentHealth = maxHealth;
    }

    void Start()
    {
        // AUTOMATISCHE KOPPELING VOOR GESPAWNDE SPELER
        if (healthSlider == null)
        {
            // Zoekt in de scene naar de UI Slider op basis van de exacte naam
            GameObject uiObject = GameObject.Find("PlayerHealthBar");
            if (uiObject != null)
            {
                healthSlider = uiObject.GetComponent<Slider>();
            }
        }

        UpdateHealthUI();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        UpdateHealthUI();

        if (bloodEffectPrefab != null)
        {
            Instantiate(bloodEffectPrefab, transform.position + Vector3.up, Quaternion.identity);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (animator != null)
                animator.SetTrigger("Hit");
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (animator != null)
            animator.SetTrigger("Die");

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            // Veiligheidscheck voor NavMesh
            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
                agent.isStopped = true;
            agent.enabled = false;
        }

        if (lootDrop != null && lootPrefab != null)
        {
            GameObject droppedObject = Instantiate(lootPrefab, transform.position + Vector3.up, Quaternion.identity);
            ItemPickup pickupScript = droppedObject.GetComponent<ItemPickup>();
            if (pickupScript != null)
            {
                pickupScript.item = lootDrop;
            }
        }

        Destroy(gameObject, destroyDelay);
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        UpdateHealthUI();
        Debug.Log("Genezings-effect! Huidige HP: " + currentHealth);
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }
}