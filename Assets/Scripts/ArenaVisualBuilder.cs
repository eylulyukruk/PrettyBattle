
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ArenaVisualBuilder : MonoBehaviour
{
#if UNITY_EDITOR

    private Transform generatedRoot;

    [ContextMenu("Build Arena Visuals")]
    public void BuildArena()
    {
        Transform oldRoot = transform.Find("GeneratedVisuals");

        if (oldRoot != null)
            DestroyImmediate(oldRoot.gameObject);

        GameObject root = new GameObject("GeneratedVisuals");
        root.transform.SetParent(transform, false);
        generatedRoot = root.transform;

        Material stone = GetMaterial(
            "Stone", new Color32(166, 160, 184, 255));

        Material stoneAlt = GetMaterial(
            "StoneAlt", new Color32(138, 148, 176, 255));

        Material blueMetal = GetMaterial(
            "BlueMetal", new Color32(54, 102, 159, 255));

        Material gold = GetMaterial(
            "GoldTrim", new Color32(235, 172, 100, 255));

        Material crystal = GetMaterial(
            "Crystal", new Color32(49, 221, 255, 255), true);

        Material grass = GetMaterial(
            "Grass", new Color32(95, 174, 99, 255));

        BuildFloor(stone, stoneAlt);
        BuildCovers(blueMetal, gold);
        BuildCrystal(stone, gold, crystal);
        BuildGrass(grass);

        EditorUtility.SetDirty(gameObject);
        EditorSceneManagerMarkDirty();

        Debug.Log("PrettyBattle arena visuals created!");
    }

    private void BuildFloor(Material stone, Material alternate)
    {
        float tileSize = 5f;

        for (int x = 0; x < 10; x++)
        {
            for (int z = 0; z < 10; z++)
            {
                float px = -22.5f + x * tileSize;
                float pz = -22.5f + z * tileSize;

                CreateBlock(
                    "FloorTile",
                    new Vector3(px, 0.025f, pz),
                    new Vector3(4.88f, 0.05f, 4.88f),
                    (x + z) % 3 == 0 ? alternate : stone
                );
            }
        }
    }

    private void BuildCovers(Material metal, Material trim)
    {
        float[] positions = { -10f, 10f };
        float[] rows = { -9f, 9f };

        foreach (float x in positions)
        {
            foreach (float z in rows)
            {
                // Decorative panels on existing cover blocks.
                CreateBlock(
                    "CoverFrontPanel",
                    new Vector3(x, 0.8f, z - 1.03f),
                    new Vector3(3.6f, 1.05f, 0.09f),
                    metal
                );

                CreateBlock(
                    "CoverBackPanel",
                    new Vector3(x, 0.8f, z + 1.03f),
                    new Vector3(3.6f, 1.05f, 0.09f),
                    metal
                );

                CreateBlock(
                    "CoverTopTrim",
                    new Vector3(x, 1.54f, z),
                    new Vector3(4.15f, 0.12f, 2.15f),
                    trim
                );

                CreateBlock(
                    "CoverFrontStripe",
                    new Vector3(x, 0.8f, z - 1.09f),
                    new Vector3(3.2f, 0.12f, 0.05f),
                    trim
                );
            }
        }
    }

    private void BuildCrystal(
        Material stone,
        Material gold,
        Material crystal)
    {
        // Decorative pedestal above the central obstacle.
        CreateBlock(
            "CrystalBase",
            new Vector3(0f, 2.16f, 0f),
            new Vector3(3f, 0.32f, 3f),
            stone
        );

        CreateBlock(
            "CrystalGoldRing",
            new Vector3(0f, 2.38f, 0f),
            new Vector3(2.25f, 0.12f, 2.25f),
            gold
        );

        GameObject gem = CreateBlock(
            "MainCrystal",
            new Vector3(0f, 3.75f, 0f),
            new Vector3(1.4f, 2f, 1.4f),
            crystal
        );

        gem.transform.rotation =
            Quaternion.Euler(0f, 45f, 45f);

        CreateBlock(
            "CrystalCore",
            new Vector3(0f, 3.75f, 0f),
            new Vector3(0.65f, 0.65f, 0.65f),
            crystal
        );
    }

    private void BuildGrass(Material grass)
    {
        Vector3[] patches =
        {
            new Vector3(-21f, 0.14f, -15f),
            new Vector3(21f, 0.14f, -15f),
            new Vector3(-21f, 0.14f, 15f),
            new Vector3(21f, 0.14f, 15f),
            new Vector3(-15f, 0.14f, -19f),
            new Vector3(15f, 0.14f, 19f)
        };

        foreach (Vector3 position in patches)
        {
            GameObject patch = GameObject.CreatePrimitive(
                PrimitiveType.Sphere
            );

            patch.name = "GrassPatch";
            patch.transform.SetParent(generatedRoot, false);
            patch.transform.position = position;
            patch.transform.localScale =
                new Vector3(2f, 0.25f, 1.4f);

            patch.GetComponent<Renderer>().sharedMaterial = grass;

            RemoveCollider(patch);
            patch.layer = 2;
        }
    }

    private GameObject CreateBlock(
        string objectName,
        Vector3 position,
        Vector3 scale,
        Material material)
    {
        GameObject obj = GameObject.CreatePrimitive(
            PrimitiveType.Cube
        );

        obj.name = objectName;
        obj.transform.SetParent(generatedRoot, false);
        obj.transform.position = position;
        obj.transform.localScale = scale;

        obj.GetComponent<Renderer>().sharedMaterial = material;

        // Decorations must not affect player movement.
        RemoveCollider(obj);

        // Keep visual pieces out of physics raycasts.
        obj.layer = 2;

        return obj;
    }

    private void RemoveCollider(GameObject obj)
    {
        Collider col = obj.GetComponent<Collider>();

        if (col != null)
            DestroyImmediate(col);
    }

    private Material GetMaterial(
        string materialName,
        Color color,
        bool glow = false)
    {
        string folder = "Assets/Materials";

        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets", "Materials");

        string path = folder + "/PB_" + materialName + ".mat";

        Material mat =
            AssetDatabase.LoadAssetAtPath<Material>(path);

        if (mat == null)
        {
            Shader shader = Shader.Find(
                "Universal Render Pipeline/Lit"
            );

            if (shader == null)
                shader = Shader.Find("Standard");

            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }

        mat.color = color;

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);

        if (glow)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 2f);
        }

        EditorUtility.SetDirty(mat);
        AssetDatabase.SaveAssets();

        return mat;
    }

    private void EditorSceneManagerMarkDirty()
    {
        UnityEditor.SceneManagement.EditorSceneManager
            .MarkSceneDirty(gameObject.scene);
    }

#endif
}
