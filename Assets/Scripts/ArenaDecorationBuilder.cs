
using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class ArenaDecorationBuilder : MonoBehaviour
{
    [Header("Arena Dimensions")]
    [Min(10f)] public float arenaSize = 50f;

    [Header("Holotna Environment Prefabs")]
    public GameObject treePrefab;
    public GameObject rockPrefab;
    public GameObject bushPrefab;
    public GameObject grassPrefab;
    public GameObject flowerPrefab;

    [Header("Decoration Options")]
    public bool generateInnerPlants = true;
    public bool generateOuterTrees = true;
    public bool generateOuterRocks = true;

    [Header("Randomization")]
    public int randomSeed = 3303;

    [Header("Visual Scale")]
    [Range(0.2f, 2f)]
    public float innerPlantScale = 0.65f;

    [Range(0.2f, 2f)]
    public float outerTreeScale = 1f;

    [Range(0.2f, 2f)]
    public float outerRockScale = 0.7f;

    [Header("Placement")]
    public float groundHeight = 0f;

    [Min(0f)]
    public float obstacleClearance = 0.7f;

#if UNITY_EDITOR

    private const string RootName = "GeneratedDecorations";

    private Transform generatedRoot;
    private System.Random random;
    private int spawnedCount;

    private readonly List<Vector3> usedPositions =
        new List<Vector3>();

    [ContextMenu("Build Arena Decorations")]
    public void BuildDecorations()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "Stop Play Mode before generating decorations.");
            return;
        }

        if (bushPrefab == null &&
            grassPrefab == null &&
            flowerPrefab == null &&
            treePrefab == null &&
            rockPrefab == null)
        {
            Debug.LogError(
                "Assign at least one environment prefab.");
            return;
        }

        if (!gameObject.scene.IsValid())
        {
            Debug.LogError(
                "Decoration Builder must be in a scene.");
            return;
        }

        random = new System.Random(randomSeed);
        spawnedCount = 0;
        usedPositions.Clear();

        Transform oldRoot = transform.Find(RootName);

        if (oldRoot != null)
        {
            Undo.DestroyObjectImmediate(oldRoot.gameObject);
        }

        GameObject root = new GameObject(RootName);

        Undo.RegisterCreatedObjectUndo(
            root, "Build Arena Decorations");

        generatedRoot = root.transform;
        generatedRoot.SetParent(transform, true);

        generatedRoot.position = Vector3.zero;
        generatedRoot.rotation = Quaternion.identity;
        generatedRoot.localScale = Vector3.one;

        if (generateInnerPlants)
            BuildInnerPlants();

        if (generateOuterTrees)
            BuildOuterTrees();

        if (generateOuterRocks)
            BuildOuterRocks();

        EditorSceneManager.MarkSceneDirty(
            gameObject.scene);

        Debug.Log(
            "PrettyBattle Decorations: " +
            spawnedCount + " prefab instances generated.");
    }

    private void BuildInnerPlants()
    {
        float half = arenaSize * 0.5f;

        // Small vegetation along the inner walls,
        // excluding the actual arena corners.
        float[] wallOffsets =
        {
            -17f, -13f, -9f,
             9f, 13f, 17f
        };

        foreach (float side in new float[] { -1f, 1f })
        {
            foreach (float offset in wallOffsets)
            {
                Vector3 alongX = new Vector3(
                    side * (half - 3f),
                    groundHeight,
                    offset);

                Vector3 alongZ = new Vector3(
                    offset,
                    groundHeight,
                    side * (half - 3f));

                BuildPlantCluster(alongX);
                BuildPlantCluster(alongZ);
            }
        }
    }

    private void BuildPlantCluster(Vector3 center)
    {
        // Keep the clusters compact and fairly sparse.
        // Candidate positions that intersect gameplay
        // obstacles will be skipped.

        GameObject primary = ChoosePlant();

        if (primary != null)
        {
            TrySpawn(
                primary,
                center,
                innerPlantScale,
                0.75f,
                false,
                0.6f);
        }

        // Grass blends the decorative plant into the floor.
        if (grassPrefab != null)
        {
            Vector3 position = center +
                new Vector3(
                    RandomRange(-0.65f, 0.65f),
                    0f,
                    RandomRange(-0.65f, 0.65f));

            TrySpawn(
                grassPrefab,
                position,
                innerPlantScale * 0.85f,
                0.65f,
                false,
                0.5f);
        }

        // A few small flowers create color accents.
        if (flowerPrefab != null && random.NextDouble() < 0.65)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector3 position = center +
                    new Vector3(
                        RandomRange(-0.8f, 0.8f),
                        0f,
                        RandomRange(-0.8f, 0.8f));

                TrySpawn(
                    flowerPrefab,
                    position,
                    innerPlantScale *
                    RandomRange(0.7f, 1.1f),
                    0.4f,
                    false,
                    0.2f);
            }
        }
    }

    private GameObject ChoosePlant()
    {
        GameObject[] options =
        {
            bushPrefab,
            grassPrefab,
            flowerPrefab
        };

        // Prefer grass and flowers over large bushes.
        int[] weights = { 2, 5, 3 };

        int total = 0;

        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] != null)
                total += weights[i];
        }

        if (total == 0)
            return null;

        int selection = random.Next(total);

        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] == null)
                continue;

            selection -= weights[i];

            if (selection < 0)
                return options[i];
        }

        return null;
    }

    private void BuildOuterTrees()
    {
        if (treePrefab == null)
            return;

        float half = arenaSize * 0.5f;

        float[] offsets = { -18f, -9f, 0f, 9f, 18f };

        foreach (float side in new float[] { -1f, 1f })
        {
            foreach (float offset in offsets)
            {
                float variation = RandomRange(-1.2f, 1.2f);

                Vector3 leftRight = new Vector3(
                    side * (half + 5f + variation),
                    groundHeight,
                    offset);

                Vector3 frontBack = new Vector3(
                    offset,
                    groundHeight,
                    side * (half + 5f + variation));

                TrySpawn(
                    treePrefab,
                    leftRight,
                    outerTreeScale,
                    2f,
                    true,
                    2.8f);

                TrySpawn(
                    treePrefab,
                    frontBack,
                    outerTreeScale,
                    2f,
                    true,
                    2.8f);
            }
        }
    }

    private void BuildOuterRocks()
    {
        if (rockPrefab == null)
            return;

        float half = arenaSize * 0.5f;

        float[] offsets = { -20f, -12f, -4f, 4f, 12f, 20f };

        foreach (float side in new float[] { -1f, 1f })
        {
            foreach (float offset in offsets)
            {
                Vector3 alongX = new Vector3(
                    side * (half + 2.5f),
                    groundHeight,
                    offset);

                Vector3 alongZ = new Vector3(
                    offset,
                    groundHeight,
                    side * (half + 2.5f));

                TrySpawn(
                    rockPrefab,
                    alongX,
                    outerRockScale,
                    1.2f,
                    true,
                    1.7f);

                TrySpawn(
                    rockPrefab,
                    alongZ,
                    outerRockScale,
                    1.2f,
                    true,
                    1.7f);
            }
        }
    }

    private bool TrySpawn(
        GameObject prefab,
        Vector3 position,
        float baseScale,
        float variationPercent,
        bool exterior,
        float spacing)
    {
        if (prefab == null)
            return false;

        // Never generate a large interior object.
        float half = arenaSize * 0.5f;

        if (!exterior)
        {
            if (Mathf.Abs(position.x) > half - 1f ||
                Mathf.Abs(position.z) > half - 1f)
                return false;

            // Reserve the central combat area.
            if (Mathf.Abs(position.x) < 18f &&
                Mathf.Abs(position.z) < 18f)
                return false;

            // Avoid the architectural corners.
            if (Mathf.Abs(position.x) > half - 5f &&
                Mathf.Abs(position.z) > half - 5f)
                return false;

            if (HasGameplayObstacle(position))
                return false;
        }

        // Prevent excessive overlap between clusters.
        foreach (Vector3 previous in usedPositions)
        {
            Vector2 a = new Vector2(
                previous.x, previous.z);

            Vector2 b = new Vector2(
                position.x, position.z);

            if (Vector2.Distance(a, b) < spacing)
                return false;
        }

        GameObject instance =
            PrefabUtility.InstantiatePrefab(
                prefab, gameObject.scene) as GameObject;

        if (instance == null)
            return false;

        Undo.RegisterCreatedObjectUndo(
            instance, "Spawn Arena Decoration");

        instance.transform.SetParent(
            generatedRoot, true);

        instance.transform.position = position;

        instance.transform.rotation = Quaternion.Euler(
            0f,
            RandomRange(0f, 360f),
            0f);

        float multiplier = RandomRange(
            1f - variationPercent * 0.15f,
            1f + variationPercent * 0.15f);

        float finalScale = baseScale * multiplier;

        instance.transform.localScale =
            prefab.transform.localScale * finalScale;

        // Align rendered geometry to the configured ground.
        // Prefab pivots may not be at the bottom.
        Renderer[] renderers =
            instance.GetComponentsInChildren<Renderer>(true);

        float lowestPoint = float.PositiveInfinity;

        foreach (Renderer r in renderers)
        {
            if (r.enabled)
                lowestPoint = Mathf.Min(
                    lowestPoint, r.bounds.min.y);
        }

        if (!float.IsInfinity(lowestPoint))
        {
            Vector3 aligned = instance.transform.position;

            aligned.y += groundHeight - lowestPoint;

            instance.transform.position = aligned;
        }

        // Disable colliders only on generated copies.
        Collider[] colliders =
            instance.GetComponentsInChildren<Collider>(true);

        foreach (Collider col in colliders)
            col.enabled = false;

        SetLayerRecursively(instance.transform, 2);

        instance.name = prefab.name + "_Decoration";

        usedPositions.Add(position);
        spawnedCount++;

        return true;
    }

    private bool HasGameplayObstacle(Vector3 position)
    {
        Vector3 checkCenter =
            position + Vector3.up * 0.9f;

        Vector3 halfExtents = new Vector3(
            obstacleClearance,
            0.55f,
            obstacleClearance);

        Collider[] hits = Physics.OverlapBox(
            checkCenter,
            halfExtents,
            Quaternion.identity,
            ~(1 << 2),
            QueryTriggerInteraction.Ignore);

        foreach (Collider col in hits)
        {
            if (!col.enabled)
                continue;

            // Ignore floor colliders that remain below
            // the decoration placement volume.
            if (col.bounds.max.y < groundHeight + 0.3f)
                continue;

            if (generatedRoot != null &&
                col.transform.IsChildOf(generatedRoot))
                continue;

            return true;
        }

        return false;
    }

    private float RandomRange(float min, float max)
    {
        return Mathf.Lerp(
            min,
            max,
            (float)random.NextDouble());
    }

    private void SetLayerRecursively(
        Transform root,
        int targetLayer)
    {
        root.gameObject.layer = targetLayer;

        foreach (Transform child in root)
            SetLayerRecursively(child, targetLayer);
    }

#endif
}
