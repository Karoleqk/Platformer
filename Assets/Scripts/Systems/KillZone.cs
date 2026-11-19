using UnityEngine;

/// <summary>
/// Place this on an invisible trigger collider at the bottom of the level.
/// When the player falls into it, they die instantly.
/// </summary>
public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player == null) player = collision.GetComponentInParent<Player>();

        if (player != null)
        {
            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth == null) playerHealth = collision.GetComponentInParent<Health>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(999);
            }
        }
    }
}
