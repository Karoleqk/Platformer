using UnityEngine;

public class LevelEnd : MonoBehaviour, IInteractable
{
    [Header("Next Level")]
    [SerializeField] private string nextSceneName;

    [Header("Requirements")]
    [Tooltip("If true, all enemies must be defeated before the door opens.")]
    [SerializeField] private bool requireAllEnemiesDefeated = true;

    [Header("Visual")]
    [SerializeField] private Animator doorAnimator;

    private bool isOpen;
    private bool playerInRange;

    public void Interact()
    {
        if (isOpen)
        {
            LoadNextLevel();
        }
    }

    private void Update()
    {
        if (!isOpen && ShouldOpen())
        {
            OpenDoor();
        }
    }

    private bool ShouldOpen()
    {
        if (!requireAllEnemiesDefeated) return true;

        // Check if all enemies in the scene are dead
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (EnemyAI enemy in enemies)
        {
            if (enemy.CurrentState != EnemyAI.EnemyState.Dead) return false;
        }
        return true;
    }

    private void OpenDoor()
    {
        isOpen = true;

        if (doorAnimator != null)
            doorAnimator.SetTrigger("open");
    }

    private void LoadNextLevel()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("LevelEnd: Next scene name is not set!");
            return;
        }

        GameManager.Instance?.LoadLevel(nextSceneName);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player == null) player = collision.GetComponentInParent<Player>();

        if (player != null)
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player == null) player = collision.GetComponentInParent<Player>();

        if (player != null)
        {
            playerInRange = false;
        }
    }
}
