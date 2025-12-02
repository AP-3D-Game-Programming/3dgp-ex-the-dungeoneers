using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float jumpForce = 5f;

    private Rigidbody rb;
    private Animator animator;
    private Vector3 moveDirection;
    
    // Checken of we op de grond staan
    private bool isGrounded;
    private bool jumpRequest = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // Veiligheidscheck
        if (animator == null) Debug.LogWarning("Geen Animator gevonden op de speler!");
    }

    void Update()
    {
        // 1. Input ophalen (Input System & Keyboard fallback)
        Vector2 inputVec = Vector2.zero;

        if (Gamepad.current != null)
        {
            inputVec = Gamepad.current.leftStick.ReadValue();
        }
        else if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) inputVec.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) inputVec.x += 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) inputVec.y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) inputVec.y -= 1f;
            inputVec = inputVec.normalized;
        }

        moveDirection = new Vector3(inputVec.x, 0f, inputVec.y);

        // 2. Rotatie (Draaien naar looprichting)
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 3. Spring Input Check
        bool jumpPressed = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
                           (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);

        if (jumpPressed && isGrounded)
        {
            jumpRequest = true;
        }

        // 4. ANIMATIE: Snelheid doorgeven
        // Dit zorgt dat hij wisselt tussen Idle en Run
        if (animator != null) 
        {
            animator.SetFloat("Speed", moveDirection.magnitude);
        }
    }

    void FixedUpdate()
    {
        // Fysieke beweging
        Vector3 move = moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        // Springen uitvoeren
        if (jumpRequest)
        {
            // OPMERKING: Gebruik rb.velocity voor Unity 2022/2023. 
            // Gebruik rb.linearVelocity alleen als je Unity 6 gebruikt.
            Vector3 vel = rb.linearVelocity; 
            vel.y = jumpForce;
            rb.linearVelocity = vel;

            isGrounded = false;
            jumpRequest = false;

            // ANIMATIE: Springen starten
            if (animator != null)
            {
                animator.SetTrigger("Jump");
                animator.SetBool("IsGrounded", false);
            }
        }
    }

    // --- Botsing Detectie (Voor Ground Check) ---

    void OnCollisionEnter(Collision collision)
    {
        // BELANGRIJK: Zorg dat je vloer de Tag "Ground" heeft!
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            if (animator != null) animator.SetBool("IsGrounded", true);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            if (animator != null) animator.SetBool("IsGrounded", true);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
            if (animator != null) animator.SetBool("IsGrounded", false);
        }
    }
}