using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays player's health on screen.
/// Works out of the box with an instant OnGUI overlay (no Canvas setup required!),
/// while also supporting optional Unity UI Slider or Hearts if assigned.
/// </summary>
public class HealthUI : MonoBehaviour
{
    public static HealthUI Instance { get; private set; }

    [Header("Optional Canvas UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text healthText;

    [Header("OnGUI HUD (Instant - works without Canvas)")]
    [SerializeField] private bool showOnGUI = true;
    [SerializeField] private Vector2 guiPosition = new Vector2(20, 20);
    [SerializeField] private Vector2 barSize = new Vector2(200, 25);
    [SerializeField] private Color barColor = new Color(0.85f, 0.15f, 0.15f, 1f);
    [SerializeField] private Color backColor = new Color(0.15f, 0.15f, 0.15f, 0.8f);

    private Health playerHealth;
    private Texture2D barTex;
    private Texture2D backTex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Prepare textures for OnGUI drawing
        barTex = MakeColorTexture(barColor);
        backTex = MakeColorTexture(backColor);
    }

    private void Start()
    {
        FindPlayerHealth();
    }

    private void Update()
    {
        if (playerHealth == null)
        {
            FindPlayerHealth();
        }
    }

    private void FindPlayerHealth()
    {
        Player player = FindAnyObjectByType<Player>();
        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged.AddListener(OnHealthChanged);
                UpdateCanvasUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        UpdateCanvasUI(current, max);
    }

    private void UpdateCanvasUI(int current, int max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }

        if (healthText != null)
        {
            healthText.text = $"HP: {current} / {max}";
        }
    }

    private void OnGUI()
    {
        if (!showOnGUI || playerHealth == null) return;

        int current = playerHealth.CurrentHealth;
        int max = playerHealth.MaxHealth;
        float percent = max > 0 ? (float)current / max : 0f;

        // Background box
        Rect backRect = new Rect(guiPosition.x, guiPosition.y, barSize.x, barSize.y);
        GUI.DrawTexture(backRect, backTex);

        // Health fill
        Rect fillRect = new Rect(guiPosition.x + 2, guiPosition.y + 2, (barSize.x - 4) * percent, barSize.y - 4);
        GUI.DrawTexture(fillRect, barTex);

        // Text style
        GUIStyle textStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        textStyle.normal.textColor = Color.white;

        // Draw HP text over the bar
        GUI.Label(backRect, $"HP: {current} / {max}", textStyle);

        // If player died, show Respawning notification
        if (current <= 0)
        {
            GUIStyle deadStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            deadStyle.normal.textColor = Color.red;

            Rect deadRect = new Rect(Screen.width / 2f - 150, Screen.height / 2f - 40, 300, 80);
            GUI.Box(deadRect, "");
            GUI.Label(deadRect, "GAME OVER\nRespawn...", deadStyle);
        }
    }

    private Texture2D MakeColorTexture(Color col)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, col);
        tex.Apply();
        return tex;
    }
}
