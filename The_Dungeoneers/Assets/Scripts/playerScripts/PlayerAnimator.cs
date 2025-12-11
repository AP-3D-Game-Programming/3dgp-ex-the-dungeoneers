using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private PlayerMovement movement;
    private PlayerCombat combat;
    private Health health;
    private PlayerPickup pickup;
    private Rigidbody rb;

    void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
        health = GetComponent<Health>();
        pickup = GetComponent<PlayerPickup>();
        rb = GetComponent<Rigidbody>();

        // Animator koppelen aan andere scripts
        if (combat != null) combat.animator = animator;
        if (health != null) health.animator = animator;
    }

    void Update()
    {
        if (animator == null) return;

        // ---- SPEED ----
        float horizontalSpeed =
            new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
        animator.SetFloat("Speed", horizontalSpeed);

        // ---- GROUNDED ----
        if (movement != null)
            animator.SetBool("IsGrounded", movement.IsGrounded);

        // ---- JUMP ----
        if (movement != null && movement.JumpRequested)
            animator.SetTrigger("Jump");

        // ---- ATTACK ----
        if (Input.GetMouseButtonDown(0))
            animator.SetTrigger("Attack");

        // ---- PICKUP ----
        if (pickup != null && Input.GetKeyDown(pickup.pickupKey))
            animator.SetTrigger("Pickup");
    }
}
