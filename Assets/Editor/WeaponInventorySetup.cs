using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.UI;

/// <summary>
/// Tools/Setup Weapons and Hotbar
/// One-click setup: copies sprites, creates WeaponInfo SOs, Animators, Prefabs, and builds the Canvas Hotbar UI.
/// </summary>
public static class WeaponInventorySetup
{
    // ── Paths ─────────────────────────────────────────────────────────────────
    const string SRC_SPRITES   = @"C:/Users/Laptop/Downloads/2drpgAssets/Sprites";
    const string DST_SPRITES   = "Assets/Sprites";
    const string DST_PREFABS   = "Assets/Prefabs/Weapons";
    const string DST_SO        = "Assets/ScriptableObjects/Weapons";
    const string DST_ANIMATORS = "Assets/Animations/Weapons";

    [MenuItem("Tools/Setup Weapons and Hotbar")]
    public static void Run()
    {
        AssetDatabase.StartAssetEditing();
        try
        {
            EnsureFolders();
            CopySprites();
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // After refresh, assets exist → configure & build
        ConfigureSpriteImports();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        CreateScriptableObjects(out WeaponInfo swordInfo, out WeaponInfo bowInfo, out WeaponInfo staffInfo);
        CreateAnimators(out AnimatorController bowCtrl, out AnimatorController staffCtrl);
        CreatePrefabs(bowCtrl, staffCtrl, bowInfo, staffInfo);

        // Update WeaponInfo prefab references
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        BuildHotbarUI(swordInfo, bowInfo, staffInfo);
        AddActiveWeaponToPlayer(swordInfo);
        AddIndestructibleToMapColliders();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[WeaponInventorySetup] ✅ Done! Hotbar, Bow, Staff, Arrow, MagicLaser all created.");
    }

    // ── 1. Folders ────────────────────────────────────────────────────────────

    static void EnsureFolders()
    {
        EnsureFolder("Assets/Sprites/UI");
        EnsureFolder("Assets/Sprites/Weapons");
        EnsureFolder("Assets/Prefabs/Weapons");
        EnsureFolder("Assets/ScriptableObjects/Weapons");
        EnsureFolder("Assets/Animations/Weapons");
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }
            current = next;
        }
    }

    // ── 2. Copy Sprites ───────────────────────────────────────────────────────

    static void CopySprites()
    {
        // UI icons for the Hotbar
        var uiCopies = new Dictionary<string, string>
        {
            { $"{SRC_SPRITES}/UI/Box.png",                    $"{DST_SPRITES}/UI/Box.png" },
            { $"{SRC_SPRITES}/UI/BoxWhiteOutline.png",        $"{DST_SPRITES}/UI/BoxWhiteOutline.png" },
            { $"{SRC_SPRITES}/UI/Sword.png",                  $"{DST_SPRITES}/UI/Sword.png" },
            { $"{SRC_SPRITES}/UI/Bow.png",                    $"{DST_SPRITES}/UI/Bow.png" },
            { $"{SRC_SPRITES}/UI/Staff.png",                  $"{DST_SPRITES}/UI/Staff.png" },
        };

        // Weapon world sprites (already in Assets/Sprites/Vegetation/ — only copy if missing)
        var weaponCopies = new Dictionary<string, string>
        {
            { $"{SRC_SPRITES}/Arrow.png",              $"{DST_SPRITES}/Weapons/Arrow.png" },
            { $"{SRC_SPRITES}/Bow.png",                $"{DST_SPRITES}/Weapons/Bow.png" },
            { $"{SRC_SPRITES}/Staff.png",              $"{DST_SPRITES}/Weapons/Staff.png" },
            { $"{SRC_SPRITES}/Magic Laser.png",        $"{DST_SPRITES}/Weapons/Magic Laser.png" },
            { $"{SRC_SPRITES}/Particle Sprite 1.png",  $"{DST_SPRITES}/Weapons/LaserHead.png" },
            { $"{SRC_SPRITES}/Vegetation/Cursor.png",   $"{DST_SPRITES}/UI/Cursor.png" },
        };

        EnsureFolder("Assets/Sprites/Weapons");

        foreach (var kv in uiCopies)       TryCopy(kv.Key, kv.Value);
        foreach (var kv in weaponCopies)   TryCopy(kv.Key, kv.Value);
    }

    static void TryCopy(string src, string dst)
    {
        string absPath = Path.GetFullPath(dst);
        string absDir  = Path.GetDirectoryName(absPath);
        if (!Directory.Exists(absDir)) Directory.CreateDirectory(absDir);
        if (!File.Exists(absPath) && File.Exists(src))
            File.Copy(src, absPath);
    }

    // ── 3. Configure Sprite Imports ───────────────────────────────────────────

    static void ConfigureSpriteImports()
    {
        // All UI sprites → Point filter, no compression
        string[] uiPaths = {
            $"{DST_SPRITES}/UI/Box.png",
            $"{DST_SPRITES}/UI/BoxWhiteOutline.png",
            $"{DST_SPRITES}/UI/Sword.png",
            $"{DST_SPRITES}/UI/Bow.png",
            $"{DST_SPRITES}/UI/Staff.png",
        };
        foreach (var p in uiPaths) SetSpriteImport(p, 32, FilterMode.Point);

        // World weapon sprites
        SetSpriteImport($"{DST_SPRITES}/Weapons/Bow.png",   16, FilterMode.Point);
        SetSpriteImport($"{DST_SPRITES}/Weapons/Staff.png", 16, FilterMode.Point);
        SetSpriteImport($"{DST_SPRITES}/Weapons/Arrow.png", 16, FilterMode.Point);
        SetSpriteImport($"{DST_SPRITES}/Weapons/LaserHead.png", 64, FilterMode.Bilinear);

        // Magic Laser — pivot at left edge {0, 0.5} so it grows forward from staff tip
        string laserPath = $"{DST_SPRITES}/Weapons/Magic Laser.png";
        var imp = AssetImporter.GetAtPath(laserPath) as TextureImporter;
        if (imp != null)
        {
            imp.spriteImportMode    = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = 16;
            imp.filterMode          = FilterMode.Point;
            imp.textureCompression  = TextureImporterCompression.Uncompressed;
            var settings = new TextureImporterSettings();
            imp.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot     = new Vector2(0f, 0.5f);
            imp.SetTextureSettings(settings);
            imp.SaveAndReimport();
        }

        // Cursor sprite
        string cursorPath = $"{DST_SPRITES}/UI/Cursor.png";
        var curImp = AssetImporter.GetAtPath(cursorPath) as TextureImporter;
        if (curImp != null)
        {
            curImp.textureType       = TextureImporterType.Cursor;
            curImp.filterMode        = FilterMode.Point;
            curImp.textureCompression = TextureImporterCompression.Uncompressed;
            curImp.SaveAndReimport();
        }
    }

    static void SetSpriteImport(string path, int ppu, FilterMode filter)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) return;
        imp.spriteImportMode    = SpriteImportMode.Single;
        imp.spritePixelsPerUnit = ppu;
        imp.filterMode          = filter;
        imp.textureCompression  = TextureImporterCompression.Uncompressed;
        imp.SaveAndReimport();
    }

    // ── 4. ScriptableObjects ──────────────────────────────────────────────────

    static void CreateScriptableObjects(out WeaponInfo swordInfo, out WeaponInfo bowInfo, out WeaponInfo staffInfo)
    {
        swordInfo = GetOrCreate<WeaponInfo>($"{DST_SO}/SwordInfo.asset");
        swordInfo.weaponCooldown = 0.35f;
        swordInfo.weaponDamage   = 1;
        swordInfo.weaponRange    = 1f;
        // weaponPrefab left null — sword uses PlayerCombat component on Player

        bowInfo = GetOrCreate<WeaponInfo>($"{DST_SO}/BowInfo.asset");
        bowInfo.weaponCooldown = 0.5f;
        bowInfo.weaponDamage   = 1;
        bowInfo.weaponRange    = 10f;

        staffInfo = GetOrCreate<WeaponInfo>($"{DST_SO}/StaffInfo.asset");
        staffInfo.weaponCooldown = 0.8f;
        staffInfo.weaponDamage   = 1;
        staffInfo.weaponRange    = 8f;

        EditorUtility.SetDirty(swordInfo);
        EditorUtility.SetDirty(bowInfo);
        EditorUtility.SetDirty(staffInfo);
        AssetDatabase.SaveAssets();
    }

    // ── 5. Animators ──────────────────────────────────────────────────────────

    static void CreateAnimators(out AnimatorController bowCtrl, out AnimatorController staffCtrl)
    {
        bowCtrl   = GetOrCreateAnimator($"{DST_ANIMATORS}/BowController.controller");
        staffCtrl = GetOrCreateAnimator($"{DST_ANIMATORS}/StaffController.controller");

        EnsureTrigger(bowCtrl,   "Fire");
        EnsureTrigger(staffCtrl, "Attack");
    }

    static AnimatorController GetOrCreateAnimator(string path)
    {
        var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (existing != null) return existing;
        string dir = Path.GetDirectoryName(path).Replace("\\", "/");
        EnsureFolder(dir);
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);
        return ctrl;
    }

    static void EnsureTrigger(AnimatorController ctrl, string paramName)
    {
        foreach (var p in ctrl.parameters)
            if (p.name == paramName) return;
        ctrl.AddParameter(paramName, AnimatorControllerParameterType.Trigger);
    }

    // ── 6. Prefabs ────────────────────────────────────────────────────────────

    static void CreatePrefabs(AnimatorController bowCtrl, AnimatorController staffCtrl,
                               WeaponInfo bowInfo, WeaponInfo staffInfo)
    {
        // 1. Arrow prefab
        string arrowPath = $"{DST_PREFABS}/Arrow.prefab";
        {
            var go = new GameObject("Arrow");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{DST_SPRITES}/Weapons/Arrow.png");
            sr.flipX = true; // Point arrowhead forward (+X)
            sr.sortingOrder = 5;

            var col = go.AddComponent<CapsuleCollider2D>();
            col.isTrigger = true;
            col.direction = CapsuleDirection2D.Horizontal;
            col.size = new Vector2(0.5f, 0.15f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints  = RigidbodyConstraints2D.FreezeRotation;

            go.AddComponent<Projectile>();

            // TrailRenderer for glowing white comet trail
            var trail = go.AddComponent<TrailRenderer>();
            trail.time = 0.2f;
            trail.startWidth = 0.3f;
            trail.endWidth = 0.01f;
            trail.sortingOrder = 4;
            var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader != null) trail.material = new Material(shader);

            var grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.95f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            trail.colorGradient = grad;

            PrefabUtility.SaveAsPrefabAsset(go, arrowPath);
            Object.DestroyImmediate(go);
        }

        // 2. MagicLaser prefab
        string laserPath = $"{DST_PREFABS}/MagicLaser.prefab";
        {
            var go = new GameObject("MagicLaser");
            var sr = go.AddComponent<SpriteRenderer>();
            var laserSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{DST_SPRITES}/Weapons/Magic Laser.png");
            sr.sprite       = laserSprite;
            sr.drawMode     = SpriteDrawMode.Tiled;
            sr.tileMode     = SpriteTileMode.Adaptive;
            sr.size         = new Vector2(1f, 1f);
            sr.color        = new Color(0.12f, 0.68f, 1f, 1f); // Electric Cyan Blue glow
            sr.sortingOrder = 5;

            var col = go.AddComponent<CapsuleCollider2D>();
            col.isTrigger  = true;
            col.direction  = CapsuleDirection2D.Horizontal;
            col.size       = new Vector2(1f, 0.35f);
            col.offset     = new Vector2(0.5f, 0f);

            go.AddComponent<SpriteFade>();
            var laserScript = go.AddComponent<MagicLaser>();

            // Glowing head cap (Particle Sprite 1) at the tip of the laser
            var headGO = new GameObject("LaserHead");
            headGO.transform.SetParent(go.transform, false);
            headGO.transform.localPosition = new Vector2(1f, 0f);
            headGO.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            var headSR = headGO.AddComponent<SpriteRenderer>();
            headSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{DST_SPRITES}/Weapons/LaserHead.png");
            headSR.color  = new Color(0.85f, 0.95f, 1f, 0.95f);
            headSR.sortingOrder = 6;
            headGO.AddComponent<SpriteFade>();

            var laserSO = new SerializedObject(laserScript);
            laserSO.FindProperty("laserHead").objectReferenceValue = headGO.transform;
            laserSO.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(go, laserPath);
            Object.DestroyImmediate(go);
        }

        // 3. Bow prefab
        string bowPrefabPath = $"{DST_PREFABS}/Bow.prefab";
        {
            var go = new GameObject("Bow");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{DST_SPRITES}/Weapons/Bow.png");
            sr.sortingOrder = 6;
            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = bowCtrl;

            // Arrow spawn point
            var spawnPt = new GameObject("ArrowSpawnPoint");
            spawnPt.transform.SetParent(go.transform);
            spawnPt.transform.localPosition = new Vector3(0.35f, 0f, 0f);

            var bowScript = go.AddComponent<Bow>();

            var prefabGO = AssetDatabase.LoadAssetAtPath<GameObject>(arrowPath);
            var so = new SerializedObject(bowScript);
            so.FindProperty("arrowPrefab").objectReferenceValue      = prefabGO;
            so.FindProperty("arrowSpawnPoint").objectReferenceValue  = spawnPt.transform;
            so.FindProperty("weaponInfo").objectReferenceValue        = bowInfo;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(go, bowPrefabPath);
            Object.DestroyImmediate(go);
        }

        var bowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(bowPrefabPath);
        if (bowPrefab != null)
        {
            bowInfo.weaponPrefab = bowPrefab;
            EditorUtility.SetDirty(bowInfo);
        }

        // 4. Staff prefab
        string staffPrefabPath = $"{DST_PREFABS}/Staff.prefab";
        {
            var go = new GameObject("Staff");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{DST_SPRITES}/Weapons/Staff.png");
            sr.sortingOrder = 6;
            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = staffCtrl;

            // Laser spawn point at top crystal
            var spawnPt = new GameObject("LaserSpawnPoint");
            spawnPt.transform.SetParent(go.transform);
            spawnPt.transform.localPosition = new Vector3(0.4f, 0.22f, 0f);

            var staffScript = go.AddComponent<Staff>();

            var laserPrefabGO = AssetDatabase.LoadAssetAtPath<GameObject>(laserPath);
            var so = new SerializedObject(staffScript);
            so.FindProperty("magicLaserPrefab").objectReferenceValue  = laserPrefabGO;
            so.FindProperty("laserSpawnPoint").objectReferenceValue   = spawnPt.transform;
            so.FindProperty("weaponInfo").objectReferenceValue        = staffInfo;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(go, staffPrefabPath);
            Object.DestroyImmediate(go);
        }

        var staffPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(staffPrefabPath);
        if (staffPrefab != null)
        {
            staffInfo.weaponPrefab = staffPrefab;
            EditorUtility.SetDirty(staffInfo);
        }
    }

    // ── 7. Canvas Hotbar UI ───────────────────────────────────────────────────

    static void BuildHotbarUI(WeaponInfo swordInfo, WeaponInfo bowInfo, WeaponInfo staffInfo)
    {
        // Ensure EventSystem with InputSystemUIInputModule
        var existingES = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (existingES == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // Ensure CursorManager on existing Canvas
        var sceneCanvas = Object.FindAnyObjectByType<Canvas>();
        if (sceneCanvas != null && sceneCanvas.GetComponent<CursorManager>() == null)
        {
            var cm = sceneCanvas.gameObject.AddComponent<CursorManager>();
            var cmSO = new SerializedObject(cm);
            var curTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{DST_SPRITES}/UI/Cursor.png");
            cmSO.FindProperty("cursorTexture").objectReferenceValue = curTex;
            cmSO.ApplyModifiedPropertiesWithoutUndo();
        }

        // Check if Hotbar already exists in scene
        var existingInventory = Object.FindAnyObjectByType<ActiveInventory>();
        if (existingInventory != null)
        {
            Debug.Log("[WeaponInventorySetup] ActiveInventory already exists in scene — skipping UI build.");
            return;
        }

        // Load UI sprites
        var boxSprite   = AssetDatabase.LoadAssetAtPath<Sprite>($"{DST_SPRITES}/UI/Box.png");
        var borderSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{DST_SPRITES}/UI/BoxWhiteOutline.png");
        var swordIcon   = AssetDatabase.LoadAssetAtPath<Sprite>($"{DST_SPRITES}/UI/Sword.png");
        var bowIcon     = AssetDatabase.LoadAssetAtPath<Sprite>($"{DST_SPRITES}/UI/Bow.png");
        var staffIcon   = AssetDatabase.LoadAssetAtPath<Sprite>($"{DST_SPRITES}/UI/Staff.png");

        // Canvas
        var canvasGO = new GameObject("UI Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Hotbar (anchored bottom-center)
        var hotbarGO = new GameObject("Hotbar");
        hotbarGO.transform.SetParent(canvasGO.transform, false);
        var hotbarRT = hotbarGO.AddComponent<RectTransform>();
        hotbarRT.anchorMin = new Vector2(0.5f, 0f);
        hotbarRT.anchorMax = new Vector2(0.5f, 0f);
        hotbarRT.pivot     = new Vector2(0.5f, 0f);
        hotbarRT.anchoredPosition = new Vector2(0f, 10f);
        hotbarRT.sizeDelta = new Vector2(144f, 48f);

        var hlg = hotbarGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 4f;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        hotbarGO.AddComponent<ActiveInventory>();

        // Build 3 slots
        var weapons = new[] {
            (swordInfo, swordIcon, "Slot_1 (Sword)"),
            (bowInfo,   bowIcon,   "Slot_2 (Bow)"),
            (staffInfo, staffIcon, "Slot_3 (Staff)"),
        };

        for (int i = 0; i < weapons.Length; i++)
        {
            (var info, var icon, var slotName) = weapons[i];
            var slotGO = new GameObject(slotName);
            slotGO.transform.SetParent(hotbarGO.transform, false);

            var slotRT = slotGO.AddComponent<RectTransform>();
            slotRT.sizeDelta = new Vector2(44f, 44f);

            var slotImg = slotGO.AddComponent<Image>();
            slotImg.sprite = boxSprite;
            slotImg.preserveAspect = false;
            if (boxSprite == null) slotImg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            // Border highlight child (active only when this slot is selected)
            var borderGO = new GameObject("Border");
            borderGO.transform.SetParent(slotGO.transform, false);
            var borderRT  = borderGO.AddComponent<RectTransform>();
            borderRT.anchorMin    = Vector2.zero;
            borderRT.anchorMax    = Vector2.one;
            borderRT.offsetMin    = Vector2.zero;
            borderRT.offsetMax    = Vector2.zero;
            var borderImg = borderGO.AddComponent<Image>();
            borderImg.sprite  = borderSprite;
            borderImg.color   = Color.white;
            if (borderSprite == null) { borderImg.color = new Color(1f, 1f, 0f, 0.9f); }
            borderGO.SetActive(i == 0); // only Slot 1 active by default

            // Weapon icon child
            var iconGO = new GameObject("WeaponIcon");
            iconGO.transform.SetParent(slotGO.transform, false);
            var iconRT  = iconGO.AddComponent<RectTransform>();
            iconRT.anchorMin    = new Vector2(0.1f, 0.1f);
            iconRT.anchorMax    = new Vector2(0.9f, 0.9f);
            iconRT.offsetMin    = Vector2.zero;
            iconRT.offsetMax    = Vector2.zero;
            var iconImg = iconGO.AddComponent<Image>();
            iconImg.sprite          = icon;
            iconImg.preserveAspect  = true;
            if (icon == null) iconImg.color = Color.gray;

            // InventorySlot component
            var slotComp = slotGO.AddComponent<InventorySlot>();
            var slotSO   = new SerializedObject(slotComp);
            slotSO.FindProperty("weaponInfo").objectReferenceValue = info;
            slotSO.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    // ── 8. Add ActiveWeapon to Player ─────────────────────────────────────────

    static void AddActiveWeaponToPlayer(WeaponInfo swordInfo)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            var pc = Object.FindAnyObjectByType<PlayerController>();
            if (pc != null) player = pc.gameObject;
        }

        if (player == null)
        {
            Debug.LogWarning("[WeaponInventorySetup] Cannot find Player in scene. Add ActiveWeapon child manually.");
            return;
        }

        // Assign swordInfo to PlayerCombat
        var combat = player.GetComponent<PlayerCombat>();
        if (combat != null)
        {
            var combatSO = new SerializedObject(combat);
            var swordInfoProp = combatSO.FindProperty("swordWeaponInfo");
            if (swordInfoProp != null)
            {
                swordInfoProp.objectReferenceValue = swordInfo;
                combatSO.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        // Check if ActiveWeapon child already exists
        var existing = player.GetComponentInChildren<ActiveWeapon>();
        if (existing != null) return;

        var awGO = new GameObject("ActiveWeapon");
        awGO.transform.SetParent(player.transform, false);
        awGO.transform.localPosition = Vector3.zero;
        awGO.AddComponent<ActiveWeapon>();
    }

    // ── 9. Mark map colliders as Indestructible ───────────────────────────────

    static void AddIndestructibleToMapColliders()
    {
        // Look for a MapColliders group in the scene
        var mapColliders = GameObject.Find("MapColliders");
        if (mapColliders == null) return;

        // Add Indestructible to children that have colliders
        foreach (Collider2D col in mapColliders.GetComponentsInChildren<Collider2D>())
        {
            if (col.GetComponent<Indestructible>() == null)
                col.gameObject.AddComponent<Indestructible>();
        }
        Debug.Log("[WeaponInventorySetup] Indestructible added to MapColliders children.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static T GetOrCreate<T>(string assetPath) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        if (existing != null) return existing;
        string dir = Path.GetDirectoryName(assetPath).Replace("\\", "/");
        EnsureFolder(dir);
        var obj = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(obj, assetPath);
        return obj;
    }
}
