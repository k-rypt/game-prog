using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 12f;
    private Rigidbody2D rb;
    private Vector2 input;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        input.Normalize();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = input * speed;
    }

    public void SetMovementEnabled(bool state) 
    {
        Debug.Log($"[PlayerMovement] SetMovementEnabled({state}) called from:\n{System.Environment.StackTrace}");
        enabled = state;
        if (!state) rb.linearVelocity = Vector2.zero;
    }
}
