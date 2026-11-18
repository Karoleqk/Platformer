using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float groundCheckDistance = 0.15f;

    [Header("Hurt & Knockback")]
    [Tooltip("Force with which the player is pushed back when taking damage")]
    [SerializeField] private float hurtKnockbackForce = 7f;
    [Tooltip("Duration in seconds that movement input is locked while being knocked back")]
    [SerializeField] private float hurtStunDuration = 0.25f;

    public Transform groundCheck;

    private Rigidbody2D rb;
    private Health health;
    private PlayerControls playerControls;
    public PlayerControls Controls => playerControls;

    private float movement;
    private bool jumpRequested = false;
    private bool isGrounded = false;
    private bool isHurtStunned = false;

    public bool IsHurtStunned => isHurtStunned;

    private void Awake()
    {
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
    }

    private void Start()
    {
        if (health != null)
        {
            health.OnDamaged.AddListener(OnTakeDamage);
        }
    }

    private void OnTakeDamage()
    {
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.SetTrigger("Hurt");
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
        if (isHurtStunned)
        {
            movement = 0;
            return;
        }

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
        // During knockback stun, do NOT overwrite velocity so the impulse pushes the player freely
        if (isHurtStunned) return;

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
        if (isHurtStunned) return false;

        RaycastHit2D hit = Physics2D.Raycast(new Vector2(groundCheck.position.x, groundCheck.position.y), Vector2.down, groundCheckDistance);
        isGrounded = hit.collider != null;

        return hit.collider && jumpRequested;
    }

    private void Jump()
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpRequested = false;
    }

    /// <summary>
    /// Applies directional knockback to the player away from the damage source.
    /// Temporarily locks movement input so the impulse isn't cancelled.
    /// </summary>
    public void ApplyKnockback(Vector2 damageSourcePosition, float customForce = -1f)
    {
        float force = customForce > 0 ? customForce : hurtKnockbackForce;

        // Determine horizontal direction away from the source
        float dirX = Mathf.Sign(transform.position.x - damageSourcePosition.x);
        if (Mathf.Abs(transform.position.x - damageSourcePosition.x) < 0.05f)
            dirX = -transform.right.x; // Fallback: knock backwards relative to facing direction

        // Arc knockback (horizontal + slight upward lift)
        Vector2 knockbackDir = new Vector2(dirX, 0.45f).normalized;

        StopCoroutine(nameof(HurtStunCoroutine));
        StartCoroutine(HurtStunCoroutine(knockbackDir, force));
    }

    private IEnumerator HurtStunCoroutine(Vector2 direction, float force)
    {
        isHurtStunned = true;

        // Reset velocity before impulse for clean, consistent bounce
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y * 0.2f);
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(hurtStunDuration);

        isHurtStunned = false;
    }
}
