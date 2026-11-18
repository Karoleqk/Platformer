using System.Collections;
using UnityEngine;

/// <summary>
/// Enemy AI with state machine: Idle -> Patrol -> Chase -> Attack -> Dead.
/// Also deals contact damage to the player on collision.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Idle, Patrol, Chase, Attack, Hurt, Dead }

    [Header("General")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private int contactDamage = 1;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waitTimeAtPoint = 1f;
    [Tooltip("If no Patrol Points are assigned, automatically patrol left and right of spawn point")]
    [SerializeField] private bool autoPatrolIfNoPoints = true;
    [SerializeField] private float autoPatrolDistance = 3f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 7f;
    [SerializeField] private float attackRange = 1.4f;
    [SerializeField] private float losePlayerRange = 9f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackHitRange = 0.5f;
    [SerializeField] private float playerKnockbackForce = 5f;

    [Header("Hurt & Stun (Ogłuszenie po otrzymaniu ciosu)")]
    [Tooltip("How long the enemy is stunned and cannot move or attack after getting hit")]
    [SerializeField] private float hitStunDuration = 0.4f;
    [Tooltip("Additional delay before enemy can attack again after recovering from stun")]
    [SerializeField] private float postStunAttackDelay = 0.25f;

    private EnemyState currentState = EnemyState.Idle;
    private Rigidbody2D rb;
    private Health health;
    private Animator animator;

    private Transform player;
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;
    private bool canAttack = true;
    private bool isStunned = false;

    private Vector2 spawnPosition;
    private int autoPatrolDirection = 1;

    public EnemyState CurrentState => currentState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        animator = GetComponentInChildren<Animator>();
        spawnPosition = transform.position;
    }

    private void Start()
    {
        // Subscribe to health events
        if (health != null)
        {
            health.OnDamaged.AddListener(OnHurt);
            health.OnDied.AddListener(OnDeath);
        }

        // Find the player
        Player playerObj = FindAnyObjectByType<Player>();
        if (playerObj != null)
            player = playerObj.transform;

        bool hasPatrol = (patrolPoints != null && patrolPoints.Length > 0) || autoPatrolIfNoPoints;
        currentState = hasPatrol ? EnemyState.Patrol : EnemyState.Idle;
    }

    private void Update()
    {
        if (currentState == EnemyState.Dead) return;

        // While in Hurt / Stun state, do not run movement logic so physics knockback pushes enemy freely
        if (currentState == EnemyState.Hurt)
        {
            UpdateAnimator();
            return;
        }

        // Re-acquire player reference if lost (e.g. after scene reload)
        if (player == null)
        {
            Player playerObj = FindAnyObjectByType<Player>();
            if (playerObj != null) player = playerObj.transform;
        }

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdle();
                break;
            case EnemyState.Patrol:
                HandlePatrol();
                break;
            case EnemyState.Chase:
                HandleChase();
                break;
            case EnemyState.Attack:
                HandleAttack();
                break;
        }

        UpdateAnimator();
    }

    // ─── STATE HANDLERS ───────────────────────────────────────────

    private void HandleIdle()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (IsPlayerDetected())
            currentState = EnemyState.Chase;
    }

    private void HandlePatrol()
    {
        if (IsPlayerDetected())
        {
            currentState = EnemyState.Chase;
            isWaiting = false;
            return;
        }

        if (isWaiting) return;

        // Mode 1: Explicit patrol points assigned
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Transform target = patrolPoints[currentPatrolIndex];
            if (target == null) return;

            Vector2 direction = (target.position - transform.position).normalized;
            FaceDirection(direction.x);
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

            if (Vector2.Distance(transform.position, target.position) < 0.3f)
            {
                StartCoroutine(WaitAtPatrolPoint());
            }
        }
        // Mode 2: Auto patrol left & right of spawn position
        else if (autoPatrolIfNoPoints)
        {
            float targetX = spawnPosition.x + (autoPatrolDirection * autoPatrolDistance);
            float diffX = targetX - transform.position.x;

            if (Mathf.Abs(diffX) < 0.2f)
            {
                StartCoroutine(WaitAndFlipAutoPatrol());
            }
            else
            {
                float moveDir = Mathf.Sign(diffX);
                FaceDirection(moveDir);
                rb.linearVelocity = new Vector2(moveDir * moveSpeed, rb.linearVelocity.y);
            }
        }
    }

    private void HandleChase()
    {
        if (player == null)
        {
            ReturnToPassiveState();
            return;
        }

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // Lost the player
        if (distToPlayer > losePlayerRange)
        {
            ReturnToPassiveState();
            return;
        }

        // Close enough to attack
        if (distToPlayer <= attackRange)
        {
            currentState = EnemyState.Attack;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // Move toward player
        Vector2 direction = (player.position - transform.position).normalized;
        FaceDirection(direction.x);
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
    }

    private void HandleAttack()
    {
        if (player == null)
        {
            ReturnToPassiveState();
            return;
        }

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        float distToPlayer = Vector2.Distance(transform.position, player.position);
        FaceDirection((player.position - transform.position).normalized.x);

        // Player moved out of attack range
        if (distToPlayer > attackRange * 1.3f)
        {
            currentState = EnemyState.Chase;
            return;
        }

        if (canAttack)
        {
            PerformAttack();
        }
    }

    // ─── ACTIONS ──────────────────────────────────────────────────

    private void PerformAttack()
    {
        canAttack = false;

        if (animator != null)
            animator.SetTrigger("Attack");

        // Small forward lunge during attack
        rb.linearVelocity = new Vector2(transform.right.x * (moveSpeed * 0.7f), rb.linearVelocity.y);

        // Determine hitbox position & radius (in front of enemy if attackPoint is not set)
        Vector3 hitPosition;
        float hitRadius;

        if (attackPoint != null)
        {
            hitPosition = attackPoint.position;
            hitRadius = attackHitRange;
        }
        else
        {
            // Place hitbox in front of enemy based on facing direction
            hitPosition = transform.position + transform.right * (attackRange * 0.7f);
            hitRadius = Mathf.Max(attackHitRange, attackRange * 0.8f);
        }

        // Layer mask check with fallback
        LayerMask mask = playerLayer.value != 0 ? playerLayer : (LayerMask)(~0);
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(hitPosition, hitRadius, mask);

        foreach (Collider2D hitCol in hitColliders)
        {
            Player playerComp = hitCol.GetComponent<Player>();
            if (playerComp == null) playerComp = hitCol.GetComponentInParent<Player>();

            if (playerComp != null)
            {
                Health playerHealth = hitCol.GetComponent<Health>();
                if (playerHealth == null) playerHealth = hitCol.GetComponentInParent<Health>();

                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(contactDamage);
                    ApplyKnockback(hitCol);
                }
            }
        }

        StartCoroutine(AttackCooldownCoroutine());
    }

    private void ApplyKnockback(Collider2D target)
    {
        // If target is player, use PlayerController.ApplyKnockback which handles hurt stun & direction
        PlayerController playerCtrl = target.GetComponent<PlayerController>();
        if (playerCtrl == null) playerCtrl = target.GetComponentInParent<PlayerController>();

        if (playerCtrl != null)
        {
            playerCtrl.ApplyKnockback(transform.position, playerKnockbackForce);
            return;
        }

        // Generic fallback for any other entity
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb == null) targetRb = target.GetComponentInParent<Rigidbody2D>();

        if (targetRb != null)
        {
            Vector2 knockbackDir = (target.transform.position - transform.position).normalized;
            if (knockbackDir == Vector2.zero) knockbackDir = transform.right;
            targetRb.AddForce(knockbackDir * playerKnockbackForce, ForceMode2D.Impulse);
        }
    }

    private void OnHurt()
    {
        if (currentState == EnemyState.Dead) return;

        StopCoroutine(nameof(HitStunCoroutine));
        StartCoroutine(HitStunCoroutine());
    }

    private IEnumerator HitStunCoroutine()
    {
        currentState = EnemyState.Hurt;
        isStunned = true;
        canAttack = false;

        if (animator != null)
            animator.SetTrigger("Hurt");

        // Stunned: enemy slides back from knockback, cannot move or attack
        yield return new WaitForSeconds(hitStunDuration);

        isStunned = false;

        // Brief delay before the enemy can initiate another attack
        yield return new WaitForSeconds(postStunAttackDelay);
        canAttack = true;

        if (currentState != EnemyState.Dead)
            currentState = EnemyState.Chase;
    }

    private void OnDeath()
    {
        currentState = EnemyState.Dead;
        rb.linearVelocity = Vector2.zero;

        if (animator != null)
            animator.SetTrigger("Death");

        // Disable physics
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        rb.bodyType = RigidbodyType2D.Static;

        // Destroy after delay (gives time for death animation)
        Destroy(gameObject, 1.5f);
    }

    // ─── CONTACT DAMAGE ───────────────────────────────────────────

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleContactDamage(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleContactDamage(collision);
    }

    private void HandleContactDamage(Collision2D collision)
    {
        if (currentState == EnemyState.Dead || isStunned) return;

        // Deal contact damage to the player
        Player playerComp = collision.gameObject.GetComponent<Player>();
        if (playerComp == null) playerComp = collision.gameObject.GetComponentInParent<Player>();

        if (playerComp != null)
        {
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth == null) playerHealth = collision.gameObject.GetComponentInParent<Health>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
                ApplyKnockback(collision.collider);
            }
        }
    }

    // ─── HELPERS ──────────────────────────────────────────────────

    private bool IsPlayerDetected()
    {
        return player != null && Vector2.Distance(transform.position, player.position) <= detectionRange;
    }

    private void ReturnToPassiveState()
    {
        bool hasPatrol = (patrolPoints != null && patrolPoints.Length > 0) || autoPatrolIfNoPoints;
        currentState = hasPatrol ? EnemyState.Patrol : EnemyState.Idle;
    }

    private void FaceDirection(float dirX)
    {
        if (dirX < 0)
            transform.rotation = Quaternion.Euler(0, 180, 0);
        else if (dirX > 0)
            transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool("IsChasing", currentState == EnemyState.Chase);
    }

    private IEnumerator WaitAtPatrolPoint()
    {
        isWaiting = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        yield return new WaitForSeconds(waitTimeAtPoint);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        isWaiting = false;
    }

    private IEnumerator WaitAndFlipAutoPatrol()
    {
        isWaiting = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        yield return new WaitForSeconds(waitTimeAtPoint);
        autoPatrolDirection *= -1;
        isWaiting = false;
    }

    private IEnumerator AttackCooldownCoroutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // ─── GIZMOS ───────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        // Detection range (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Attack range (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Lose player range (transparent red)
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, losePlayerRange);

        // Attack hitbox
        Vector3 gizmoHitPos = attackPoint != null ? attackPoint.position : (transform.position + transform.right * (attackRange * 0.7f));
        float gizmoHitRadius = attackPoint != null ? attackHitRange : Mathf.Max(attackHitRange, attackRange * 0.8f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(gizmoHitPos, gizmoHitRadius);

        // Patrol path
        if (patrolPoints != null && patrolPoints.Length > 1)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] == null) continue;

                Gizmos.DrawSphere(patrolPoints[i].position, 0.15f);

                int next = (i + 1) % patrolPoints.Length;
                if (patrolPoints[next] != null)
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[next].position);
            }
        }
    }
}
