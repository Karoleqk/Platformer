using UnityEngine;

/// <summary>
/// Projectile launched by CannonTrap.
/// Deals damage on collision and destroys on impact with obstacles or player.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class CannonBall : MonoBehaviour
{
    [Header("Damage & Impact")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float knockbackForce = 6f;
    [SerializeField] private float lifeTime = 5f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    /// <summary>
    /// Launches the cannon ball in the specified direction with given speed.
    /// </summary>
    public void Launch(Vector2 direction, float speed)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction.normalized * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleHit(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.gameObject);
    }

    private void HandleHit(GameObject hitObj)
    {
        // Don't collide with the cannon itself
        if (hitObj.GetComponent<CannonTrap>() != null) return;

        Player player = hitObj.GetComponent<Player>();
        if (player == null) player = hitObj.GetComponentInParent<Player>();

        if (player != null)
        {
            Health playerHealth = hitObj.GetComponent<Health>();
            if (playerHealth == null) playerHealth = hitObj.GetComponentInParent<Health>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);

                PlayerController controller = player.GetComponent<PlayerController>();
                if (controller != null)
                {
                    controller.ApplyKnockback(transform.position, knockbackForce);
                }
            }
        }

        // Destroy cannonball on any impact (ground, wall, player)
        Destroy(gameObject);
    }
}
