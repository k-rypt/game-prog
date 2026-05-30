using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 12f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Read arrow keys / WASD via legacy input
        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        if (animator == null) return;

        if (moveInput != Vector2.zero)
        {
            animator.SetBool("isWalking", true);
            animator.SetFloat("InputX", moveInput.x);
            animator.SetFloat("InputY", moveInput.y);
        }
        else
        {
            animator.SetBool("isWalking", false);
            animator.SetFloat("InputX", 0f);
            animator.SetFloat("InputY", 0f);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * speed;
    }

    public void SetMovementEnabled(bool enabled)
    {
        this.enabled = enabled;

        if (!enabled)
        {
            moveInput = Vector2.zero;
            rb.linearVelocity = Vector2.zero;

            if (animator != null)
            {
                animator.SetBool("isWalking", false);
                animator.SetFloat("InputX", 0f);
                animator.SetFloat("InputY", 0f);
            }
        }
    }

    // Kept for compatibility if you add a PlayerInput component later
    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}
