using System.Collections;
using UnityEngine;

/// <summary>
/// Stationary cannon trap that fires cannonballs at intervals.
/// Can shoot continuously on a timer or only when the player is detected in line of fire.
/// </summary>
public class CannonTrap : MonoBehaviour
{
    [Header("Firing Settings")]
    [SerializeField] private GameObject cannonBallPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 2.5f;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private Vector2 fireDirection = Vector2.left;

    [Header("Detection")]
    [Tooltip("If true, only fires when player is detected in line of fire")]
    [SerializeField] private bool requirePlayerDetection = false;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private LayerMask playerLayer;

    private Animator animator;
    private float timer = 0f;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= fireRate)
        {
            if (!requirePlayerDetection || IsPlayerInLineOfSight())
            {
                Shoot();
                timer = 0f;
            }
        }
    }

    private void Shoot()
    {
        if (animator != null)
            animator.SetTrigger("Shoot");

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector2 dir = fireDirection.normalized;

        if (cannonBallPrefab != null)
        {
            GameObject ball = Instantiate(cannonBallPrefab, spawnPos, Quaternion.identity);
            CannonBall cb = ball.GetComponent<CannonBall>();
            if (cb != null)
            {
                cb.Launch(dir, projectileSpeed);
            }
        }
    }

    private bool IsPlayerInLineOfSight()
    {
        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        LayerMask mask = playerLayer.value != 0 ? playerLayer : (LayerMask)(~0);
        RaycastHit2D hit = Physics2D.Raycast(origin, fireDirection.normalized, detectionRange, mask);

        if (hit.collider == null) return false;

        Player player = hit.collider.GetComponent<Player>();
        if (player == null) player = hit.collider.GetComponentInParent<Player>();

        return player != null || hit.collider.CompareTag("Player");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        Gizmos.DrawRay(origin, (Vector3)fireDirection.normalized * detectionRange);
    }
}
