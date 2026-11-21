using System.Collections;
using UnityEngine;

/// <summary>
/// Provides hit-stop (frame freeze) effect when attacks connect.
/// Industry-standard game feel technique used in Hollow Knight, Dead Cells, and Super Smash Bros.
/// Temporarily sets Time.timeScale to 0 for a fraction of a second on impact.
/// </summary>
public class HitStop : MonoBehaviour
{
    private static HitStop instance;
    private Coroutine currentCoroutine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    /// <summary>
    /// Freezes frame for specified duration (in real-time seconds).
    /// Safe to call from anywhere without needing a scene reference.
    /// </summary>
    public static void Freeze(float duration = 0.05f)
    {
        if (duration <= 0f) return;

        if (instance == null)
        {
            GameObject go = new GameObject("HitStopManager");
            instance = go.AddComponent<HitStop>();
            DontDestroyOnLoad(go);
        }

        instance.TriggerFreeze(duration);
    }

    private void TriggerFreeze(float duration)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            Time.timeScale = 1f;
        }
        currentCoroutine = StartCoroutine(FreezeCoroutine(duration));
    }

    private IEnumerator FreezeCoroutine(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        currentCoroutine = null;
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
