using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Color activatedColor = Color.green;
    [SerializeField] private Color defaultColor = Color.white;

    private SpriteRenderer spriteRenderer;
    private bool isActivated;

    public bool IsActivated => isActivated;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = defaultColor;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isActivated)
        {
            ActivateCheckpoint();
        }
    }

    private void ActivateCheckpoint()
    {
        isActivated = true;

        // Deactivate all other checkpoints
        Checkpoint[] allCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        foreach (Checkpoint cp in allCheckpoints)
        {
            if (cp != this)
                cp.Deactivate();
        }

        // Visual feedback
        if (spriteRenderer != null)
            spriteRenderer.color = activatedColor;

        // Register with GameManager
        GameManager.Instance?.SetCheckpoint(transform.position);
    }

    public void Deactivate()
    {
        isActivated = false;
        if (spriteRenderer != null)
            spriteRenderer.color = defaultColor;
    }
}
