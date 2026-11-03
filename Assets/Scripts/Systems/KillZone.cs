using UnityEngine;

/// <summary>
/// Place this on an invisible trigger collider at the bottom of the level.
/// When the player falls into it, they die instantly.
/// </summary>
public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(999);
            }
        }
    }
}
