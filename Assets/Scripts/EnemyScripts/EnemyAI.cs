using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Hit
    }

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float idleWaitTime = 2f;

    [Header("Chase")]
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float chaseStopRange = 8f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackKnockbackForce = 6f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Wall Check")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckDistance = 0.3f;

    // Components
    private Rigidbody2D rb;
    private Animator anim;
    private EnemyHealth enemyHealth;

    // State
    private EnemyState currentState = EnemyState.Patrol;
    private Transform playerTransform;
    private int currentPatrolIndex;
    private float idleTimer;
    private float attackCooldownTimer;
    private bool isFacingRight = true;

    public EnemyState CurrentState => currentState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void Start()
    {
        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        if (patrolPoints.Length == 0)
        {
            currentState = EnemyState.Idle;
        }
    }

    private void Update()
    {
        if (enemyHealth != null && enemyHealth.IsDead) return;

        attackCooldownTimer -= Time.deltaTime;

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

        CheckForPlayer();
        UpdateAnimations();
    }

    #region State Handlers

    private void HandleIdle()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0f && patrolPoints.Length > 0)
        {
            currentState = EnemyState.Patrol;
        }
    }

    private void HandlePatrol()
    {
        if (patrolPoints.Length == 0) return;

        Transform target = patrolPoints[currentPatrolIndex];
        Vector2 direction = (target.position - transform.position).normalized;

        // Check for wall or edge
        if (IsWallAhead() || !IsGroundAhead())
        {
            Flip();
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            currentState = EnemyState.Idle;
            idleTimer = idleWaitTime;
            return;
        }

        // Move toward patrol point
        rb.linearVelocity = new Vector2(direction.x * patrolSpeed, rb.linearVelocity.y);
        FaceDirection(direction.x);

        // Check if reached patrol point
        float distance = Vector2.Distance(transform.position, target.position);
        if (distance < 0.3f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            currentState = EnemyState.Idle;
            idleTimer = idleWaitTime;
        }
    }

    private void HandleChase()
    {
        if (playerTransform == null) return;

        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Check if we should attack
        if (distToPlayer <= attackRange && attackCooldownTimer <= 0f)
        {
            currentState = EnemyState.Attack;
            return;
        }

        // Stop chasing if too far
        if (distToPlayer > chaseStopRange)
        {
            currentState = EnemyState.Patrol;
            return;
        }

        // Move toward player
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * chaseSpeed, rb.linearVelocity.y);
        FaceDirection(direction.x);
    }

    private void HandleAttack()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (attackCooldownTimer <= 0f)
        {
            PerformAttack();
            attackCooldownTimer = attackCooldown;
        }

        // Return to chase after attack
        currentState = EnemyState.Chase;
    }

    #endregion

    #region Detection

    private void CheckForPlayer()
    {
        if (playerTransform == null) return;
        if (currentState == EnemyState.Attack) return;

        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distToPlayer <= detectionRange && currentState != EnemyState.Chase)
        {
            currentState = EnemyState.Chase;
        }
        else if (distToPlayer > chaseStopRange && currentState == EnemyState.Chase)
        {
            currentState = EnemyState.Patrol;
        }
    }

    private bool IsWallAhead()
    {
        if (wallCheck == null) return false;

        Vector2 dir = isFacingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, dir, wallCheckDistance, groundLayer);
        return hit.collider != null;
    }

    private bool IsGroundAhead()
    {
        if (groundCheck == null) return true;

        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider != null;
    }

    #endregion

    #region Attack

    private void PerformAttack()
    {
        if (anim != null)
            anim.SetTrigger("attack");

        // Check if player is in range
        if (playerTransform == null) return;

        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distToPlayer <= attackRange)
        {
            PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage, transform.position);
            }
        }
    }

    #endregion

    #region Movement Helpers

    private void FaceDirection(float directionX)
    {
        if (directionX > 0 && !isFacingRight)
            Flip();
        else if (directionX < 0 && isFacingRight)
            Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    #endregion

    #region Animation

    private void UpdateAnimations()
    {
        if (anim == null) return;

        bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f;
        anim.SetBool("isMoving", isMoving);
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Chase stop range
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, chaseStopRange);

        // Ground check
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
        }

        // Wall check
        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Vector3 dir = isFacingRight ? Vector3.right : Vector3.left;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + dir * wallCheckDistance);
        }

        // Patrol points
        if (patrolPoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (Transform point in patrolPoints)
            {
                if (point != null)
                    Gizmos.DrawSphere(point.position, 0.2f);
            }
        }
    }
}
