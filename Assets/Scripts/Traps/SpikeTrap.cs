using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 1;

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
        if (!isActive) return;

        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform.position);
            }
        }
    }
}
