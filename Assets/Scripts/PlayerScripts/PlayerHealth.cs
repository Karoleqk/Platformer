using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;

    [Header("Invincibility Frames")]
    [SerializeField] private float invincibilityDuration = 1.5f;
    [SerializeField] private float flashInterval         = 0.12f;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 8f;

    // Components — use GetComponentInChildren so sprite can live on a child
    private Animator         anim;
    private SpriteRenderer   sr;
    private PlayerController controller;

    // State
    private int   currentHealth;
    private bool  isInvincible;
    private float invincTimer;
    private float flashTimer;

    // Events consumed by UI, GameManager, etc.
    public event Action<int, int> OnHealthChanged;  // (current, max)
    public event Action           OnPlayerDeath;

    public int  CurrentHealth => currentHealth;
    public int  MaxHealth     => maxHealth;
    public bool IsDead        { get; private set; }

    private void Awake()
    {
        anim       = GetComponent<Animator>();
        sr         = GetComponentInChildren<SpriteRenderer>();   // works on child too
        controller = GetComponent<PlayerController>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Update()
    {
        HandleInvincibility();
    }

    // ─── Public API ──────────────────────────────────────────────────────────

    /// <summary>Deal damage to the player. damageSourcePos is used for knockback direction.</summary>
    public void TakeDamage(int damage, Vector2? damageSourcePos = null)
    {
        if (isInvincible || IsDead) return;

        currentHealth = Mathf.Max(currentHealth - damage, 0);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (anim != null) anim.SetTrigger("hit");

        // Knockback
        if (damageSourcePos.HasValue && controller != null)
        {
            Vector2 dir = ((Vector2)transform.position - damageSourcePos.Value).normalized;
            dir.y = Mathf.Max(dir.y, 0.3f);   // small upward component
            controller.Knockback(dir, knockbackForce);
        }

        if (currentHealth <= 0)
            Die();
        else
            BeginInvincibility();
    }

    /// <summary>Heal the player by the given amount (capped at maxHealth).</summary>
    public void Heal(int amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>Restore full health, re-enable control, reset collider — called by GameManager after respawn.</summary>
    public void Respawn()
    {
        IsDead        = false;
        currentHealth = maxHealth;
        isInvincible  = false;
        invincTimer   = 0f;

        if (sr != null) { sr.enabled = true; sr.color = Color.white; }
        if (controller != null) controller.CanBeControlled = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // ─── Private ─────────────────────────────────────────────────────────────

    private void Die()
    {
        IsDead = true;

        if (controller != null) controller.CanBeControlled = false;
        if (anim != null)       anim.SetTrigger("die");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        OnPlayerDeath?.Invoke();
    }

    private void BeginInvincibility()
    {
        isInvincible = true;
        invincTimer  = invincibilityDuration;
        flashTimer   = 0f;
    }

    private void HandleInvincibility()
    {
        if (!isInvincible) return;

        invincTimer -= Time.deltaTime;
        flashTimer  -= Time.deltaTime;

        if (flashTimer <= 0f && sr != null)
        {
            sr.enabled = !sr.enabled;
            flashTimer = flashInterval;
        }

        if (invincTimer <= 0f)
        {
            isInvincible = false;
            if (sr != null) sr.enabled = true;
        }
    }
}
