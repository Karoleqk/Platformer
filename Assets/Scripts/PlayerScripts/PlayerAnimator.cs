using UnityEngine;

/// <summary>
/// Drives player animations based on movement, combat and health state.
/// Requires an Animator on this GameObject or a child with the following parameters:
///   Float  "Speed"       — absolute horizontal velocity
///   Bool   "IsGrounded"  — true when on the ground
///   Float  "VerticalVelocity" — y velocity (positive = going up)
///   Trigger "Attack"     — set by PlayerCombat
///   Trigger "Hurt"       — set by PlayerController.OnTakeDamage
///   Trigger "Death"      — set when HP reaches 0
///   Bool   "IsDead"      — stays true after death (prevents other transitions)
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private PlayerController controller;
    private Health health;

    // Cached parameter hashes for performance
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int HurtHash = Animator.StringToHash("Hurt");
    private static readonly int DeathHash = Animator.StringToHash("Death");
    private static readonly int IsDeadHash = Animator.StringToHash("IsDead");

    private bool isDead = false;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlayerController>();
        health = GetComponent<Health>();
    }

    private void Start()
    {
        if (health != null)
        {
            health.OnDamaged.AddListener(OnHurt);
            health.OnDied.AddListener(OnDied);
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnDamaged.RemoveListener(OnHurt);
            health.OnDied.RemoveListener(OnDied);
        }
    }

    private void Update()
    {
        if (animator == null || isDead) return;

        animator.SetFloat(SpeedHash, Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool(IsGroundedHash, IsGrounded());
        animator.SetFloat(VerticalVelocityHash, rb.linearVelocity.y);
    }

    /// <summary>
    /// Called by PlayerCombat or externally to trigger the attack animation.
    /// </summary>
    public void TriggerAttack()
    {
        if (animator != null && !isDead)
            animator.SetTrigger(AttackHash);
    }

    private void OnHurt()
    {
        if (animator != null && !isDead)
            animator.SetTrigger(HurtHash);
    }

    private void OnDied()
    {
        if (animator == null) return;

        isDead = true;
        animator.SetBool(IsDeadHash, true);
        animator.SetTrigger(DeathHash);
    }

    /// <summary>
    /// Called when the player respawns (by PersistentPlayer/GameManager).
    /// Resets the death state so animations work again.
    /// </summary>
    public void ResetAnimationState()
    {
        isDead = false;
        if (animator != null)
        {
            animator.SetBool(IsDeadHash, false);
            animator.Rebind();
            animator.Update(0f);
        }
    }

    private bool IsGrounded()
    {
        if (controller.groundCheck == null) return false;

        RaycastHit2D hit = Physics2D.Raycast(
            controller.groundCheck.position,
            Vector2.down,
            0.15f
        );
        return hit.collider != null;
    }
}
