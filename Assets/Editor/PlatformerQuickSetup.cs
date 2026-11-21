using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor utility to instantiate and configure fully working checkpoints and hazards
/// into any active scene with a single click from the Unity menu.
/// Automatically sets SortingLayer to 'Foreground' so assets are visible in front of tilemaps.
/// </summary>
public static class PlatformerQuickSetup
{
    private const string SPIKE_PREFAB_PATH = "Assets/Locations/Entry/Pixel Art Platformer - Village Props/Prefab/PF Village Props - Spike.prefab";
    private const string STONE_PREFAB_PATH = "Assets/Locations/Entry/Pixel Art Platformer - Village Props/Prefab/PF Village Props - Stone of Recall.prefab";

    [MenuItem("GameObject/Platformer Hazards/Create Checkpoint (Stone of Recall)", false, 10)]
    public static void CreateCheckpoint()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(STONE_PREFAB_PATH);
        GameObject go;

        if (prefab != null)
        {
            go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
        else
        {
            go = new GameObject("Checkpoint");
        }

        go.name = "Checkpoint";
        PositionNearSceneView(go);

        // Ensure visible sorting layer
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Foreground";
            sr.sortingOrder = 5;
        }

        Transform glow = go.transform.Find("Glow");
        if (glow != null)
        {
            SpriteRenderer glowSr = glow.GetComponent<SpriteRenderer>();
            if (glowSr != null)
            {
                glowSr.sortingLayerName = "Foreground";
                glowSr.sortingOrder = 6;
            }
        }

        // Add BoxCollider2D trigger
        BoxCollider2D col = go.GetComponent<BoxCollider2D>();
        if (col == null) col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 1.2f);
        col.offset = new Vector2(0f, 0.4f);

        // Add Checkpoint component
        Checkpoint cp = go.GetComponent<Checkpoint>();
        if (cp == null) cp = go.AddComponent<Checkpoint>();

        Undo.RegisterCreatedObjectUndo(go, "Create Checkpoint");
        Selection.activeGameObject = go;
    }

    [MenuItem("GameObject/Platformer Hazards/Create Spike Trap", false, 11)]
    public static void CreateSpikeTrap()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SPIKE_PREFAB_PATH);
        GameObject go;

        if (prefab != null)
        {
            go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
        else
        {
            go = new GameObject("SpikeTrap");
        }

        go.name = "SpikeTrap";
        PositionNearSceneView(go);

        // Ensure visible sorting layer
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Foreground";
            sr.sortingOrder = 5;
        }

        // Add BoxCollider2D trigger
        BoxCollider2D col = go.GetComponent<BoxCollider2D>();
        if (col == null) col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.9f, 0.35f);
        col.offset = new Vector2(0f, 0.15f);

        // Add SpikeTrap component
        SpikeTrap spike = go.GetComponent<SpikeTrap>();
        if (spike == null) spike = go.AddComponent<SpikeTrap>();

        Undo.RegisterCreatedObjectUndo(go, "Create Spike Trap");
        Selection.activeGameObject = go;
    }

    [MenuItem("GameObject/Platformer Hazards/Create Kill Zone (Abyss)", false, 12)]
    public static void CreateKillZone()
    {
        GameObject go = new GameObject("KillZone");
        PositionNearSceneView(go);

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(100f, 3f);

        go.AddComponent<KillZone>();

        Undo.RegisterCreatedObjectUndo(go, "Create Kill Zone");
        Selection.activeGameObject = go;
    }

    [MenuItem("GameObject/Platformer Hazards/Create Moving Platform", false, 13)]
    public static void CreateMovingPlatform()
    {
        GameObject go = new GameObject("MovingPlatform");
        PositionNearSceneView(go);

        // Add Kinematic Rigidbody2D
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;

        // Add BoxCollider2D (solid, not trigger, so player can stand on it)
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(2f, 0.44f);

        // Add SpriteRenderer (MovingPlatform will auto-populate with procedural wooden platform if null)
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Foreground";
        sr.sortingOrder = 5;

        // Add MovingPlatform component
        go.AddComponent<MovingPlatform>();

        Undo.RegisterCreatedObjectUndo(go, "Create Moving Platform");
        Selection.activeGameObject = go;
    }

    private static void PositionNearSceneView(GameObject go)
    {
        if (SceneView.lastActiveSceneView != null)
        {
            Vector3 camPos = SceneView.lastActiveSceneView.camera.transform.position;
            go.transform.position = new Vector3(Mathf.Round(camPos.x), Mathf.Round(camPos.y), 0f);
        }
        else
        {
            go.transform.position = Vector3.zero;
        }
    }
}
