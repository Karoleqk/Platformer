using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("Damage & Knockback")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float knockbackForce = 8f;

    [Header("Animated Spikes (optional)")]
    [SerializeField] private bool isAnimated;
    [SerializeField] private float activeTime = 1.5f;
    [SerializeField] private float inactiveTime = 2f;

    private Collider2D spikeCollider;
    private SpriteRenderer spriteRenderer;
    private float timer;
    private bool isActive = true;

    private void Awake()
    {
        spikeCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        timer = activeTime;
    }

    private void Update()
    {
        if (!isAnimated) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            isActive = !isActive;
            timer = isActive ? activeTime : inactiveTime;

            if (spikeCollider != null)
                spikeCollider.enabled = isActive;

            if (spriteRenderer != null)
                spriteRenderer.enabled = isActive;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandlePlayerCollision(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        HandlePlayerCollision(collision);
    }

    private void HandlePlayerCollision(Collider2D collision)
    {
        if (!isActive) return;

        Player player = collision.GetComponent<Player>();
        if (player == null) player = collision.GetComponentInParent<Player>();

        if (player != null)
        {
            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth == null) playerHealth = collision.GetComponentInParent<Health>();

            if (playerHealth != null && !playerHealth.IsInvincible)
            {
                playerHealth.TakeDamage(damage);

                PlayerController controller = player.GetComponent<PlayerController>();
                if (controller != null)
                {
                    controller.ApplyKnockback(transform.position, knockbackForce);
                }
            }
        }
    }
}
