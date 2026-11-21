using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Reusable health component for both Player and Enemies.
/// Handles HP, damage, healing, invincibility frames, and visual feedback.
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invincibilityDuration = 1f;

    [Header("Visual Feedback")]
    [SerializeField] private bool flashOnDamage = true;
    [SerializeField] private Color damageFlashColor = Color.red;

    [Header("Death Settings")]
    [Tooltip("If true, this GameObject will be destroyed when health reaches 0 (useful if no EnemyAI/Death script handles it).")]
    [SerializeField] private bool destroyOnDeath = false;
    [SerializeField] private float destroyDelay = 0.5f;

    [Header("Events")]
    public UnityEvent<int, int> OnHealthChanged;
    public UnityEvent OnDamaged;
    public UnityEvent OnDied;

    private int currentHealth;
    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsAlive => currentHealth > 0;
    public bool IsInvincible => isInvincible;

    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    /// <summary>
    /// Deal damage to this entity. Respects invincibility frames.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (!IsAlive || isInvincible) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamaged?.Invoke();

        if (flashOnDamage && spriteRenderer != null)
            StartCoroutine(DamageFlashCoroutine());

        if (!IsAlive)
        {
            OnDied?.Invoke();

            if (destroyOnDeath)
            {
                Destroy(gameObject, destroyDelay);
            }
        }
        else if (invincibilityDuration > 0)
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    /// <summary>
    /// Heal this entity by the given amount (capped at maxHealth).
    /// </summary>
    public void Heal(int amount)
    {
        if (!IsAlive) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Fully restore health and clear invincibility. Used on respawn.
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isInvincible = false;
        StopAllCoroutines();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        // Blink effect during invincibility
        if (spriteRenderer != null)
        {
            float elapsed = 0f;
            while (elapsed < invincibilityDuration)
            {
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f);
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.2f;
            }
            spriteRenderer.color = originalColor;
        }
        else
        {
            yield return new WaitForSeconds(invincibilityDuration);
        }

        isInvincible = false;
    }

    private IEnumerator DamageFlashCoroutine()
    {
        if (spriteRenderer == null) yield break;

        spriteRenderer.color = damageFlashColor;
        yield return new WaitForSeconds(0.1f);

        if (!isInvincible)
            spriteRenderer.color = originalColor;
    }
}
