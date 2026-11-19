using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton GameManager — persists across scenes.
/// Manages checkpoint data, player death, and respawn flow.
/// Automatically creates itself if not present in the scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<GameManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GameManager (Auto-Generated)");
                    instance = go.AddComponent<GameManager>();
                }
            }
            return instance;
        }
    }

    [Header("Respawn Settings")]
    [SerializeField] private float respawnDelay = 1.5f;

    private Vector3 lastCheckpointPosition;
    private string lastCheckpointScene;
    private bool hasCheckpoint = false;
    private bool pendingRespawn = false;

    /// <summary>
    /// True while waiting for a respawn scene load.
    /// </summary>
    public bool IsPendingRespawn => pendingRespawn;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Called by Checkpoint objects when the player activates them.
    /// </summary>
    public void SetCheckpoint(Vector3 position, string sceneName)
    {
        lastCheckpointPosition = position;
        lastCheckpointScene = sceneName;
        hasCheckpoint = true;
    }

    /// <summary>
    /// Called by PersistentPlayer when the player dies.
    /// </summary>
    public void OnPlayerDeath()
    {
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        RespawnPlayer();
    }

    private void RespawnPlayer()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (hasCheckpoint)
        {
            if (lastCheckpointScene != currentScene)
            {
                pendingRespawn = true;
                SceneManager.LoadScene(lastCheckpointScene);
            }
            else
            {
                ResetPlayer(lastCheckpointPosition, true);
            }
        }
        else
        {
            // No checkpoint: reload current scene so enemies and level elements reset
            pendingRespawn = true;
            SceneManager.LoadScene(currentScene);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!pendingRespawn) return;

        pendingRespawn = false;

        if (hasCheckpoint)
        {
            ResetPlayer(lastCheckpointPosition, true);
        }
        else
        {
            // Reset to SpawnPoint position
            ResetPlayer(Vector3.zero, false);
        }
    }

    private void ResetPlayer(Vector3 position, bool useGivenPosition)
    {
        StartCoroutine(ResetPlayerCoroutine(position, useGivenPosition));
    }

    private IEnumerator ResetPlayerCoroutine(Vector3 position, bool useGivenPosition)
    {
        yield return null; // Wait one frame for scene objects to settle

        Player player = FindAnyObjectByType<Player>();
        if (player == null) yield break;

        if (useGivenPosition)
        {
            player.transform.position = position;
        }
        else
        {
            SpawnPoint spawn = FindAnyObjectByType<SpawnPoint>();
            if (spawn != null)
                player.transform.position = spawn.transform.position;
        }

        // Reset health
        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth != null)
            playerHealth.ResetHealth();

        // Re-enable controls
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null) controller.enabled = true;

        PlayerCombat combat = player.GetComponent<PlayerCombat>();
        if (combat != null) combat.enabled = true;

        // Reset animation state
        PlayerAnimator animator = player.GetComponent<PlayerAnimator>();
        if (animator != null) animator.ResetAnimationState();

        // Reset physics
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        Collider2D col = player.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
    }

    /// <summary>
    /// Load a new level by scene name.
    /// </summary>
    public void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Reload the current level.
    /// </summary>
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Clears all checkpoint data (e.g. when starting a new game).
    /// </summary>
    public void ClearCheckpoints()
    {
        hasCheckpoint = false;
    }
}
