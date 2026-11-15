using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StiffBackground : MonoBehaviour
{
    private GameObject targetCamera;

    private void FixedUpdate()
    {
        transform.position = new Vector3(targetCamera.transform.position.x, targetCamera.transform.position.y, transform.position.z);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Camera cam = FindAnyObjectByType<Camera>();

        if(cam != null)
        {
            targetCamera = cam.gameObject;
        }
    }
}
