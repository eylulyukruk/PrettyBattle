
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class ArenaWallBuilder : MonoBehaviour
{
    [Header("Existing Arena")]
    public Transform arenaEnvironment;

    [Header("Stone Wall Design")]
    public float blockLength = 4f;
    public int stoneRows = 3;
    public float pillarSpacing = 10f;
    public float decorationDepth = 1.06f;

#if UNITY_EDITOR

    private Transform generatedRoot;

    [ContextMenu("Build Stone Arena Walls")]
    public void BuildWalls()
    {
        if (arenaEnvironment == null)
        {
            Debug.LogError(
                "Assign ArenaEnvironment in the Inspector."
            );
            return;
        }

        string[] names =
        {
            "Wall_North",
            "Wall_South",
            "Wall_East",
            "Wall_West"
        };

        foreach (string name in names)
        {
            if (arenaEnvironment.Find(name) == null)
            {
                Debug.LogError("Wall not found: " + name);
                return;
            }
        }

        Transform previous =
            transform.Find("GeneratedStoneWalls");

        if (previous != null)
            DestroyImmediate(previous.gameObject);

        GameObject root =
            new GameObject("GeneratedStoneWalls");

        root.transform.SetParent(transform, false);
        generatedRoot = root.transform;

        Material stone = LoadMaterial(
            "PB_Stone",
            new Color(0.63f, 0.62f, 0.68f)
        );

        Material alternate = LoadMaterial(
            "PB_StoneAlt",
            new Color(0.52f, 0.55f, 0.62f)
        );

        Material trim = LoadMaterial(
            "PB_StoneLight",
            new Color(0.77f, 0.74f, 0.72f)
        );

        Material gold = LoadMaterial(
            "PB_GoldTrim",
            new Color(0.90f, 0.67f, 0.40f)
        );

        foreach (string name in names)
        {
            Transform wall = arenaEnvironment.Find(name);

            BuildWall(wall, stone, alternate, trim, gold);
        }

        // Hide original blue geometry, but retain its collider.
        foreach (string name in names)
        {
            Transform wall = arenaEnvironment.Find(name);
            MeshRenderer renderer =
                wall.GetComponent<MeshRenderer>();

            if (renderer != null)
            {
                Undo.RecordObject(renderer, "Hide Blue Wall");
                renderer.enabled = false;
            }
        }

        EditorSceneManager.MarkSceneDirty(gameObject.scene);

        Debug.Log(
            "PrettyBattle: 4 stone arena walls generated."
        );
    }

    private void BuildWall(
        Transform wall,
        Material stone,
        Material alternate,
        Material trim,
        Material gold)
    {
        Vector3 center = wall.position;
        Vector3 dimensions = wall.lossyScale;

        bool horizontal =
            Mathf.Abs(dimensions.x) >
            Mathf.Abs(dimensions.z);

        float length = horizontal
            ? Mathf.Abs(dimensions.x)
            : Mathf.Abs(dimensions.z);

        float height = Mathf.Abs(dimensions.y);

        int segments = Mathf.Max(
            1,
            Mathf.CeilToInt(length / blockLength)
        );

        int rows = Mathf.Max(1, stoneRows);

        float segmentLength = length / segments;
        float rowHeight = height / rows;

        // Stone wall blocks.
        for (int row = 0; row < rows; row++)
        {
            for (int index = 0; index < segments; index++)
            {
                float offset =
                    -length / 2f +
                    segmentLength * (index + 0.5f);

                Vector3 position = center;

                if (horizontal)
                    position.x += offset;
                else
                    position.z += offset;

                position.y =
                    center.y - height / 2f +
                    rowHeight * (row + 0.5f);

                Vector3 size = horizontal
                    ? new Vector3(
                        segmentLength - 0.06f,
                        rowHeight - 0.05f,
                        decorationDepth)
                    : new Vector3(
                        decorationDepth,
                        rowHeight - 0.05f,
                        segmentLength - 0.06f);

                CreateBlock(
                    "StoneBlock",
                    position,
                    size,
                    (index + row) % 3 == 0
                        ? alternate
                        : stone
                );
            }
        }

        // Decorative top edge.
        Vector3 capPosition = center;
        capPosition.y =
            center.y + height / 2f + 0.12f;

        Vector3 capSize = horizontal
            ? new Vector3(length + 0.25f, 0.24f, 1.35f)
            : new Vector3(1.35f, 0.24f, length + 0.25f);

        CreateBlock(
            "StoneWall_TopCap",
            capPosition,
            capSize,
            trim
        );

        // Short golden line below the top edge.
        Vector3 goldPosition = center;

        goldPosition.y =
            center.y + height / 2f - 0.18f;

        if (horizontal)
            goldPosition.z += center.z > 0f ? -0.58f : 0.58f;
        else
            goldPosition.x += center.x > 0f ? -0.58f : 0.58f;

        Vector3 goldSize = horizontal
            ? new Vector3(length - 1f, 0.10f, 0.07f)
            : new Vector3(0.07f, 0.10f, length - 1f);

        CreateBlock(
            "Wall_GoldAccent",
            goldPosition,
            goldSize,
            gold
        );

        // Decorative pillars at regular intervals.
        int pillarCount = Mathf.Max(
            2,
            Mathf.RoundToInt(length / pillarSpacing) + 1
        );

        for (int i = 0; i < pillarCount; i++)
        {
            float offset =
                -length / 2f +
                length * i / (pillarCount - 1);

            Vector3 pillarPosition = center;

            if (horizontal)
                pillarPosition.x += offset;
            else
                pillarPosition.z += offset;

            Vector3 pillarSize = horizontal
                ? new Vector3(0.8f, height + 0.25f, 1.35f)
                : new Vector3(1.35f, height + 0.25f, 0.8f);

            CreateBlock(
                "StonePillar",
                pillarPosition,
                pillarSize,
                alternate
            );

            Vector3 top = pillarPosition;
            top.y = center.y + height / 2f + 0.25f;

            CreateBlock(
                "PillarCap",
                top,
                new Vector3(1.3f, 0.35f, 1.3f),
                trim
            );
        }
    }

    private GameObject CreateBlock(
        string name,
        Vector3 position,
        Vector3 size,
        Material material)
    {
        GameObject block =
            GameObject.CreatePrimitive(PrimitiveType.Cube);

        block.name = name;
        block.transform.SetParent(generatedRoot, false);
        block.transform.position = position;
        block.transform.localScale = size;

        block.GetComponent<Renderer>().sharedMaterial =
            material;

        Collider collider = block.GetComponent<Collider>();

        if (collider != null)
            DestroyImmediate(collider);

        // Ignore Raycast.
        block.layer = 2;

        return block;
    }

    private Material LoadMaterial(
        string name,
        Color fallbackColor)
    {
        string path = "Assets/Materials/" + name + ".mat";

        Material existing =
            AssetDatabase.LoadAssetAtPath<Material>(path);

        if (existing != null)
            return existing;

        Shader shader = Shader.Find(
            "Universal Render Pipeline/Lit"
        );

        if (shader == null)
            shader = Shader.Find("Standard");

        Material material = new Material(shader);
        material.color = fallbackColor;

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", fallbackColor);

        AssetDatabase.CreateAsset(material, path);

        return material;
    }

#endif
}
