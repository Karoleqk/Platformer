using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 2f;

    // State
    private Vector3 lastCheckpointPosition;
    private string currentSceneName;
    private PlayerHealth playerHealth;
    private PlayerController playerController;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        FindPlayer();
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            playerController = player.GetComponent<PlayerController>();

            // Set initial checkpoint to player's start position
            lastCheckpointPosition = player.transform.position;

            // Subscribe to death event
            if (playerHealth != null)
            {
                playerHealth.OnPlayerDeath += HandlePlayerDeath;
            }
        }
    }

    /// <summary>
    /// Save a new checkpoint position.
    /// </summary>
    public void SetCheckpoint(Vector3 position)
    {
        lastCheckpointPosition = position;
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
        SceneManager.LoadScene(currentSceneName);
    }

    private void HandlePlayerDeath()
    {
        Invoke(nameof(RespawnPlayer), respawnDelay);
    }

    private void RespawnPlayer()
    {
        // Reload the current scene to reset enemies and objects
        // Then move the player to the checkpoint
        SceneManager.LoadScene(currentSceneName);

        // After scene reload, we need to position the player at checkpoint
        // This is handled in OnSceneLoaded via coroutine
        StartCoroutine(RespawnAtCheckpoint());
    }

    private System.Collections.IEnumerator RespawnAtCheckpoint()
    {
        yield return null; // Wait one frame for scene to load

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = lastCheckpointPosition;

            playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Respawn();
                playerHealth.OnPlayerDeath += HandlePlayerDeath;
            }

            playerController = player.GetComponent<PlayerController>();
        }
    }
}
