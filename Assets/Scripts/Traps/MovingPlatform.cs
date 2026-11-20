using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A moving platform that smoothly carries the player and other dynamic physics bodies.
/// Uses Rigidbody2D (Kinematic) and directly offsets passengers by the platform's position delta,
/// ensuring the player stays firmly on the platform regardless of PlayerController velocity overwriting.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Waypoints")]
    [Tooltip("Target waypoints. If empty, platform will oscillate between its spawn position and spawn + localOffset.")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private Vector2 localOffset = new Vector2(4f, 0f);

    [Header("Movement Settings")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float waitTimeAtWaypoint = 0.6f;

    [Header("Visuals (Auto-configured if Default)")]
    [SerializeField] private string targetSortingLayer = "Foreground";
    [SerializeField] private int targetOrderInLayer = 5;

    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer spriteRenderer;

    private Vector2[] targetPositions;
    private int currentTargetIndex = 0;
    private bool isWaiting = false;

    // Track passengers currently standing on top of the platform
    private readonly HashSet<Rigidbody2D> passengers = new HashSet<Rigidbody2D>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;

        col = GetComponent<Collider2D>();

        // Ensure SpriteRenderer is visible in front of tilemaps/background
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            if (string.IsNullOrEmpty(spriteRenderer.sortingLayerName) || spriteRenderer.sortingLayerName == "Default")
            {
                spriteRenderer.sortingLayerName = targetSortingLayer;
                spriteRenderer.sortingOrder = targetOrderInLayer;
            }

            // If no sprite assigned, generate a crisp wooden platform sprite
            if (spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = GeneratePlatformSprite();
            }
        }
    }

    private void Start()
    {
        InitializeWaypoints();
    }

    private void InitializeWaypoints()
    {
        if (waypoints != null && waypoints.Length >= 2)
        {
            targetPositions = new Vector2[waypoints.Length];
            for (int i = 0; i < waypoints.Length; i++)
            {
                targetPositions[i] = waypoints[i] != null ? (Vector2)waypoints[i].position : (Vector2)transform.position;
            }
        }
        else
        {
            // Default oscillation between start and start + localOffset
            targetPositions = new Vector2[2];
            targetPositions[0] = transform.position;
            targetPositions[1] = (Vector2)transform.position + localOffset;
        }
    }

    private void FixedUpdate()
    {
        if (isWaiting || targetPositions == null || targetPositions.Length < 2) return;

        Vector2 currentPos = rb.position;
        Vector2 target = targetPositions[currentTargetIndex];
        Vector2 newPos = Vector2.MoveTowards(currentPos, target, speed * Time.fixedDeltaTime);
        Vector2 delta = newPos - currentPos;

        rb.MovePosition(newPos);

        // Move passengers along with the platform
        if (delta != Vector2.zero && passengers.Count > 0)
        {
            passengers.RemoveWhere(p => p == null || !p.gameObject.activeInHierarchy);

            foreach (Rigidbody2D passenger in passengers)
            {
                passenger.position += delta;
            }
        }

        if (Vector2.Distance(newPos, target) < 0.02f)
        {
            StartCoroutine(WaitAtWaypointCoroutine());
        }
    }

    private IEnumerator WaitAtWaypointCoroutine()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTimeAtWaypoint);
        currentTargetIndex = (currentTargetIndex + 1) % targetPositions.Length;
        isWaiting = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryAddPassenger(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryAddPassenger(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Rigidbody2D passengerRb = collision.collider.attachedRigidbody;
        if (passengerRb != null)
        {
            passengers.Remove(passengerRb);
        }
    }

    private void TryAddPassenger(Collision2D collision)
    {
        // Only accept objects that are standing on the top half of the platform
        float platformTop = col != null ? col.bounds.max.y : transform.position.y;
        float passengerBottom = collision.collider.bounds.min.y;

        // If the passenger's bottom is near or above the platform's top surface
        if (passengerBottom >= platformTop - 0.25f)
        {
            Rigidbody2D passengerRb = collision.collider.attachedRigidbody;
            if (passengerRb != null && passengerRb.bodyType == RigidbodyType2D.Dynamic)
            {
                passengers.Add(passengerRb);
            }
        }
    }

    private Sprite GeneratePlatformSprite()
    {
        int w = 64;
        int h = 14;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color woodLight = new Color(0.72f, 0.48f, 0.28f, 1f);
        Color woodDark = new Color(0.48f, 0.30f, 0.16f, 1f);
        Color woodEdge = new Color(0.32f, 0.18f, 0.08f, 1f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (x == 0 || x == w - 1 || y == 0 || y == h - 1)
                {
                    tex.SetPixel(x, y, woodEdge);
                }
                else if (y >= h - 3)
                {
                    tex.SetPixel(x, y, woodLight);
                }
                else
                {
                    // Planks pattern
                    bool isPlankDivider = (x % 16 == 0);
                    tex.SetPixel(x, y, isPlankDivider ? woodEdge : woodDark);
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        Vector3 start = Application.isPlaying && targetPositions != null && targetPositions.Length > 0
            ? (Vector3)targetPositions[0]
            : transform.position;
        Vector3 end = start + (Vector3)localOffset;

        Gizmos.DrawSphere(start, 0.15f);
        Gizmos.DrawSphere(end, 0.15f);
        Gizmos.DrawLine(start, end);
    }
}
