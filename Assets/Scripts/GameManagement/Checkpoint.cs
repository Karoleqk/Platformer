using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Checkpoint — saves the player's respawn position.
/// Automatically configures a Trigger Collider2D if one isn't already present.
/// Supports visual feedback via SpriteRenderer color change and optional child "Glow" GameObject.
/// Also restores the player's health and displays a notification banner.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private Color activeColor = new Color(0.2f, 1f, 0.4f, 1f);
    [SerializeField] private Color inactiveColor = Color.white;
    [Tooltip("Optional child object (e.g. 'Glow' on Stone of Recall) enabled when active")]
    [SerializeField] private GameObject glowObject;

    [Header("Checkpoint Features")]
    [Tooltip("Restore player HP to full when activating checkpoint")]
    [SerializeField] private bool healPlayerOnActivate = true;

    private SpriteRenderer spriteRenderer;
    private bool isActivated = false;

    private static Checkpoint currentActiveCheckpoint;

    public bool IsActivated => isActivated;

    private void Reset()
    {
        // Auto-configure trigger collider in Editor
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            if (string.IsNullOrEmpty(spriteRenderer.sortingLayerName) || spriteRenderer.sortingLayerName == "Default")
            {
                spriteRenderer.sortingLayerName = "Foreground";
                spriteRenderer.sortingOrder = 5;
            }
        }

        // Auto-find child named "Glow" if not explicitly assigned
        if (glowObject == null)
        {
            Transform glowChild = transform.Find("Glow");
            if (glowChild != null)
                glowObject = glowChild.gameObject;
        }

        if (glowObject != null)
        {
            SpriteRenderer glowSr = glowObject.GetComponent<SpriteRenderer>();
            if (glowSr != null && (string.IsNullOrEmpty(glowSr.sortingLayerName) || glowSr.sortingLayerName == "Default"))
            {
                glowSr.sortingLayerName = "Foreground";
                glowSr.sortingOrder = 6;
            }
        }

        // Ensure collider is marked as trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Start in inactive visual state
        ApplyVisuals(false);
    }

    private void Start()
    {
        // Check if this checkpoint is already saved in GameManager
        if (GameManager.Instance != null && GameManager.Instance.HasCheckpoint)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            if (GameManager.Instance.LastCheckpointScene == currentScene)
            {
                if (Vector2.Distance(transform.position, GameManager.Instance.LastCheckpointPosition) < 1f)
                {
                    isActivated = true;
                    currentActiveCheckpoint = this;
                    ApplyVisuals(true);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isActivated) return;

        Player player = collision.GetComponent<Player>();
        if (player == null) player = collision.GetComponentInParent<Player>();
        if (player == null && !collision.CompareTag("Player")) return;

        ActivateCheckpoint(player);
    }

    private void ActivateCheckpoint(Player player)
    {
        // Deactivate previous checkpoint
        if (currentActiveCheckpoint != null && currentActiveCheckpoint != this)
        {
            currentActiveCheckpoint.Deactivate();
        }

        isActivated = true;
        currentActiveCheckpoint = this;

        ApplyVisuals(true);

        // Save position to GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCheckpoint(
                transform.position,
                SceneManager.GetActiveScene().name
            );
        }

        // Optional full heal
        if (healPlayerOnActivate && player != null)
        {
            Health health = player.GetComponent<Health>();
            if (health != null)
                health.ResetHealth();
        }

        // UI notification
        if (HealthUI.Instance != null)
        {
            HealthUI.Instance.ShowNotification("CHECKPOINT REACHED!");
        }

        Debug.Log($"Checkpoint activated at {transform.position}");
    }

    public void Deactivate()
    {
        isActivated = false;
        ApplyVisuals(false);
    }

    private void ApplyVisuals(bool active)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = active ? activeColor : inactiveColor;

        if (glowObject != null)
            glowObject.SetActive(active);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isActivated ? Color.green : new Color(1f, 1f, 0f, 0.5f);
        Gizmos.DrawWireCube(transform.position, new Vector3(0.8f, 1.2f, 0f));
    }
}
