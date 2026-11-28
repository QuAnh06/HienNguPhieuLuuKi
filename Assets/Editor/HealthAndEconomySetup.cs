using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class HealthAndEconomySetup
{
    private const string UiSpriteDir = "Assets/Sprites/UI";
    private const string PrefabUiDir = "Assets/Prefabs/UI";
    private const string PrefabPickupsDir = "Assets/Prefabs/Pickups";
    private const string PlayerPrefabPath = "Assets/Prefabs/Player/Player.prefab";
    private const string GoldCoinPrefabPath = "Assets/Prefabs/Gold Coin.prefab";
    private const string HeartPickupPrefabPath = "Assets/Prefabs/Pickups/HeartPickup.prefab";
    private const string PlayerHudPrefabPath = "Assets/Prefabs/UI/PlayerHUD.prefab";

    [MenuItem("Tools/Setup Health and Drops System")]
    public static void RunSetup()
    {
        Debug.Log("Starting Health, HUD and Drops System Setup...");

        // 1. Create or update HeartPickup.prefab
        GameObject heartPrefab = SetupHeartPickupPrefab();

        // 2. Update Gold Coin.prefab
        GameObject goldCoinPrefab = SetupGoldCoinPrefab();

        // 3. Update Enemy Prefabs (BlueSlime & Ghost) with PickupSpawner
        SetupEnemyPrefabs(goldCoinPrefab, heartPrefab);

        // 4. Safely update Player.prefab
        SetupPlayerPrefab();

        // 5. Create PlayerHUD.prefab
        GameObject hudPrefab = SetupPlayerHudPrefab();

        // 6. Process all scenes
        ProcessAllScenes(hudPrefab);

        Debug.Log("Health and Drops System Setup completed successfully!");
    }

    private static GameObject SetupHeartPickupPrefab()
    {
        Sprite heartSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{UiSpriteDir}/Heart Pickup.png");
        if (heartSprite == null)
        {
            Debug.LogError($"Heart sprite not found at {UiSpriteDir}/Heart Pickup.png");
            return null;
        }

        GameObject go = new GameObject("HeartPickup");

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = heartSprite;
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 2;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.2f;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.linearDamping = 2f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        Pickup pickup = go.AddComponent<Pickup>();
        SerializedObject so = new SerializedObject(pickup);
        so.FindProperty("pickupType").enumValueIndex = (int)Pickup.PickupType.Heart;
        so.FindProperty("amount").intValue = 1;
        so.FindProperty("pickUpDistance").floatValue = 3.5f;
        so.FindProperty("moveSpeed").floatValue = 3.5f;
        so.FindProperty("popDuration").floatValue = 0.5f;
        so.FindProperty("heightY").floatValue = 1.2f;
        so.FindProperty("lifetime").floatValue = 20f;
        so.ApplyModifiedProperties();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, HeartPickupPrefabPath);
        Object.DestroyImmediate(go);
        Debug.Log("Created HeartPickup.prefab");
        return prefab;
    }

    private static GameObject SetupGoldCoinPrefab()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GoldCoinPrefabPath);
        if (prefab == null) return null;

        GameObject root = PrefabUtility.LoadPrefabContents(GoldCoinPrefabPath);
        Pickup pickup = root.GetComponent<Pickup>();
        if (pickup == null) pickup = root.AddComponent<Pickup>();

        SerializedObject so = new SerializedObject(pickup);
        so.FindProperty("pickupType").enumValueIndex = (int)Pickup.PickupType.Gold;
        so.FindProperty("amount").intValue = 1;
        so.FindProperty("pickUpDistance").floatValue = 3.5f;
        so.FindProperty("moveSpeed").floatValue = 3.5f;
        so.FindProperty("popDuration").floatValue = 0.5f;
        so.FindProperty("heightY").floatValue = 1.2f;
        so.FindProperty("lifetime").floatValue = 20f;
        so.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(root, GoldCoinPrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        Debug.Log("Updated Gold Coin.prefab with Pickup settings.");
        return AssetDatabase.LoadAssetAtPath<GameObject>(GoldCoinPrefabPath);
    }

    private static void SetupEnemyPrefabs(GameObject goldCoinPrefab, GameObject heartPrefab)
    {
        string[] enemies = { "Assets/Prefabs/Enemies/BlueSlime.prefab", "Assets/Prefabs/Enemies/Ghost.prefab" };
        foreach (var path in enemies)
        {
            if (!File.Exists(path)) continue;

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            PickupSpawner spawner = root.GetComponent<PickupSpawner>();
            if (spawner == null) spawner = root.AddComponent<PickupSpawner>();

            SerializedObject so = new SerializedObject(spawner);
            var goldProp = so.FindProperty("goldCoinPrefab");
            var heartProp = so.FindProperty("heartPickupPrefab");

            if (goldProp != null && goldCoinPrefab != null) goldProp.objectReferenceValue = goldCoinPrefab;
            if (heartProp != null && heartPrefab != null) heartProp.objectReferenceValue = heartPrefab;
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
            Debug.Log($"Wired PickupSpawner on {path}");
        }
    }

    private static void SetupPlayerPrefab()
    {
        if (!File.Exists(PlayerPrefabPath)) return;

        GameObject root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);

        // CapsuleCollider2D
        CapsuleCollider2D col = root.GetComponent<CapsuleCollider2D>();
        if (col == null) col = root.AddComponent<CapsuleCollider2D>();
        col.isTrigger = false;
        col.size = new Vector2(0.7f, 1.0f);
        col.offset = new Vector2(0f, -0.1f);

        // Knockback
        if (root.GetComponent<Knockback>() == null) root.AddComponent<Knockback>();

        // Flash
        if (root.GetComponent<Flash>() == null) root.AddComponent<Flash>();

        // PlayerHealth
        PlayerHealth ph = root.GetComponent<PlayerHealth>();
        if (ph == null) ph = root.AddComponent<PlayerHealth>();

        SerializedObject so = new SerializedObject(ph);
        so.FindProperty("maxHealth").intValue = 7;
        so.FindProperty("damageCooldown").floatValue = 0.6f;
        so.FindProperty("knockbackThrust").floatValue = 10f;
        so.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        Debug.Log("Safely updated Player.prefab with Collider, Health (7), Flash and Knockback.");
    }

    private static GameObject SetupPlayerHudPrefab()
    {
        Sprite heartSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{UiSpriteDir}/Heart Full.png");
        Sprite borderSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{UiSpriteDir}/Border Empty White Outline.png");
        Sprite fillSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{UiSpriteDir}/Health_Fill.png");
        Sprite coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{UiSpriteDir}/GoldCoin_WithOutline.png");
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>($"{UiSpriteDir}/Gixel SDF.asset");

        GameObject hudGO = new GameObject("PlayerHUD");
        RectTransform hudRT = hudGO.AddComponent<RectTransform>();

        Canvas canvas = hudGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = hudGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        hudGO.AddComponent<GraphicRaycaster>();
        PlayerHUD playerHUD = hudGO.AddComponent<PlayerHUD>();

        // ── Health Container ──────────────────────────────────────────────────
        GameObject healthContainer = new GameObject("Health_Container");
        RectTransform hcRT = healthContainer.AddComponent<RectTransform>();
        hcRT.SetParent(hudRT, false);
        hcRT.anchorMin = new Vector2(0, 1);
        hcRT.anchorMax = new Vector2(0, 1);
        hcRT.pivot = new Vector2(0, 1);
        hcRT.anchoredPosition = new Vector2(24, -20);
        hcRT.sizeDelta = new Vector2(230, 36);

        // Heart Icon
        GameObject heartGO = new GameObject("Heart_Icon");
        RectTransform heartRT = heartGO.AddComponent<RectTransform>();
        heartRT.SetParent(hcRT, false);
        heartRT.anchorMin = new Vector2(0, 0.5f);
        heartRT.anchorMax = new Vector2(0, 0.5f);
        heartRT.pivot = new Vector2(0, 0.5f);
        heartRT.anchoredPosition = new Vector2(0, 0);
        heartRT.sizeDelta = new Vector2(32, 32);

        Image heartImg = heartGO.AddComponent<Image>();
        if (heartSprite != null) heartImg.sprite = heartSprite;

        // Health Slider
        GameObject sliderGO = new GameObject("Health Slider");
        RectTransform sliderRT = sliderGO.AddComponent<RectTransform>();
        sliderRT.SetParent(hcRT, false);
        sliderRT.anchorMin = new Vector2(0, 0.5f);
        sliderRT.anchorMax = new Vector2(0, 0.5f);
        sliderRT.pivot = new Vector2(0, 0.5f);
        sliderRT.anchoredPosition = new Vector2(38, 0);
        sliderRT.sizeDelta = new Vector2(170, 24);

        Slider slider = sliderGO.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 7;
        slider.value = 7;
        slider.wholeNumbers = true;
        slider.interactable = false;

        // Background
        GameObject bgGO = new GameObject("Background");
        RectTransform bgRT = bgGO.AddComponent<RectTransform>();
        bgRT.SetParent(sliderRT, false);
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.sizeDelta = Vector2.zero;
        Image bgImg = bgGO.AddComponent<Image>();
        if (borderSprite != null)
        {
            bgImg.sprite = borderSprite;
            bgImg.type = Image.Type.Sliced;
        }
        bgImg.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

        // Fill Area
        GameObject fillAreaGO = new GameObject("Fill Area");
        RectTransform faRT = fillAreaGO.AddComponent<RectTransform>();
        faRT.SetParent(sliderRT, false);
        faRT.anchorMin = Vector2.zero;
        faRT.anchorMax = Vector2.one;
        faRT.sizeDelta = new Vector2(-4, -4);
        faRT.anchoredPosition = Vector2.zero;

        // Fill
        GameObject fillGO = new GameObject("Fill");
        RectTransform fillRT = fillGO.AddComponent<RectTransform>();
        fillRT.SetParent(faRT, false);
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.sizeDelta = Vector2.zero;
        Image fillImg = fillGO.AddComponent<Image>();
        if (fillSprite != null)
        {
            fillImg.sprite = fillSprite;
            fillImg.type = Image.Type.Sliced;
        }
        fillImg.color = new Color(1f, 0.2f, 0.25f, 1f); // Vibrant red

        slider.fillRect = fillRT;

        // ── Gold Container ────────────────────────────────────────────────────
        GameObject goldContainer = new GameObject("Coin_Container");
        RectTransform gcRT = goldContainer.AddComponent<RectTransform>();
        gcRT.SetParent(hudRT, false);
        gcRT.anchorMin = new Vector2(0, 1);
        gcRT.anchorMax = new Vector2(0, 1);
        gcRT.pivot = new Vector2(0, 1);
        gcRT.anchoredPosition = new Vector2(26, -58);
        gcRT.sizeDelta = new Vector2(140, 28);

        // Coin Icon
        GameObject coinGO = new GameObject("Coin_Icon");
        RectTransform coinRT = coinGO.AddComponent<RectTransform>();
        coinRT.SetParent(gcRT, false);
        coinRT.anchorMin = new Vector2(0, 0.5f);
        coinRT.anchorMax = new Vector2(0, 0.5f);
        coinRT.pivot = new Vector2(0, 0.5f);
        coinRT.anchoredPosition = new Vector2(0, 0);
        coinRT.sizeDelta = new Vector2(22, 22);

        Image coinImg = coinGO.AddComponent<Image>();
        if (coinSprite != null) coinImg.sprite = coinSprite;

        // Gold Amount Text
        GameObject textGO = new GameObject("Gold Amount Text");
        RectTransform textRT = textGO.AddComponent<RectTransform>();
        textRT.SetParent(gcRT, false);
        textRT.anchorMin = new Vector2(0, 0.5f);
        textRT.anchorMax = new Vector2(0, 0.5f);
        textRT.pivot = new Vector2(0, 0.5f);
        textRT.anchoredPosition = new Vector2(28, 0);
        textRT.sizeDelta = new Vector2(90, 26);

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "000";
        tmp.fontSize = 22;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.color = Color.white;
        if (fontAsset != null) tmp.font = fontAsset;

        // Wire references to PlayerHUD script
        SerializedObject hudSO = new SerializedObject(playerHUD);
        hudSO.FindProperty("healthSlider").objectReferenceValue = slider;
        hudSO.FindProperty("goldText").objectReferenceValue = tmp;
        hudSO.ApplyModifiedProperties();

        GameObject savedHud = PrefabUtility.SaveAsPrefabAsset(hudGO, PlayerHudPrefabPath);
        Object.DestroyImmediate(hudGO);
        Debug.Log("Created PlayerHUD.prefab successfully.");
        return savedHud;
    }

    private static void ProcessAllScenes(GameObject hudPrefab)
    {
        string currentActiveScene = EditorSceneManager.GetActiveScene().path;

        string[] scenes = {
            "Assets/Scenes/SampleScene.unity",
            "Assets/Scenes/HaHang.unity",
            "Assets/Scenes/Sample Scene 2.unity",
            "Assets/Scenes/New Scene.unity",
            "Assets/Scenes/OK.unity",
            "Assets/Scenes/final.unity"
        };

        foreach (var scenePath in scenes)
        {
            if (!File.Exists(scenePath)) continue;

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // 1. Process Player if present
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                // Cleanup duplicate components
                CleanupDuplicates<PlayerHealth>(player);
                CleanupDuplicates<Flash>(player);
                CleanupDuplicates<Knockback>(player);
                CleanupDuplicates<CapsuleCollider2D>(player);

                // Configure CapsuleCollider2D
                CapsuleCollider2D col = player.GetComponent<CapsuleCollider2D>();
                if (col == null) col = player.AddComponent<CapsuleCollider2D>();
                col.isTrigger = false;
                col.size = new Vector2(0.7f, 1.0f);
                col.offset = new Vector2(0f, -0.1f);

                // Configure PlayerHealth (Standardize to 7 HP)
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph == null) ph = player.AddComponent<PlayerHealth>();

                SerializedObject so = new SerializedObject(ph);
                so.FindProperty("maxHealth").intValue = 7;
                so.FindProperty("damageCooldown").floatValue = 0.6f;
                so.FindProperty("knockbackThrust").floatValue = 10f;
                so.ApplyModifiedProperties();

                // Flash & Knockback
                if (player.GetComponent<Flash>() == null) player.AddComponent<Flash>();
                if (player.GetComponent<Knockback>() == null) player.AddComponent<Knockback>();

                Debug.Log($"Synchronized Player in scene: {scenePath} (maxHealth = 7, Collider non-trigger).");
            }

            // 2. Process PlayerHUD
            var existingHuds = Object.FindObjectsByType<PlayerHUD>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var h in existingHuds)
            {
                Object.DestroyImmediate(h.gameObject);
            }

            if (hudPrefab != null)
            {
                GameObject hudInst = (GameObject)PrefabUtility.InstantiatePrefab(hudPrefab, scene);
                hudInst.name = "PlayerHUD";
            }

            // 3. Process [EconomyManager]
            var existingEcon = Object.FindObjectsByType<EconomyManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (existingEcon.Length == 0)
            {
                GameObject econGO = new GameObject("[EconomyManager]");
                econGO.AddComponent<EconomyManager>();
                Debug.Log($"Added [EconomyManager] to scene: {scenePath}");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        // Return to originally active scene
        if (!string.IsNullOrEmpty(currentActiveScene) && File.Exists(currentActiveScene))
        {
            EditorSceneManager.OpenScene(currentActiveScene, OpenSceneMode.Single);
        }
        else
        {
            EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);
        }
    }

    private static void CleanupDuplicates<T>(GameObject go) where T : Component
    {
        T[] components = go.GetComponents<T>();
        if (components.Length > 1)
        {
            for (int i = 1; i < components.Length; i++)
            {
                Object.DestroyImmediate(components[i]);
            }
            Debug.Log($"Removed {components.Length - 1} duplicate {typeof(T).Name} from {go.name}");
        }
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Replace('\\', '/').Split('/');
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
}
