using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;


public class PlayerMovement : MonoBehaviour
{
   
    public float moveSpeed = 5f;        
    public float rotationSpeed = 10f;   
    public float jumpForce = 5f;        
    

    private Rigidbody rb;
    private Animator animator;
    private Vector3 moveDirection;
    private bool isGrounded;
    private bool jumpRequest = false;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"Animator component not found on '{gameObject.name}'. Animator calls will be skipped.");
        }
    }

    void Update()
    {
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

        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        bool jumpPressed = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
                           (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);

        if (jumpPressed && isGrounded)
        {
            jumpRequest = true;
            Debug.Log("Jump pressed, queuing jump");
        }

        if (animator != null) animator.SetFloat("Speed", moveDirection.magnitude);

    }

    void FixedUpdate()
    {
        Vector3 move = moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        if (jumpRequest)
        {
            if (!rb.isKinematic && (rb.constraints & RigidbodyConstraints.FreezePositionY) == 0)
            {
                // Apply jump as an instantaneous vertical velocity for reliability
                Vector3 vel = rb.linearVelocity;
                vel.y = jumpForce;
                rb.linearVelocity = vel;
                isGrounded = false;
                if (animator != null)
                {
                    animator.SetTrigger("Jump");
                    animator.SetBool("IsGrounded", isGrounded);
                }
                Debug.Log($"Jump applied. new velocity: {rb.linearVelocity}");
            }
            else
            {
                Debug.LogWarning("Cannot apply jump: Rigidbody is kinematic or Y position is frozen.");
            }
            jumpRequest = false;
        }
    }

    // Dash removed for simplified movement
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Raakt: " + collision.gameObject.name);

        if (collision.gameObject.tag == "Ground")
        {
            Debug.Log("Grond geraakt!");
            isGrounded = true;
            if (animator != null) animator.SetBool("IsGrounded", true);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            isGrounded = true;
            if (animator != null) animator.SetBool("IsGrounded", true);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            isGrounded = false;
            if (animator != null) animator.SetBool("IsGrounded", false);
        }
    }
}
