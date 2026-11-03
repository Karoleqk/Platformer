using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Coyote Time & Jump Buffer")]
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.15f;

    // Components
    private Rigidbody2D rb;
    private Animator anim;
    private PlayerControls playerControls;

    // Input
    private float moveInput;

    // State
    private bool isFacingRight = true;
    private float coyoteCounter;
    private float jumpBufferCounter;

    // Public state used by other scripts
    public bool IsGrounded  { get; private set; }
    public bool IsMoving    => Mathf.Abs(moveInput) > 0.01f;
    public bool CanBeControlled { get; set; } = true;

    private void Awake()
    {
        rb             = GetComponent<Rigidbody2D>();
        anim           = GetComponent<Animator>();
        playerControls = new PlayerControls();
    }

    private void OnEnable()  => playerControls.Enable();
    private void OnDisable() => playerControls.Disable();

    // ─── Update ───────────────────────────────────────────────────────────────

    private void Update()
    {
        // Ground check runs always so IsGrounded is always accurate
        CheckGround();

        if (!CanBeControlled)
        {
            // Stop horizontal movement when control is disabled (e.g. dead)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        ReadInput();
        TickTimers();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (!CanBeControlled) return;

        MoveHorizontal();
        TryJump();
    }

    // ─── Input ────────────────────────────────────────────────────────────────

    private void ReadInput()
    {
        moveInput = playerControls.Movement.Move.ReadValue<float>();

        // Buffer jump input
        if (playerControls.Movement.Jump.WasPressedThisFrame())
            jumpBufferCounter = jumpBufferTime;
    }

    // ─── Timers ───────────────────────────────────────────────────────────────

    private void TickTimers()
    {
        // Coyote: stay "grounded" briefly after walking off a ledge
        coyoteCounter = IsGrounded ? coyoteTime : coyoteCounter - Time.deltaTime;

        // Jump buffer: allow jumping slightly before landing
        if (jumpBufferCounter > 0f)
            jumpBufferCounter -= Time.deltaTime;
    }

    // ─── Movement ────────────────────────────────────────────────────────────

    private void MoveHorizontal()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        UpdateFlip();
    }

    private void TryJump()
    {
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteCounter     = 0f;
        }
    }

    private void UpdateFlip()
    {
        if (moveInput > 0f && !isFacingRight) Flip();
        else if (moveInput < 0f && isFacingRight) Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    // ─── Ground Check ─────────────────────────────────────────────────────────

    private void CheckGround()
    {
        Vector2 origin = groundCheck != null
            ? (Vector2)groundCheck.position
            : new Vector2(transform.position.x, GetComponent<Collider2D>()?.bounds.min.y ?? transform.position.y);

        IsGrounded = Physics2D.OverlapCircle(origin, groundCheckRadius, groundLayer);
    }

    // ─── Animation ───────────────────────────────────────────────────────────

    private void UpdateAnimations()
    {
        if (anim == null) return;

        anim.SetBool("isMoving",   IsMoving);
        anim.SetBool("isGrounded", IsGrounded);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    // ─── Public API ──────────────────────────────────────────────────────────

    /// <summary>Push the player in a direction (e.g. knockback from hit).</summary>
    public void Knockback(Vector2 direction, float force)
    {
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
    }

    public PlayerControls GetPlayerControls() => playerControls;
    public int GetFacingDirection() => isFacingRight ? 1 : -1;

    // ─── Gizmos ──────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
