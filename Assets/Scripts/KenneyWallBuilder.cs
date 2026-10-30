
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class KenneyWallBuilder : MonoBehaviour
{
    [Header("Kenney Asset")]
    public GameObject wallPrefab;

    [Header("Original Arena Walls")]
    public Transform northWall;
    public Transform southWall;
    public Transform eastWall;
    public Transform westWall;

    [Header("Corner Towers")]
    public Transform northWest;
    public Transform northEast;
    public Transform southEast;
    public Transform southWest;

    [Header("Kenney Wall Scale")]
    public Vector3 wallScale = new Vector3(4f, 9f, 4f);

    [Header("Placement")]
    public float groundY = 0f;

    [Tooltip("How far each wall ends before the tower pivot.")]
    public float cornerInset = 2.2f;

    [Tooltip("Moves the visible wall slightly toward the arena.")]
    public float innerFaceOffset = 0.3f;

#if UNITY_EDITOR

    private const string GeneratedName =
        "GeneratedKenneyCastleWalls";

    [ContextMenu("Build All Castle Walls")]
    public void BuildAllWalls()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "Exit Play Mode before building walls.");
            return;
        }

        if (!ValidateReferences())
            return;

        if (wallScale.x <= 0f ||
            wallScale.y <= 0f ||
            wallScale.z <= 0f)
        {
            Debug.LogError("Wall scale must be positive.");
            return;
        }

        Transform old = transform.Find(GeneratedName);

        if (old != null)
            Undo.DestroyObjectImmediate(old.gameObject);

        GameObject root = new GameObject(GeneratedName);

        Undo.RegisterCreatedObjectUndo(
            root, "Create Kenney Castle Walls");

        root.transform.SetParent(transform, false);

        // NORTH: North-West -> North-East
        BuildSide(
            root.transform,
            "North",
            northWall,
            northWest.position.x,
            northEast.position.x,
            270f,
            true,
            true
        );

        // SOUTH: South-West -> South-East
        BuildSide(
            root.transform,
            "South",
            southWall,
            southWest.position.x,
            southEast.position.x,
            90f,
            true,
            false
        );

        // EAST: South-East -> North-East
        BuildSide(
            root.transform,
            "East",
            eastWall,
            southEast.position.z,
            northEast.position.z,
            0f,
            false,
            true
        );

        // WEST: South-West -> North-West
        BuildSide(
            root.transform,
            "West",
            westWall,
            southWest.position.z,
            northWest.position.z,
            180f,
            false,
            false
        );

        // Disable the earlier north-wall visual test.
        Transform oldNorth =
            transform.Find("GeneratedKenneyNorthWall");

        if (oldNorth != null)
        {
            Undo.RecordObject(
                oldNorth.gameObject,
                "Hide Old Kenney North Wall"
            );

            oldNorth.gameObject.SetActive(false);
        }

        // Hide the earlier cube-based decorations.
        ArenaWallBuilder oldBuilder =
            FindFirstObjectByType<ArenaWallBuilder>();

        if (oldBuilder != null)
        {
            Transform previousDecoration =
                oldBuilder.transform.Find(
                    "GeneratedStoneWalls");

            if (previousDecoration != null)
            {
                Undo.RecordObject(
                    previousDecoration.gameObject,
                    "Hide Old Stone Walls"
                );

                previousDecoration.gameObject.SetActive(false);
            }
        }

        SetOriginalWallVisuals(false);

        EditorSceneManager.MarkSceneDirty(gameObject.scene);

        Debug.Log(
            "PrettyBattle: Four Kenney castle walls generated."
        );
    }

    private void BuildSide(
        Transform root,
        string sideName,
        Transform originalWall,
        float firstCornerCoordinate,
        float secondCornerCoordinate,
        float rotationY,
        bool alongX,
        bool positiveSide)
    {
        float start = Mathf.Min(
            firstCornerCoordinate,
            secondCornerCoordinate
        ) + cornerInset;

        float end = Mathf.Max(
            firstCornerCoordinate,
            secondCornerCoordinate
        ) - cornerInset;

        float availableLength = end - start;

        if (availableLength <= 0f)
        {
            Debug.LogError(
                sideName + ": Corner spacing is invalid.");
            return;
        }

        GameObject sample = InstantiatePrefab(
            null, "TemporaryWall");

        if (sample == null)
            return;

        sample.transform.position = Vector3.zero;
        sample.transform.rotation =
            Quaternion.Euler(0f, rotationY, 0f);

        sample.transform.localScale = wallScale;

        Bounds sampleBounds = GetVisualBounds(sample);

        float originalLength = alongX
            ? sampleBounds.size.x
            : sampleBounds.size.z;

        DestroyImmediate(sample);

        if (originalLength <= 0.001f)
        {
            Debug.LogError(
                sideName + ": Invalid Kenney mesh bounds.");
            return;
        }

        // Fit an integer number of wall pieces
        // between the two corner tower connection points.
        int count = Mathf.Max(
            1,
            Mathf.RoundToInt(
                availableLength / originalLength)
        );

        float step = availableLength / count;

        // All chosen rotations place the model's
        // local Z axis along the wall.
        float fitFactor = step / originalLength;

        Vector3 fittedScale = wallScale;
        fittedScale.z *= fitFactor;

        GameObject sideGroup = new GameObject(
            "CastleWall_" + sideName);

        Undo.RegisterCreatedObjectUndo(
            sideGroup,
            "Create " + sideName + " Wall"
        );

        sideGroup.transform.SetParent(root, false);

        BoxCollider boundary =
            originalWall.GetComponent<BoxCollider>();

        Bounds boundaryBounds = boundary.bounds;

        float innerFace;

        if (alongX)
        {
            innerFace = positiveSide
                ? boundaryBounds.min.z - innerFaceOffset
                : boundaryBounds.max.z + innerFaceOffset;
        }
        else
        {
            innerFace = positiveSide
                ? boundaryBounds.min.x - innerFaceOffset
                : boundaryBounds.max.x + innerFaceOffset;
        }

        for (int i = 0; i < count; i++)
        {
            GameObject piece = InstantiatePrefab(
                sideGroup.transform,
                sideName + "_Wall_" + (i + 1)
            );

            if (piece == null)
                continue;

            piece.transform.position = Vector3.zero;
            piece.transform.rotation =
                Quaternion.Euler(0f, rotationY, 0f);

            piece.transform.localScale = fittedScale;

            Bounds bounds = GetVisualBounds(piece);

            float coordinate =
                start + (i + 0.5f) * step;

            Vector3 positionAdjustment = Vector3.zero;

            if (alongX)
            {
                positionAdjustment.x =
                    coordinate - bounds.center.x;

                positionAdjustment.z = positiveSide
                    ? innerFace - bounds.min.z
                    : innerFace - bounds.max.z;
            }
            else
            {
                positionAdjustment.z =
                    coordinate - bounds.center.z;

                positionAdjustment.x = positiveSide
                    ? innerFace - bounds.min.x
                    : innerFace - bounds.max.x;
            }

            positionAdjustment.y =
                groundY - bounds.min.y;

            piece.transform.position +=
                positionAdjustment;

            // The original arena wall keeps its collider.
            // New Kenney pieces are visual only.
            foreach (Collider collider in
                piece.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
            }

            foreach (Transform part in
                piece.GetComponentsInChildren<Transform>(true))
            {
                part.gameObject.layer = 2;
            }
        }

        Debug.Log(
            sideName + " wall: " + count +
            " pieces. Length fit: " +
            fitFactor.ToString("F3")
        );

        if (Mathf.Abs(fitFactor - 1f) > 0.15f)
        {
            Debug.LogWarning(
                sideName +
                ": Wall pieces were resized noticeably. " +
                "Check their proportions in Scene view."
            );
        }
    }

    private bool ValidateReferences()
    {
        if (wallPrefab == null ||
            northWall == null ||
            southWall == null ||
            eastWall == null ||
            westWall == null ||
            northWest == null ||
            northEast == null ||
            southEast == null ||
            southWest == null)
        {
            Debug.LogError(
                "KenneyWallBuilder: Assign all references."
            );
            return false;
        }

        Transform[] walls =
        {
            northWall,
            southWall,
            eastWall,
            westWall
        };

        foreach (Transform wall in walls)
        {
            if (wall.GetComponent<BoxCollider>() == null)
            {
                Debug.LogError(
                    wall.name + " needs a BoxCollider."
                );
                return false;
            }
        }

        return true;
    }

    private GameObject InstantiatePrefab(
        Transform parent,
        string objectName)
    {
        GameObject instance =
            PrefabUtility.InstantiatePrefab(
                wallPrefab) as GameObject;

        if (instance == null)
        {
            Debug.LogError(
                "Failed to instantiate Kenney wall.");
            return null;
        }

        instance.name = objectName;

        if (parent != null)
        {
            instance.transform.SetParent(
                parent, true);
        }

        return instance;
    }

    private Bounds GetVisualBounds(GameObject obj)
    {
        Renderer[] renderers =
            obj.GetComponentsInChildren<Renderer>(true);

        if (renderers.Length == 0)
            return new Bounds(
                obj.transform.position,
                Vector3.zero
            );

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private void SetOriginalWallVisuals(bool visible)
    {
        Transform[] walls =
        {
            northWall,
            southWall,
            eastWall,
            westWall
        };

        foreach (Transform wall in walls)
        {
            MeshRenderer renderer =
                wall.GetComponent<MeshRenderer>();

            if (renderer != null)
            {
                Undo.RecordObject(
                    renderer,
                    "Update Original Wall Visibility");

                renderer.enabled = visible;
            }

            // Physical boundaries must stay active.
            BoxCollider collider =
                wall.GetComponent<BoxCollider>();

            if (collider != null)
                collider.enabled = true;
        }
    }

    [ContextMenu("Restore Original Wall Visuals")]
    public void RestoreOriginalWalls()
    {
        Transform generated =
            transform.Find(GeneratedName);

        if (generated != null)
            generated.gameObject.SetActive(false);

        SetOriginalWallVisuals(true);

        EditorSceneManager.MarkSceneDirty(gameObject.scene);

        Debug.Log("Original wall visuals restored.");
    }

#endif
}
