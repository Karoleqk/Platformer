using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Checkpoint — saves the player's respawn position.
/// Add a Collider2D (set as Trigger) to the same GameObject.
/// Only one checkpoint can be active at a time.
/// </summary>
public class Checkpoint : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color inactiveColor = Color.white;

    private SpriteRenderer spriteRenderer;
    private bool isActivated = false;

    private static Checkpoint currentActiveCheckpoint;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.color = inactiveColor;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isActivated) return;
        if (collision.GetComponent<Player>() == null) return;

        ActivateCheckpoint();
    }

    private void ActivateCheckpoint()
    {
        // Deactivate previous checkpoint
        if (currentActiveCheckpoint != null && currentActiveCheckpoint != this)
        {
            currentActiveCheckpoint.Deactivate();
        }

        isActivated = true;
        currentActiveCheckpoint = this;

        if (spriteRenderer != null)
            spriteRenderer.color = activeColor;

        // Save to GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCheckpoint(
                transform.position,
                SceneManager.GetActiveScene().name
            );
        }

        Debug.Log($"Checkpoint activated at {transform.position}");
    }

    private void Deactivate()
    {
        isActivated = false;

        if (spriteRenderer != null)
            spriteRenderer.color = inactiveColor;
    }
}
