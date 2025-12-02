using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    public int currentHealth;
    public Animator animator; // optioneel: voor hit / death anim
    public bool destroyOnDeath = false;

    void Awake()
    {
        currentHealth = maxHealth;
        if (animator == null) animator = GetComponent<Animator>();
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;
        currentHealth -= amount;

        if (animator != null)
        {
            animator.SetTrigger("Hit"); // maak een "Hit" trigger in je animator als je wilt
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (animator != null)
        {
            animator.SetTrigger("Die");
            // je kunt object na animatie verwijderen via animation event of coroutine
        }
        if (destroyOnDeath)
        {
            Destroy(gameObject, 2f);
        }
        // disable components zodat geen verdere interactie
        var col = GetComponent<Collider>();
        if (col) col.enabled = false;
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent) agent.enabled = false;
    }
}
