using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float     attackRange     = 0.65f;
    [SerializeField] private int       attackDamage    = 1;
    [SerializeField] private float     attackCooldown  = 0.45f;
    [SerializeField] private float     attackDuration  = 0.35f;  // fallback if no anim event
    [SerializeField] private LayerMask enemyLayer;

    [Header("Knockback on Hit")]
    [SerializeField] private float enemyKnockbackForce = 5f;

    // Components
    private Animator         anim;
    private PlayerController controller;
    private PlayerControls   controls;

    // State
    private float cooldownTimer;
    private float attackActiveTimer;
    public  bool  IsAttacking => attackActiveTimer > 0f;

    private void Awake()
    {
        anim       = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
    }

    private void Start()
    {
        controls = controller.GetPlayerControls();
    }

    private void Update()
    {
        cooldownTimer     -= Time.deltaTime;
        attackActiveTimer -= Time.deltaTime;

        if (!controller.CanBeControlled) return;

        if (controls.Movement.Attack.WasPressedThisFrame() && cooldownTimer <= 0f)
            PerformAttack();
    }

    // ─── Attack ──────────────────────────────────────────────────────────────

    private void PerformAttack()
    {
        cooldownTimer     = attackCooldown;
        attackActiveTimer = attackDuration;   // timer-based fallback

        if (anim != null)
            anim.SetTrigger("attack");

        // Determine origin even without a dedicated point
        Vector2 origin = attackPoint != null
            ? (Vector2)attackPoint.position
            : (Vector2)transform.position + Vector2.right * controller.GetFacingDirection() * 0.5f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, attackRange, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            EnemyHealth eh = hit.GetComponent<EnemyHealth>();
            if (eh == null) eh = hit.GetComponentInParent<EnemyHealth>();
            if (eh == null) continue;

            eh.TakeDamage(attackDamage);

            Rigidbody2D erb = hit.attachedRigidbody;
            if (erb != null)
            {
                Vector2 dir = (hit.transform.position - transform.position).normalized;
                erb.AddForce(dir * enemyKnockbackForce, ForceMode2D.Impulse);
            }
        }
    }

    /// <summary>Called by an Animation Event at the end of the attack clip.</summary>
    public void OnAttackAnimationEnd()
    {
        attackActiveTimer = 0f;
    }

    // ─── Gizmos ──────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Vector2 origin = attackPoint != null
            ? (Vector2)attackPoint.position
            : (Vector2)transform.position + Vector2.right * 0.5f;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, attackRange);
    }
}
