using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;

    [Header("Hit Flash")]
    [SerializeField] private Color hitColor         = Color.red;
    [SerializeField] private float hitFlashDuration = 0.15f;

    // Components
    private Animator       anim;
    private SpriteRenderer sr;

    // State
    private int   currentHealth;
    private float flashTimer;
    private Color baseColor;

    public event Action OnDeath;
    public bool IsDead { get; private set; }

    private void Awake()
    {
        anim = GetComponent<Animator>();
        sr   = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        baseColor     = sr != null ? sr.color : Color.white;
    }

    private void Update()
    {
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f && sr != null)
                sr.color = baseColor;
        }
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        currentHealth -= damage;

        if (sr != null) { sr.color = hitColor; flashTimer = hitFlashDuration; }
        if (anim != null) anim.SetTrigger("hit");

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        IsDead = true;

        if (anim != null) anim.SetTrigger("die");

        // Disable AI & collider
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;

        foreach (Collider2D col in GetComponents<Collider2D>())
            col.enabled = false;

        // Freeze rigidbody in place so corpse doesn't slide
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        OnDeath?.Invoke();

        Destroy(gameObject, 1.2f);
    }
}
