using System.Collections;
using UnityEngine;

/// <summary>
/// A moving platform that cycles between two or more waypoints.
/// Uses Rigidbody2D (Kinematic) and rb.MovePosition so physics riders move with it naturally,
/// avoiding reparenting issues with persistent DontDestroyOnLoad Player objects.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Waypoints")]
    [Tooltip("Target waypoints. If empty, platform will oscillate between its start position and start + localOffset.")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private Vector2 localOffset = new Vector2(3f, 0f);

    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float waitTimeAtWaypoint = 0.5f;

    private Rigidbody2D rb;
    private Vector2[] targetPositions;
    private int currentTargetIndex = 0;
    private bool isWaiting = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
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
            // Fallback: oscillate between spawn position and spawn position + localOffset
            targetPositions = new Vector2[2];
            targetPositions[0] = transform.position;
            targetPositions[1] = (Vector2)transform.position + localOffset;
        }
    }

    private void FixedUpdate()
    {
        if (isWaiting || targetPositions == null || targetPositions.Length < 2) return;

        Vector2 target = targetPositions[currentTargetIndex];
        Vector2 newPos = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        if (Vector2.Distance(rb.position, target) < 0.02f)
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        if (waypoints != null && waypoints.Length >= 2)
        {
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;
                Gizmos.DrawSphere(waypoints[i].position, 0.15f);

                int next = (i + 1) % waypoints.Length;
                if (waypoints[next] != null)
                    Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
            }
        }
        else
        {
            Vector3 start = Application.isPlaying && targetPositions != null && targetPositions.Length > 0
                ? (Vector3)targetPositions[0]
                : transform.position;
            Vector3 end = start + (Vector3)localOffset;

            Gizmos.DrawSphere(start, 0.15f);
            Gizmos.DrawSphere(end, 0.15f);
            Gizmos.DrawLine(start, end);
        }
    }
}
