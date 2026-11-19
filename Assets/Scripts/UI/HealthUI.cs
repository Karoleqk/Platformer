using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays player's health on screen.
/// Works out of the box with an instant OnGUI overlay (no Canvas setup required!),
/// while also supporting optional Unity UI Slider or Hearts if assigned.
/// Features both a stylized HP bar and pixel-art heart icons!
/// </summary>
public class HealthUI : MonoBehaviour
{
    public static HealthUI Instance { get; private set; }

    public enum DisplayMode { HeartsAndBar, HeartsOnly, BarOnly }

    [Header("Display Mode")]
    [SerializeField] private DisplayMode displayMode = DisplayMode.HeartsAndBar;

    [Header("Optional Canvas UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text healthText;

    [Header("OnGUI HUD (Instant - works without Canvas)")]
    [SerializeField] private bool showOnGUI = true;
    [SerializeField] private Vector2 guiPosition = new Vector2(20, 20);
    [SerializeField] private Vector2 barSize = new Vector2(180, 18);
    [SerializeField] private Color barColor = new Color(0.9f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color backColor = new Color(0.12f, 0.12f, 0.12f, 0.85f);
    [SerializeField] private Color borderColor = new Color(0.9f, 0.8f, 0.3f, 1f); // Golden frame

    private Health playerHealth;
    private Texture2D barTex;
    private Texture2D backTex;
    private Texture2D borderTex;
    private Texture2D fullHeartTex;
    private Texture2D emptyHeartTex;

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
        borderTex = MakeColorTexture(borderColor);

        fullHeartTex = GenerateHeartTexture(new Color(1f, 0.2f, 0.25f, 1f), Color.black);
        emptyHeartTex = GenerateHeartTexture(new Color(0.35f, 0.35f, 0.35f, 0.7f), new Color(0.15f, 0.15f, 0.15f, 0.9f));
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

        float currentY = guiPosition.y;

        // 1. Draw Hearts
        if (displayMode == DisplayMode.HeartsOnly || displayMode == DisplayMode.HeartsAndBar)
        {
            float heartSize = 24f;
            float spacing = 4f;

            for (int i = 0; i < max; i++)
            {
                Rect heartRect = new Rect(guiPosition.x + i * (heartSize + spacing), currentY, heartSize, heartSize);
                Texture2D texToDraw = (i < current) ? fullHeartTex : emptyHeartTex;
                GUI.DrawTexture(heartRect, texToDraw, ScaleMode.ScaleToFit);
            }

            currentY += heartSize + 6f;
        }

        // 2. Draw Health Bar
        if (displayMode == DisplayMode.BarOnly || displayMode == DisplayMode.HeartsAndBar)
        {
            // Golden Border
            Rect borderRect = new Rect(guiPosition.x, currentY, barSize.x + 4, barSize.y + 4);
            GUI.DrawTexture(borderRect, borderTex);

            // Background box
            Rect backRect = new Rect(guiPosition.x + 2, currentY + 2, barSize.x, barSize.y);
            GUI.DrawTexture(backRect, backTex);

            // Health fill
            Rect fillRect = new Rect(guiPosition.x + 2, currentY + 2, barSize.x * percent, barSize.y);
            GUI.DrawTexture(fillRect, barTex);

            // Text style
            GUIStyle textStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            textStyle.normal.textColor = Color.white;

            GUI.Label(backRect, $"{current} / {max}", textStyle);
        }

        // 3. Game Over Overlay
        if (current <= 0)
        {
            GUIStyle deadStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            deadStyle.normal.textColor = new Color(1f, 0.2f, 0.2f, 1f);

            GUIStyle subStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter
            };
            subStyle.normal.textColor = Color.white;

            Rect deadRect = new Rect(Screen.width / 2f - 160, Screen.height / 2f - 50, 320, 100);
            GUI.Box(deadRect, "");
            GUI.Label(new Rect(deadRect.x, deadRect.y + 10, deadRect.width, 40), "GAME OVER", deadStyle);
            GUI.Label(new Rect(deadRect.x, deadRect.y + 55, deadRect.width, 30), "Respawning at checkpoint...", subStyle);
        }
    }

    private Texture2D MakeColorTexture(Color col)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, col);
        tex.Apply();
        return tex;
    }

    /// <summary>
    /// Generates a crisp pixel-art heart texture dynamically.
    /// </summary>
    private Texture2D GenerateHeartTexture(Color fill, Color border)
    {
        int w = 11;
        int h = 10;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point; // Crisp pixel art

        // 0 = transparent, 1 = border, 2 = fill
        int[,] heartMap = new int[,]
        {
            { 0,0,1,1,0,0,0,1,1,0,0 },
            { 0,1,2,2,1,0,1,2,2,1,0 },
            { 1,2,2,2,2,1,2,2,2,2,1 },
            { 1,2,2,2,2,2,2,2,2,2,1 },
            { 1,2,2,2,2,2,2,2,2,2,1 },
            { 0,1,2,2,2,2,2,2,2,1,0 },
            { 0,0,1,2,2,2,2,2,1,0,0 },
            { 0,0,0,1,2,2,2,1,0,0,0 },
            { 0,0,0,0,1,2,1,0,0,0,0 },
            { 0,0,0,0,0,1,0,0,0,0,0 }
        };

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                // Invert y because Unity Texture2D coordinates start from bottom
                int mapY = (h - 1) - y;
                int val = heartMap[mapY, x];

                if (val == 2)
                    tex.SetPixel(x, y, fill);
                else if (val == 1)
                    tex.SetPixel(x, y, border);
                else
                    tex.SetPixel(x, y, Color.clear);
            }
        }

        tex.Apply();
        return tex;
    }
}
