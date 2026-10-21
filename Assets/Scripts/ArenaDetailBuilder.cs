
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class ArenaDetailBuilder : MonoBehaviour
{
#if UNITY_EDITOR

    private Transform generatedRoot;

    [ContextMenu("Build Arena Details")]
    public void BuildDetails()
    {
        Transform old = transform.Find("GeneratedDetails");

        if (old != null)
            DestroyImmediate(old.gameObject);

        GameObject root = new GameObject("GeneratedDetails");
        root.transform.SetParent(transform, false);
        generatedRoot = root.transform;

        Material metal = GetMaterial("PB_BlueMetal");
        Material gold = GetMaterial("PB_GoldTrim");
        Material stone = GetMaterial("PB_Stone");
        Material cyan = GetMaterial("PB_Crystal");

        BuildAllCrates(metal, gold, cyan);
        BuildFloorDecorations(stone, gold);
        BuildArenaPillars(stone, gold);

        EditorSceneManager.MarkSceneDirty(gameObject.scene);

        Debug.Log("PrettyBattle arena details created!");
    }

    private void BuildAllCrates(
        Material metal,
        Material gold,
        Material cyan)
    {
        float[] xs = { -10f, 10f };
        float[] zs = { -9f, 9f };

        foreach (float x in xs)
        {
            foreach (float z in zs)
            {
                BuildCrate(x, z, metal, gold, cyan);
            }
        }
    }

    private void BuildCrate(
        float x,
        float z,
        Material metal,
        Material gold,
        Material cyan)
    {
        // Decorative face panels on both sides.
        foreach (float side in new float[] { -1f, 1f })
        {
            float faceZ = z + side * 1.12f;

            Block(
                "Crate_Face",
                new Vector3(x, 0.8f, faceZ),
                new Vector3(3.5f, 1.08f, 0.06f),
                metal
            );

            // Decorative diagonal braces.
            for (int i = 0; i < 2; i++)
            {
                GameObject brace = Block(
                    "Crate_Brace",
                    new Vector3(x, 0.8f, faceZ + side * 0.04f),
                    new Vector3(2.65f, 0.12f, 0.08f),
                    gold
                );

                brace.transform.rotation =
                    Quaternion.Euler(
                        0f, 0f, i == 0 ? 21f : -21f
                    );
            }

            // Central decorative badge.
            GameObject badge = GameObject.CreatePrimitive(
                PrimitiveType.Cylinder
            );

            badge.name = "Crate_Badge";
            badge.transform.SetParent(generatedRoot, false);
            badge.transform.position =
                new Vector3(x, 0.8f, faceZ + side * 0.11f);

            badge.transform.rotation =
                Quaternion.Euler(90f, 0f, 0f);

            badge.transform.localScale =
                new Vector3(0.25f, 0.035f, 0.25f);

            badge.GetComponent<Renderer>().sharedMaterial = cyan;
            RemoveCollider(badge);
            badge.layer = 2;

            // Four small corner bolts.
            foreach (float dx in new float[] { -1.55f, 1.55f })
            {
                foreach (float dy in new float[] { -0.4f, 0.4f })
                {
                    Block(
                        "Crate_Bolt",
                        new Vector3(
                            x + dx,
                            0.8f + dy,
                            faceZ + side * 0.12f
                        ),
                        new Vector3(0.17f, 0.17f, 0.08f),
                        gold
                    );
                }
            }
        }

        // Metallic side posts.
        foreach (float dx in new float[] { -1.95f, 1.95f })
        {
            Block(
                "Crate_CornerPost",
                new Vector3(x + dx, 0.8f, z),
                new Vector3(0.17f, 1.55f, 2.2f),
                gold
            );
        }
    }

    private void BuildFloorDecorations(
        Material stone,
        Material gold)
    {
        // Decorative path markers near both team spawns.
        foreach (float z in new float[] { -15f, 15f })
        {
            for (int i = -3; i <= 3; i++)
            {
                Block(
                    "Spawn_PathTile",
                    new Vector3(i * 2.4f, 0.065f, z),
                    new Vector3(1.7f, 0.035f, 2.2f),
                    stone
                );
            }

            Block(
                "Spawn_GoldLine",
                new Vector3(0f, 0.087f, z + (z < 0 ? 1.5f : -1.5f)),
                new Vector3(17f, 0.025f, 0.12f),
                gold
            );
        }
    }

    private void BuildArenaPillars(
        Material stone,
        Material gold)
    {
        Vector3[] positions =
        {
            new Vector3(-22f, 0f, -22f),
            new Vector3(22f, 0f, -22f),
            new Vector3(-22f, 0f, 22f),
            new Vector3(22f, 0f, 22f)
        };

        foreach (Vector3 pos in positions)
        {
            Block(
                "Pillar_Base",
                pos + Vector3.up * 0.3f,
                new Vector3(2f, 0.6f, 2f),
                stone
            );

            Block(
                "Pillar_Body",
                pos + Vector3.up * 1.8f,
                new Vector3(1.35f, 2.5f, 1.35f),
                stone
            );

            Block(
                "Pillar_Cap",
                pos + Vector3.up * 3.15f,
                new Vector3(1.8f, 0.25f, 1.8f),
                gold
            );
        }
    }

    private GameObject Block(
        string name,
        Vector3 position,
        Vector3 scale,
        Material material)
    {
        GameObject obj =
            GameObject.CreatePrimitive(PrimitiveType.Cube);

        obj.name = name;
        obj.transform.SetParent(generatedRoot, false);
        obj.transform.position = position;
        obj.transform.localScale = scale;

        obj.GetComponent<Renderer>().sharedMaterial = material;

        RemoveCollider(obj);

        // Ignore Raycast layer.
        obj.layer = 2;

        return obj;
    }

    private void RemoveCollider(GameObject obj)
    {
        Collider col = obj.GetComponent<Collider>();

        if (col != null)
            DestroyImmediate(col);
    }

    private Material GetMaterial(string name)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Materials/" + name + ".mat"
        );

        if (mat == null)
        {
            Debug.LogWarning("Material not found: " + name);

            Shader shader = Shader.Find(
                "Universal Render Pipeline/Lit"
            );

            if (shader == null)
                shader = Shader.Find("Standard");

            mat = new Material(shader);
        }

        return mat;
    }

#endif
}
