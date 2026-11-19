using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player melee combat — attack input, cooldown, input buffering, hitbox detection, and damage dealing.
/// Designed according to industry-standard 2D action-platformer combat responsiveness.
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private int attackDamage = 1;
    [Tooltip("Minimum time in seconds between attacks")]
    [SerializeField] private float attackCooldown = 0.35f;
    [SerializeField] private LayerMask enemyLayers;

    [Header("Input Buffering (Płynność sterowania)")]
    [Tooltip("If player presses attack within this many seconds before cooldown ends, the attack will execute immediately when ready")]
    [SerializeField] private float inputBufferTime = 0.15f;

    [Header("Knockback Dealt to Enemy")]
    [SerializeField] private float knockbackForce = 6f;

    [Header("Game Feel & Impact")]
    [Tooltip("Brief frame freeze in seconds when a melee hit connects (like Hollow Knight / Dead Cells)")]
    [SerializeField] private float hitStopDuration = 0.05f;

    private InputAction attackAction;
    private bool canAttack = true;
    private float lastAttackInputTime = -999f;
    private PlayerAnimator playerAnimator;
    private PlayerController playerController;

    public bool CanAttack => canAttack;

    private void Awake()
    {
        attackAction = new InputAction("Attack", InputActionType.Button);
        attackAction.AddBinding("<Mouse>/leftButton");
        attackAction.AddBinding("<Keyboard>/j");

        playerAnimator = GetComponent<PlayerAnimator>();
        playerController = GetComponent<PlayerController>();
    }

    private void OnEnable() => attackAction.Enable();
    private void OnDisable() => attackAction.Disable();

    private void Update()
    {
        // Register attack input and timestamp it for buffering
        if (attackAction.WasPressedThisFrame())
        {
            lastAttackInputTime = Time.time;
        }

        // Check if an attack should be executed (ready + buffered input + not currently stunned)
        bool hasBufferedInput = (Time.time - lastAttackInputTime) <= inputBufferTime;
        bool isStunned = playerController != null && playerController.IsHurtStunned;

        if (hasBufferedInput && canAttack && !isStunned)
        {
            lastAttackInputTime = -999f; // Consume buffered input
            Attack();
        }
    }

    private void Attack()
    {
        canAttack = false;

        // Trigger animation
        if (playerAnimator != null)
            playerAnimator.TriggerAttack();

        // Determine attack position (fallback to in front of player if attackPoint not assigned)
        Vector3 attackPos;
        if (attackPoint != null)
        {
            attackPos = attackPoint.position;
        }
        else
        {
            attackPos = transform.position + transform.right * (attackRange * 0.7f);
        }

        // Detect all enemies in attack range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPos, attackRange, enemyLayers);

        if (hitEnemies.Length > 0 && hitStopDuration > 0f)
        {
            HitStop.Freeze(hitStopDuration);
        }

        foreach (Collider2D enemy in hitEnemies)
        {
            // Deal damage
            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth == null)
                enemyHealth = enemy.GetComponentInParent<Health>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
            }

            // Apply directional knockback to enemy
            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
            if (enemyRb == null)
                enemyRb = enemy.GetComponentInParent<Rigidbody2D>();

            if (enemyRb != null)
            {
                float dirX = Mathf.Sign(enemy.transform.position.x - transform.position.x);
                if (Mathf.Abs(enemy.transform.position.x - transform.position.x) < 0.05f)
                    dirX = transform.right.x;

                Vector2 knockbackDir = new Vector2(dirX, 0.4f).normalized;

                enemyRb.linearVelocity = new Vector2(0, 0);
                enemyRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
            }
        }

        StartCoroutine(AttackCooldownCoroutine());
    }

    private IEnumerator AttackCooldownCoroutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 gizmoPos = attackPoint != null ? attackPoint.position : (transform.position + transform.right * (attackRange * 0.7f));
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gizmoPos, attackRange);
    }
}
