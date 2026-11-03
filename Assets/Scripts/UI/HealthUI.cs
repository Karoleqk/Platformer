using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("Heart Images")]
    [SerializeField] private Image[] heartImages;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;

    private PlayerHealth playerHealth;

    private void Start()
    {
        // Find player health component
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHearts;
                UpdateHearts(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHearts;
        }
    }

    private void UpdateHearts(int currentHealth, int maxHealth)
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < maxHealth)
            {
                heartImages[i].enabled = true;
                heartImages[i].sprite = (i < currentHealth) ? fullHeart : emptyHeart;
            }
            else
            {
                heartImages[i].enabled = false;
            }
        }
    }
}
