using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float groundCheckDistance = 0.15f;

    public Transform groundCheck;

    private Rigidbody2D rb;
    private PlayerControls playerControls;

    private float movement;
    private bool jumpRequested = false;
    private bool isGrounded = false;

    private void Awake()
    {
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    void Update()
    {
        handlePlayerInput();
    }

    private void handlePlayerInput()
    {
        movement = playerControls.Movement.Move.ReadValue<float>();

        if (playerControls.Movement.Jump.WasPressedThisFrame() && isGrounded) jumpRequested = true;
    }

    private void FixedUpdate()
    {
        HandleHorizontalMovement();
        HandleVerticalMovement();
    }

    private void HandleHorizontalMovement()
    {
        rb.linearVelocity = new Vector2(movement * moveSpeed, rb.linearVelocity.y);

        if (movement < 0) transform.rotation = Quaternion.Euler(0, 180, 0);
        else if (movement > 0) transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    private void HandleVerticalMovement()
    {
        if (IsAbleToJump()) Jump();
    }

    private bool IsAbleToJump()
    {
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(groundCheck.position.x, groundCheck.position.y), Vector2.down, groundCheckDistance);
        isGrounded = hit.collider != null;

        return hit.collider && jumpRequested;
    }

    private void Jump()
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpRequested = false;
    }
}
