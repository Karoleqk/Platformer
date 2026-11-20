using UnityEngine;

/// <summary>
/// Drives enemy animations based on EnemyAI state.
/// Works with any Animator Controller that has the following parameters:
///   Float   "Speed"      — absolute horizontal velocity
///   Trigger "Attack"     — when enemy starts an attack
///   Trigger "Hurt"       — when enemy takes damage
///   Trigger "Death"      — when enemy dies
///   Bool    "IsDead"     — stays true after death
///
/// For the existing PigAnimator controller that only has "isHit" (bool),
/// this component gracefully checks if each parameter exists before setting it.
/// </summary>
public class EnemyAnimator : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;

    // Cached parameter hashes
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int HurtHash = Animator.StringToHash("Hurt");
    private static readonly int DeathHash = Animator.StringToHash("Death");
    private static readonly int IsDeadHash = Animator.StringToHash("IsDead");
    private static readonly int IsHitHash = Animator.StringToHash("isHit");

    // Track which parameters actually exist in the Animator Controller
    private bool hasSpeed, hasAttack, hasHurt, hasDeath, hasIsDead, hasIsHit;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        CacheParameters();
    }

    private void CacheParameters()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            int hash = param.nameHash;
            if (hash == SpeedHash) hasSpeed = true;
            else if (hash == AttackHash) hasAttack = true;
            else if (hash == HurtHash) hasHurt = true;
            else if (hash == DeathHash) hasDeath = true;
            else if (hash == IsDeadHash) hasIsDead = true;
            else if (hash == IsHitHash) hasIsHit = true;
        }
    }

    /// <summary>
    /// Call every frame from EnemyAI.Update to sync movement speed.
    /// </summary>
    public void UpdateMovement()
    {
        if (animator == null) return;

        if (hasSpeed)
            animator.SetFloat(SpeedHash, Mathf.Abs(rb.linearVelocity.x));
    }

    public void TriggerAttack()
    {
        if (animator == null) return;

        if (hasAttack)
            animator.SetTrigger(AttackHash);
    }

    public void TriggerHurt()
    {
        if (animator == null) return;

        if (hasHurt)
            animator.SetTrigger(HurtHash);

        // Fallback for PigAnimator which uses "isHit" bool
        if (hasIsHit)
            animator.SetBool(IsHitHash, true);
    }

    /// <summary>
    /// Call when stun ends and enemy returns to normal.
    /// </summary>
    public void ClearHurt()
    {
        if (animator == null) return;

        if (hasIsHit)
            animator.SetBool(IsHitHash, false);
    }

    public void TriggerDeath()
    {
        if (animator == null) return;

        if (hasDeath)
            animator.SetTrigger(DeathHash);

        if (hasIsDead)
            animator.SetBool(IsDeadHash, true);
    }
}
