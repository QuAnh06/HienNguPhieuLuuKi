using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MapSetup
{
    private const string CleanGroundAssetPath = "Assets/Environment/ForestGroundClean.png";

    [MenuItem("Tools/Build Interactive RPG Map")]
    public static void BuildInteractiveMap()
    {
        // 0. Force refresh the clean ground texture
        AssetDatabase.ImportAsset(CleanGroundAssetPath, ImportAssetOptions.ForceUpdate);

        // Find or create ForestMap root
        GameObject forestMap = GameObject.Find("ForestMap");
        if (forestMap == null)
        {
            forestMap = new GameObject("ForestMap");
            Undo.RegisterCreatedObjectUndo(forestMap, "Create ForestMap");
        }

        // 1. Setup Ground Layer
        Transform groundTrans = forestMap.transform.Find("ForestGround");
        GameObject groundObj = groundTrans != null ? groundTrans.gameObject : new GameObject("ForestGround");
        if (groundTrans == null)
        {
            groundObj.transform.SetParent(forestMap.transform);
            Undo.RegisterCreatedObjectUndo(groundObj, "Create ForestGround");
        }

        groundObj.transform.localPosition = Vector3.zero;
        groundObj.transform.localRotation = Quaternion.identity;
        groundObj.transform.localScale = Vector3.one;

        SpriteRenderer groundSr = groundObj.GetComponent<SpriteRenderer>();
        if (groundSr == null) groundSr = groundObj.AddComponent<SpriteRenderer>();

        Sprite cleanGroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CleanGroundAssetPath);
        if (cleanGroundSprite == null) cleanGroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Environment/ForestMapHD.png");
        groundSr.sprite = cleanGroundSprite;
        groundSr.sortingLayerName = "Default";
        groundSr.sortingOrder = -100;

        // 2. Clear old children except ForestGround
        for (int i = forestMap.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = forestMap.transform.GetChild(i);
            if (child.name != "ForestGround")
            {
                Undo.DestroyObjectImmediate(child.gameObject);
            }
        }

        // 3. Load all sprite assets
        Sprite sprTree1 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Vegetation/spr_tree1.png");
        Sprite sprTree2 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Vegetation/spr_tree2.png");
        Sprite sprBush = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Vegetation/Bush.png");
        Sprite sprBarrel1 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Objects and buildings/Barrels and crates/spr_barrel1.png");
        Sprite sprBarrel2 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Objects and buildings/Barrels and crates/spr_barrel2.png");
        Sprite sprCrate1 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Objects and buildings/Barrels and crates/spr_crate1.png");
        Sprite sprCrate2 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Objects and buildings/Barrels and crates/spr_crate2.png");
        Sprite sprBoulder1 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Rocks/spr_boulder1.png");
        Sprite sprSmallRock1 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Rocks/spr_smallrock1.png");

        Vector3 treeScale = new Vector3(0.55f, 0.55f, 1f);
        Vector3 barrelScale = new Vector3(0.42f, 0.42f, 1f);
        Vector3 crateScale = new Vector3(0.42f, 0.42f, 1f);
        Vector3 bushScale = new Vector3(0.55f, 0.55f, 1f);

        // --- Container: Trees ---
        GameObject treesRoot = CreateCategoryContainer(forestMap, "Trees");
        CreateProp(treesRoot, "Tree_Oak_TopLeft", sprTree1, new Vector3(-5.49f, 0.66f, 0f), treeScale, true, 0.45f);
        CreateProp(treesRoot, "Tree_Oak_TopRight", sprTree1, new Vector3(3.88f, 1.28f, 0f), treeScale, true, 0.45f);
        CreateProp(treesRoot, "Tree_Oak_BottomRight", sprTree1, new Vector3(3.66f, -2.64f, 0f), treeScale, true, 0.45f);
        CreateProp(treesRoot, "Tree_Oak_Left", sprTree1, new Vector3(-7.36f, -1.30f, 0f), treeScale, true, 0.45f);
        CreateProp(treesRoot, "Tree_Cypress_Top", sprTree2, new Vector3(2.14f, 1.77f, 0f), treeScale, true, 0.35f);
        CreateProp(treesRoot, "Tree_Cypress_Bottom", sprTree2, new Vector3(2.68f, -2.82f, 0f), treeScale, true, 0.35f);

        // --- Container: Barrels ---
        GameObject barrelsRoot = CreateCategoryContainer(forestMap, "Barrels");
        CreateProp(barrelsRoot, "Barrel_TopPond", sprBarrel1, new Vector3(-1.47f, 2.00f, 0f), barrelScale, true, 0.32f);
        CreateProp(barrelsRoot, "Barrel_SouthLeft", sprBarrel2 != null ? sprBarrel2 : sprBarrel1, new Vector3(-4.60f, -3.54f, 0f), barrelScale, true, 0.32f);
        CreateProp(barrelsRoot, "Barrel_SouthRight", sprBarrel1, new Vector3(1.65f, -3.49f, 0f), barrelScale, true, 0.32f);

        // --- Container: Crates ---
        GameObject cratesRoot = CreateCategoryContainer(forestMap, "Crates");
        // Stack Left (3 crates)
        CreateBoxProp(cratesRoot, "Crate_Left_Base1", sprCrate1, new Vector3(-4.28f, 1.15f, 0f), crateScale, new Vector2(0.8f, 0.8f));
        CreateBoxProp(cratesRoot, "Crate_Left_Base2", sprCrate2 != null ? sprCrate2 : sprCrate1, new Vector3(-3.75f, 1.15f, 0f), crateScale, new Vector2(0.8f, 0.8f));
        CreateBoxProp(cratesRoot, "Crate_Left_Top", sprCrate1, new Vector3(-4.02f, 1.60f, 0f), crateScale, new Vector2(0.8f, 0.8f));
        // Stack Cliff Top-Right (3 crates)
        CreateBoxProp(cratesRoot, "Crate_Cliff_Base1", sprCrate1, new Vector3(5.58f, 2.94f, 0f), crateScale, new Vector2(0.8f, 0.8f));
        CreateBoxProp(cratesRoot, "Crate_Cliff_Base2", sprCrate2 != null ? sprCrate2 : sprCrate1, new Vector3(6.07f, 2.94f, 0f), crateScale, new Vector2(0.8f, 0.8f));
        CreateBoxProp(cratesRoot, "Crate_Cliff_Top", sprCrate1, new Vector3(5.80f, 3.43f, 0f), crateScale, new Vector2(0.8f, 0.8f));
        // Single Crates
        CreateBoxProp(cratesRoot, "Crate_CenterRight", sprCrate1, new Vector3(2.68f, 1.15f, 0f), crateScale, new Vector2(0.8f, 0.8f));
        CreateBoxProp(cratesRoot, "Crate_South", sprCrate1, new Vector3(-0.76f, -3.80f, 0f), crateScale, new Vector2(0.8f, 0.8f));

        // --- Container: Bushes ---
        GameObject bushesRoot = CreateCategoryContainer(forestMap, "Bushes");
        CreateProp(bushesRoot, "Bush_CliffTopLeft", sprBush, new Vector3(-7.36f, 2.58f, 0f), bushScale, false, 0f);
        CreateProp(bushesRoot, "Bush_NearOakLeft", sprBush, new Vector3(-4.68f, -0.41f, 0f), bushScale, false, 0f);
        CreateProp(bushesRoot, "Bush_CenterPath", sprBush, new Vector3(3.30f, 0.35f, 0f), bushScale, false, 0f);
        CreateProp(bushesRoot, "Bush_TopRightCliff", sprBush, new Vector3(7.18f, 2.49f, 0f), bushScale, false, 0f);

        // --- Container: Rocks ---
        GameObject rocksRoot = CreateCategoryContainer(forestMap, "Rocks");
        CreateProp(rocksRoot, "Boulder_CliffTopRight", sprBoulder1, new Vector3(7.5f, 3.6f, 0f), new Vector3(1.2f, 1.2f, 1f), true, 0.35f);
        CreateProp(rocksRoot, "Boulder_CliffBottomRight", sprBoulder1, new Vector3(7.2f, -1.8f, 0f), new Vector3(1.2f, 1.2f, 1f), true, 0.35f);
        CreateProp(rocksRoot, "SmallRock_Path1", sprSmallRock1, new Vector3(0.8f, 0.8f, 0f), new Vector3(1.0f, 1.0f, 1f), false, 0f);
        CreateProp(rocksRoot, "SmallRock_Path2", sprSmallRock1, new Vector3(1.6f, -0.6f, 0f), new Vector3(1.0f, 1.0f, 1f), false, 0f);

        // --- Container: Colliders (Terrain Boundaries, Ponds, Cliffs) ---
        GameObject collidersObj = CreateCategoryContainer(forestMap, "MapColliders");
        AddBox(collidersObj, new Vector2(0f, 4.4f), new Vector2(18f, 1.4f)); // Top Canopy
        AddBox(collidersObj, new Vector2(-4.8f, -4.3f), new Vector2(8f, 1.4f)); // Bottom-Left Hedge
        AddBox(collidersObj, new Vector2(4.8f, -4.3f), new Vector2(8f, 1.4f)); // Bottom-Right Hedge
        AddBox(collidersObj, new Vector2(-8.4f, 0f), new Vector2(1.2f, 10f)); // Left Wall
        AddBox(collidersObj, new Vector2(8.4f, 0f), new Vector2(1.2f, 10f)); // Right Wall

        // Central Divider Hedge
        AddBox(collidersObj, new Vector2(-3.3f, -2.5f), new Vector2(4.2f, 1.4f));
        AddBox(collidersObj, new Vector2(0.3f, -2.5f), new Vector2(1.6f, 1.4f));

        // Water Ponds
        AddCapsule(collidersObj, new Vector2(-3.45f, 2.7f), new Vector2(1.1f, 1.7f)); // Top-Left Pond
        AddCapsule(collidersObj, new Vector2(-6.6f, -3.5f), new Vector2(2.4f, 1.3f)); // Bottom-Left Pond
        AddCapsule(collidersObj, new Vector2(5.7f, -3.8f), new Vector2(1.2f, 0.9f)); // Bottom-Right Pond

        // Cliffs
        AddBox(collidersObj, new Vector2(-7.6f, 2.6f), new Vector2(2.4f, 2.0f)); // Top-Left Cliff
        AddBox(collidersObj, new Vector2(-7.2f, -0.6f), new Vector2(1.8f, 1.8f)); // Middle-Left Cliff
        AddBox(collidersObj, new Vector2(6.8f, 3.8f), new Vector2(4.5f, 1.4f)); // Top-Right Cliff
        AddBox(collidersObj, new Vector2(8.0f, 0.4f), new Vector2(2.0f, 3.2f)); // Middle-Right Cliff
        AddBox(collidersObj, new Vector2(7.8f, -2.6f), new Vector2(2.0f, 1.8f)); // Bottom-Right Cliff

        // --- Sunbeams ---
        GameObject sunbeamsObj = CreateCategoryContainer(forestMap, "Sunbeams");
        Sprite beamSprite = CreateSoftBeamSprite();
        CreateSunbeam(sunbeamsObj, beamSprite, "Beam_Left", new Vector3(-3.8f, 3.2f, 0f), 32f, new Vector2(2.8f, 8.5f));
        CreateSunbeam(sunbeamsObj, beamSprite, "Beam_Center", new Vector3(1.2f, 3.6f, 0f), 26f, new Vector2(3.2f, 9.5f));
        CreateSunbeam(sunbeamsObj, beamSprite, "Beam_Right", new Vector3(6.2f, 2.8f, 0f), 30f, new Vector2(2.6f, 8.5f));

        // Configure Camera
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.backgroundColor = new Color(0.04f, 0.10f, 0.05f, 1f);
            mainCam.transparencySortMode = TransparencySortMode.CustomAxis;
            mainCam.transparencySortAxis = new Vector3(0f, 1f, 0f);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Successfully built Interactive RPG Map with selectable GameObjects for all assets!");
    }

    private static GameObject CreateCategoryContainer(GameObject parent, string name)
    {
        GameObject container = new GameObject(name);
        container.transform.SetParent(parent.transform);
        container.transform.localPosition = Vector3.zero;
        container.transform.localRotation = Quaternion.identity;
        container.transform.localScale = Vector3.one;
        Undo.RegisterCreatedObjectUndo(container, "Create " + name);
        return container;
    }

    private static GameObject CreateProp(GameObject parent, string name, Sprite sprite, Vector3 position, Vector3 scale, bool addCircleCollider, float colRadius)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform);
        obj.transform.localPosition = position;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = scale;

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Default";
        sr.sortingOrder = Mathf.RoundToInt(-position.y * 10);

        if (addCircleCollider)
        {
            CircleCollider2D circle = obj.AddComponent<CircleCollider2D>();
            circle.radius = colRadius;
        }

        Undo.RegisterCreatedObjectUndo(obj, "Create " + name);
        return obj;
    }

    private static GameObject CreateBoxProp(GameObject parent, string name, Sprite sprite, Vector3 position, Vector3 scale, Vector2 boxSize)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform);
        obj.transform.localPosition = position;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = scale;

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Default";
        sr.sortingOrder = Mathf.RoundToInt(-position.y * 10);

        BoxCollider2D box = obj.AddComponent<BoxCollider2D>();
        box.size = boxSize;

        Undo.RegisterCreatedObjectUndo(obj, "Create " + name);
        return obj;
    }

    private static void AddBox(GameObject parent, Vector2 offset, Vector2 size)
    {
        BoxCollider2D box = parent.AddComponent<BoxCollider2D>();
        box.offset = offset;
        box.size = size;
    }

    private static void AddCapsule(GameObject parent, Vector2 offset, Vector2 size)
    {
        CapsuleCollider2D cap = parent.AddComponent<CapsuleCollider2D>();
        cap.offset = offset;
        cap.size = size;
        cap.direction = size.x > size.y ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical;
    }

    private static Sprite CreateSoftBeamSprite()
    {
        int w = 64;
        int h = 128;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Color beamColor = new Color(1f, 0.96f, 0.72f, 1f);

        for (int y = 0; y < h; y++)
        {
            float v = (float)y / (h - 1);
            float lengthFade = Mathf.Pow(1f - v, 1.2f) * Mathf.SmoothStep(0f, 1f, v * 3.5f);

            for (int x = 0; x < w; x++)
            {
                float u = (float)x / (w - 1);
                float distFromCenter = Mathf.Abs(u - 0.5f) * 2f;
                float widthFade = Mathf.SmoothStep(1f, 0f, distFromCenter);
                float alpha = widthFade * lengthFade * 0.16f;

                tex.SetPixel(x, y, new Color(beamColor.r, beamColor.g, beamColor.b, alpha));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.9f), 100f);
    }

    private static void CreateSunbeam(GameObject parent, Sprite sprite, string name, Vector3 pos, float angleZ, Vector2 scale)
    {
        GameObject beam = new GameObject(name);
        beam.transform.SetParent(parent.transform);
        beam.transform.localPosition = pos;
        beam.transform.localRotation = Quaternion.Euler(0f, 0f, -angleZ);
        beam.transform.localScale = new Vector3(scale.x, scale.y, 1f);

        SpriteRenderer sr = beam.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 50;
    }
}
