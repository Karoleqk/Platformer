using System.Collections;
using UnityEngine;

/// <summary>
/// Universal 2D Camera Shake effect.
/// Works with both standard cameras and Cinemachine by applying the shake offset
/// in LateUpdate with high execution order (after Cinemachine has positioned the camera).
/// Can be invoked from anywhere via CameraShake.Shake(intensity, duration).
/// </summary>
[DefaultExecutionOrder(1000)]
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Default Settings")]
    [SerializeField] private float defaultIntensity = 0.2f;
    [SerializeField] private float defaultDuration = 0.15f;

    private float shakeIntensity = 0f;
    private float shakeTimeRemaining = 0f;
    private float currentDuration = 0f;
    private Vector3 shakeOffset = Vector3.zero;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Shake the camera with custom intensity and duration.
    /// Can be called statically from any script.
    /// </summary>
    public static void Shake(float intensity = 0.2f, float duration = 0.15f)
    {
        if (Instance == null)
        {
            // Auto-attach to Main Camera if not yet in scene
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                Instance = mainCam.gameObject.AddComponent<CameraShake>();
            }
            else
            {
                GameObject go = new GameObject("CameraShakeManager");
                Instance = go.AddComponent<CameraShake>();
                DontDestroyOnLoad(go);
            }
        }

        Instance.TriggerShake(intensity, duration);
    }

    private void TriggerShake(float intensity, float duration)
    {
        shakeIntensity = intensity;
        currentDuration = duration;
        shakeTimeRemaining = duration;
    }

    private void LateUpdate()
    {
        if (shakeTimeRemaining > 0f)
        {
            shakeTimeRemaining -= Time.deltaTime;

            // Decay shake intensity over time
            float progress = 1f - (shakeTimeRemaining / currentDuration);
            float damper = 1f - Mathf.Clamp01(progress);

            float offsetX = Random.Range(-1f, 1f) * shakeIntensity * damper;
            float offsetY = Random.Range(-1f, 1f) * shakeIntensity * damper;
            shakeOffset = new Vector3(offsetX, offsetY, 0f);

            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position += shakeOffset;
            }
        }
        else
        {
            shakeOffset = Vector3.zero;
        }
    }
}
