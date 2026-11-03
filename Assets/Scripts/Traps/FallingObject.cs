using UnityEngine;

public class FallingObject : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRange = 3f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Falling")]
    [SerializeField] private float fallDelay = 0.5f;
    [SerializeField] private float shakeIntensity = 0.05f;

    [Header("Damage")]
    [SerializeField] private int damage = 1;

    [Header("Respawn")]
    [SerializeField] private bool respawns = true;
    [SerializeField] private float respawnDelay = 3f;

    // Components
    private Rigidbody2D rb;
    private Collider2D col;

    // State
    private Vector3 startPosition;
    private bool isTriggered;
    private bool isFalling;
    private float fallDelayTimer;
    private float shakeTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    private void Start()
    {
        startPosition = transform.position;

        // Start as kinematic (don't fall until triggered)
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void Update()
    {
        if (!isTriggered)
        {
            DetectPlayer();
        }
        else if (!isFalling)
        {
            HandleFallDelay();
        }
    }

    private void DetectPlayer()
    {
        // Check if player is below this object
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, detectionRange, playerLayer);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            TriggerFall();
        }
    }

    private void TriggerFall()
    {
        isTriggered = true;
        fallDelayTimer = fallDelay;
    }

    private void HandleFallDelay()
    {
        fallDelayTimer -= Time.deltaTime;

        // Shake effect before falling
        shakeTimer += Time.deltaTime * 50f;
        float offsetX = Mathf.Sin(shakeTimer) * shakeIntensity;
        transform.position = startPosition + new Vector3(offsetX, 0f, 0f);

        if (fallDelayTimer <= 0f)
        {
            StartFalling();
        }
    }

    private void StartFalling()
    {
        isFalling = true;
        transform.position = startPosition; // Reset shake offset

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isFalling) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform.position);
            }
        }

        // Hit something - stop or respawn
        if (respawns)
        {
            Invoke(nameof(Respawn), respawnDelay);
        }
        else
        {
            Destroy(gameObject, 0.5f);
        }
    }

    private void Respawn()
    {
        transform.position = startPosition;
        isTriggered = false;
        isFalling = false;
        shakeTimer = 0f;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * detectionRange);
    }
}
