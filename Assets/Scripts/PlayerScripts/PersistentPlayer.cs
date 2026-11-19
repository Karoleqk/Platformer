using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public static Player instance;

    private Health health;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        health = GetComponent<Health>();

        // Ensure Health UI is present to display HP and Game Over overlay
        if (GetComponent<HealthUI>() == null)
        {
            gameObject.AddComponent<HealthUI>();
        }

        // Ensure PlayerAnimator is present to drive animation parameters
        if (GetComponent<PlayerAnimator>() == null)
        {
            gameObject.AddComponent<PlayerAnimator>();
        }
    }

    private void Start()
    {
        if (health != null)
        {
            health.OnDied.AddListener(OnPlayerDeath);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If GameManager is handling a respawn, let it control positioning
        if (GameManager.Instance != null && GameManager.Instance.IsPendingRespawn)
            return;

        SpawnPoint spawnPoint = FindAnyObjectByType<SpawnPoint>();

        if(spawnPoint != null)
        {
            transform.position = spawnPoint.transform.position;
        }
    }

    /// <summary>
    /// Called when player HP reaches 0.
    /// Disables controls and triggers respawn via GameManager.
    /// </summary>
    private void OnPlayerDeath()
    {
        // Disable player controls
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null) combat.enabled = false;

        // Trigger death animation (will work once Animator is set up)
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.SetTrigger("Death");

        // Stop physics movement
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Tell GameManager to handle respawn
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDeath();
        }
    }
}
