
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class ArenaVisualBuilder : MonoBehaviour
{
#if UNITY_EDITOR

    private Transform generatedRoot;

    [ContextMenu("Build Arena Visuals")]
    public void BuildArena()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning(
                "Exit Play Mode before rebuilding arena visuals.");
            return;
        }

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

        Material stoneLight = GetMaterial(
            "StoneLight", new Color32(200, 190, 202, 255));

        Material blueMetal = GetMaterial(
            "BlueMetal", new Color32(54, 102, 159, 255));

        Material darkMetal = GetMaterial(
            "DarkMetal", new Color32(39, 56, 91, 255));

        Material gold = GetMaterial(
            "GoldTrim", new Color32(235, 172, 100, 255));

        Material crystal = GetMaterial(
            "Crystal", new Color32(49, 221, 255, 255), true);

        Material blueGlow = GetMaterial(
            "BlueGlow", new Color32(24, 181, 255, 255), true);

        Material grass = GetMaterial(
            "Grass", new Color32(95, 174, 99, 255));

        Material grassLight = GetMaterial(
            "GrassLight", new Color32(139, 202, 96, 255));

        BuildFloor(stone, stoneAlt, stoneLight);
        BuildCovers(blueMetal, darkMetal, gold, blueGlow);
        BuildCrystal(stone, gold, crystal, blueGlow);
        BuildGrass(grass, grassLight);
        BuildArenaEdges(stoneAlt, darkMetal, gold, blueGlow);

        EditorUtility.SetDirty(gameObject);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);

        AssetDatabase.SaveAssets();

        Debug.Log(
            "PrettyBattle Ders 32: Arena visuals created!");
    }

    private void BuildFloor(
        Material stone,
        Material alternate,
        Material light)
    {
        float tileSize = 5f;

        for (int x = 0; x < 10; x++)
        {
            for (int z = 0; z < 10; z++)
            {
                float px = -22.5f + x * tileSize;
                float pz = -22.5f + z * tileSize;

                Material tileMaterial =
                    (x + z) % 3 == 0 ? alternate : stone;

                CreateBlock(
                    "FloorTile",
                    new Vector3(px, 0.025f, pz),
                    new Vector3(4.88f, 0.05f, 4.88f),
                    tileMaterial);

                // Raised, inset stone slab for surface depth.
                CreateBlock(
                    "FloorInset",
                    new Vector3(px, 0.057f, pz),
                    new Vector3(4.54f, 0.016f, 4.54f),
                    tileMaterial);

                // A few lighter decorative stone tiles.
                if ((x + z * 3) % 11 == 0)
                {
                    CreateBlock(
                        "StoneAccent",
                        new Vector3(px, 0.069f, pz),
                        new Vector3(3.7f, 0.009f, 3.7f),
                        light);
                }
            }
        }
    }

    private void BuildCovers(
        Material metal,
        Material dark,
        Material trim,
        Material glow)
    {
        float[] positions = { -10f, 10f };
        float[] rows = { -9f, 9f };

        foreach (float x in positions)
        {
            foreach (float z in rows)
            {
                CreateBlock(
                    "CoverFrontPanel",
                    new Vector3(x, 0.8f, z - 1.03f),
                    new Vector3(3.6f, 1.05f, 0.09f),
                    metal);

                CreateBlock(
                    "CoverBackPanel",
                    new Vector3(x, 0.8f, z + 1.03f),
                    new Vector3(3.6f, 1.05f, 0.09f),
                    metal);

                CreateBlock(
                    "CoverTopTrim",
                    new Vector3(x, 1.54f, z),
                    new Vector3(4.15f, 0.12f, 2.15f),
                    trim);

                foreach (float side in new float[] { -1f, 1f })
                {
                    float faceZ = z + side * 1.1f;

                    CreateBlock(
                        "CoverDarkInset",
                        new Vector3(x, 0.8f, faceZ),
                        new Vector3(2.9f, 0.7f, 0.035f),
                        dark);

                    CreateBlock(
                        "CoverGlowStrip",
                        new Vector3(x, 1.14f,
                            faceZ + side * 0.025f),
                        new Vector3(2.5f, 0.09f, 0.035f),
                        glow);

                    foreach (float column in
                        new float[] { -1.68f, 1.68f })
                    {
                        CreateBlock(
                            "CoverVerticalTrim",
                            new Vector3(x + column, 0.8f,
                                faceZ + side * 0.04f),
                            new Vector3(0.12f, 1.12f, 0.06f),
                            trim);
                    }
                }

                CreateBlock(
                    "CoverTopPlate",
                    new Vector3(x, 1.62f, z),
                    new Vector3(3.5f, 0.045f, 1.7f),
                    dark);

                foreach (float side in new float[] { -1f, 1f })
                {
                    CreateBlock(
                        "CoverCornerCap",
                        new Vector3(x + side * 1.95f,
                            1.48f, z),
                        new Vector3(0.21f, 0.24f, 2.17f),
                        trim);
                }
            }
        }
    }

    private void BuildCrystal(
        Material stone,
        Material gold,
        Material crystal,
        Material glow)
    {
        CreateBlock(
            "CrystalBase",
            new Vector3(0f, 2.16f, 0f),
            new Vector3(3.0f, 0.32f, 3.0f),
            stone);

        CreateBlock(
            "CrystalGoldRing",
            new Vector3(0f, 2.38f, 0f),
            new Vector3(2.6f, 0.12f, 2.6f),
            gold);

        CreateBlock(
            "CrystalGlowPlatform",
            new Vector3(0f, 2.455f, 0f),
            new Vector3(2.3f, 0.065f, 2.3f),
            glow);

        CreateBlock(
            "CrystalUpperPlate",
            new Vector3(0f, 2.52f, 0f),
            new Vector3(1.85f, 0.1f, 1.85f),
            stone);

        // A proper double-pyramid crystal mesh.
        GameObject gem = new GameObject("MainCrystal");
        gem.transform.SetParent(generatedRoot, false);
        gem.transform.localPosition = new Vector3(0f, 3.75f, 0f);
        gem.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
        gem.transform.localScale = Vector3.one;
        gem.layer = 2;

        MeshFilter filter = gem.AddComponent<MeshFilter>();
        MeshRenderer renderer = gem.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        mesh.name = "PrettyBattleCrystalMesh";

        // Independent triangles give the crystal sharp facets.
        Vector3 top = new Vector3(0f, 1.2f, 0f);
        Vector3 bottom = new Vector3(0f, -1.2f, 0f);

        Vector3[] ring =
        {
            new Vector3(-0.8f, 0f, -0.8f),
            new Vector3(-0.8f, 0f, 0.8f),
            new Vector3(0.8f, 0f, 0.8f),
            new Vector3(0.8f, 0f, -0.8f)
        };

        Vector3[] vertices = new Vector3[24];
        int[] triangles = new int[24];

        int index = 0;

        for (int i = 0; i < 4; i++)
        {
            int next = (i + 1) % 4;

            // Upper face.
            vertices[index] = top;
            vertices[index + 1] = ring[i];
            vertices[index + 2] = ring[next];

            triangles[index] = index;
            triangles[index + 1] = index + 1;
            triangles[index + 2] = index + 2;
            index += 3;

            // Lower face.
            vertices[index] = bottom;
            vertices[index + 1] = ring[next];
            vertices[index + 2] = ring[i];

            triangles[index] = index;
            triangles[index + 1] = index + 1;
            triangles[index + 2] = index + 2;
            index += 3;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        filter.sharedMesh = mesh;
        renderer.sharedMaterial = crystal;

        CreateBlock(
            "CrystalCore",
            new Vector3(0f, 3.75f, 0f),
            new Vector3(0.33f, 0.33f, 0.33f),
            glow);
    }

    private void BuildGrass(
        Material grass,
        Material grassLight)
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
            CreateShape(
                "GrassPatch",
                PrimitiveType.Sphere,
                position,
                new Vector3(2f, 0.25f, 1.4f),
                grass);

            for (int i = 0; i < 3; i++)
            {
                float angle = i * Mathf.PI * 2f / 3f;

                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * 0.45f,
                    0.14f,
                    Mathf.Sin(angle) * 0.3f);

                CreateShape(
                    "GrassLeafCluster",
                    PrimitiveType.Sphere,
                    position + offset,
                    new Vector3(0.5f, 0.4f, 0.55f),
                    grassLight);
            }
        }
    }

    private void BuildArenaEdges(
        Material stone,
        Material dark,
        Material trim,
        Material glow)
    {
        // Decorative strips sit close to the existing arena walls.
        // No new collidable walls are generated.
        foreach (float side in new float[] { -1f, 1f })
        {
            CreateBlock(
                "SideWallDarkBand",
                new Vector3(side * 24.1f, 0.35f, 0f),
                new Vector3(0.12f, 0.38f, 42f),
                dark);

            CreateBlock(
                "SideWallGlowStrip",
                new Vector3(side * 24.1f, 0.58f, 0f),
                new Vector3(0.15f, 0.085f, 42f),
                glow);

            CreateBlock(
                "EndWallDarkBand",
                new Vector3(0f, 0.35f, side * 24.1f),
                new Vector3(42f, 0.38f, 0.12f),
                dark);

            CreateBlock(
                "EndWallGlowStrip",
                new Vector3(0f, 0.58f, side * 24.1f),
                new Vector3(42f, 0.085f, 0.15f),
                glow);
        }

        // Corner architectural details.
        foreach (float x in new float[] { -22f, 22f })
        {
            foreach (float z in new float[] { -22f, 22f })
            {
                CreateBlock(
                    "CornerPillarBase",
                    new Vector3(x, 0.22f, z),
                    new Vector3(1.7f, 0.44f, 1.7f),
                    stone);

                CreateBlock(
                    "CornerPillar",
                    new Vector3(x, 1.1f, z),
                    new Vector3(1.15f, 1.6f, 1.15f),
                    stone);

                CreateBlock(
                    "CornerPillarCap",
                    new Vector3(x, 2f, z),
                    new Vector3(1.55f, 0.2f, 1.55f),
                    trim);

                CreateBlock(
                    "CornerPillarGlow",
                    new Vector3(x, 1.8f, z),
                    new Vector3(1.22f, 0.12f, 1.22f),
                    glow);
            }
        }
    }

    private GameObject CreateBlock(
        string objectName,
        Vector3 position,
        Vector3 scale,
        Material material)
    {
        return CreateShape(
            objectName,
            PrimitiveType.Cube,
            position,
            scale,
            material);
    }

    private GameObject CreateShape(
        string objectName,
        PrimitiveType primitive,
        Vector3 position,
        Vector3 scale,
        Material material)
    {
        GameObject obj = GameObject.CreatePrimitive(primitive);

        obj.name = objectName;
        obj.transform.SetParent(generatedRoot, false);
        obj.transform.localPosition = position;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = scale;

        obj.GetComponent<Renderer>().sharedMaterial = material;

        // All generated pieces are visual-only.
        RemoveCollider(obj);
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
                "Universal Render Pipeline/Lit");

            if (shader == null)
                shader = Shader.Find("Standard");

            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }

        mat.color = color;

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);

        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", glow ? 0.7f : 0.25f);

        if (glow)
        {
            mat.EnableKeyword("_EMISSION");

            if (mat.HasProperty("_EmissionColor"))
                mat.SetColor("_EmissionColor", color * 2f);

            mat.globalIlluminationFlags =
                MaterialGlobalIlluminationFlags
                    .RealtimeEmissive;
        }
        else
        {
            mat.DisableKeyword("_EMISSION");

            if (mat.HasProperty("_EmissionColor"))
                mat.SetColor("_EmissionColor", Color.black);
        }

        EditorUtility.SetDirty(mat);
        return mat;
    }

#endif
}
