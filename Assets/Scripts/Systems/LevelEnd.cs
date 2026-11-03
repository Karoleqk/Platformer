using UnityEngine;

public class LevelEnd : MonoBehaviour
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

    private void Update()
    {
        if (!isOpen && ShouldOpen())
        {
            OpenDoor();
        }

        if (isOpen && playerInRange)
        {
            // Player can interact to go to next level
            if (Input.GetKeyDown(KeyCode.E))
            {
                LoadNextLevel();
            }
        }
    }

    private bool ShouldOpen()
    {
        if (!requireAllEnemiesDefeated) return true;

        // Check if all enemies are dead
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        foreach (EnemyHealth enemy in enemies)
        {
            if (!enemy.IsDead) return false;
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
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
