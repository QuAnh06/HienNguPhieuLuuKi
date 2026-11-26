using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GhostSetup
{
    private const string SourceGhostDir = "C:/Users/Laptop/Downloads/2drpgAssets/Sprites/Ghost";
    private const string TargetGhostDir = "Assets/Sprites/Enemy/Ghost";
    private const string PrefabDir = "Assets/Prefabs/Enemies";
    private const string Scene2Path = "Assets/Scenes/Sample Scene 2.unity";

    [MenuItem("Tools/Setup Ghost in Scene 2")]
    public static void SetupGhostInScene2()
    {
        Debug.Log("Starting Ghost & Scene 2 Setup...");

        // 1. Ensure folders
        EnsureFolder("Assets/Sprites");
        EnsureFolder("Assets/Sprites/Enemy");
        EnsureFolder(TargetGhostDir);
        EnsureFolder("Assets/Prefabs");
        EnsureFolder(PrefabDir);

        // 2. Load sprites
        Sprite ghostSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{TargetGhostDir}/Ghost.png");
        if (ghostSprite == null)
        {
            Debug.LogError($"Could not load Ghost sprite at {TargetGhostDir}/Ghost.png");
            return;
        }

        Sprite[] shadowSprites = AssetDatabase.LoadAllAssetsAtPath($"{TargetGhostDir}/Shadows_Sheet.png")
            .OfType<Sprite>().ToArray();
        Sprite shadowSprite = shadowSprites.FirstOrDefault(s => s.name == "Shadows_Sheet_0") 
                              ?? shadowSprites.FirstOrDefault();

        // 3. Create or update GhostBullet prefab
        GameObject bulletPrefab = SetupGhostBulletPrefab();

        // 4. Create Ghost prefab
        GameObject ghostPrefab = SetupGhostPrefab(ghostSprite, shadowSprite, bulletPrefab);

        // 5. Open Scene 2, place 2 Ghost instances, and save
        PopulateScene2(ghostPrefab);

        Debug.Log("Ghost setup completed successfully!");
    }

    private static GameObject SetupGhostBulletPrefab()
    {
        string bulletPrefabPath = $"{PrefabDir}/GhostBullet.prefab";
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(bulletPrefabPath);
        if (existingPrefab != null)
        {
            return existingPrefab;
        }

        GameObject bulletGO = new GameObject("GhostBullet");

        // Sprite
        SpriteRenderer sr = bulletGO.AddComponent<SpriteRenderer>();
        Sprite bulletSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Player/Bullet.png");
        if (bulletSprite != null) sr.sprite = bulletSprite;
        sr.color = new Color(0f, 0.9f, 1f, 1f); // Glowing Cyan / Neon Blue
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 10;

        // Collider (Trigger)
        CircleCollider2D col = bulletGO.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.12f;

        // Projectile script
        Projectile proj = bulletGO.AddComponent<Projectile>();
        proj.UpdateMoveSpeed(4.5f);
        proj.UpdateProjectileRange(16f);
        proj.SetDamage(1);

        // Use SerializedObject to set isEnemyProjectile = true
        SerializedObject so = new SerializedObject(proj);
        SerializedProperty isEnemyProp = so.FindProperty("isEnemyProjectile");
        if (isEnemyProp != null)
        {
            isEnemyProp.boolValue = true;
            so.ApplyModifiedProperties();
        }

        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(bulletGO, bulletPrefabPath);
        Object.DestroyImmediate(bulletGO);
        return savedPrefab;
    }

    private static GameObject SetupGhostPrefab(Sprite ghostSprite, Sprite shadowSprite, GameObject bulletPrefab)
    {
        string ghostPrefabPath = $"{PrefabDir}/Ghost.prefab";

        GameObject root = new GameObject("Ghost");

        // Rigidbody2D (Flying dynamic, 0 gravity)
        Rigidbody2D rb = root.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.mass = 1f;
        rb.linearDamping = 3f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // CircleCollider2D
        CircleCollider2D col = root.AddComponent<CircleCollider2D>();
        col.radius = 0.28f;
        col.offset = Vector2.zero;

        // Combat & Health components
        root.AddComponent<Knockback>();
        root.AddComponent<Flash>();
        root.AddComponent<EnemyPathfinding>();
        Shooter shooter = root.AddComponent<Shooter>();
        EnemyAI ai = root.AddComponent<EnemyAI>();
        EnemyHealth health = root.AddComponent<EnemyHealth>();

        // Configure Shooter serialized properties
        SerializedObject shooterSO = new SerializedObject(shooter);
        shooterSO.FindProperty("bulletPrefab").objectReferenceValue = bulletPrefab;
        shooterSO.FindProperty("bulletMoveSpeed").floatValue = 4.5f;
        shooterSO.FindProperty("burstCount").intValue = 2;
        shooterSO.FindProperty("projectilesPerBurst").intValue = 8;
        shooterSO.FindProperty("angleSpread").floatValue = 70f;
        shooterSO.FindProperty("startingDistance").floatValue = 0.25f;
        shooterSO.FindProperty("timeBetweenBursts").floatValue = 0.8f;
        shooterSO.FindProperty("restTime").floatValue = 1.5f;
        shooterSO.FindProperty("stagger").boolValue = true;
        shooterSO.FindProperty("oscillate").boolValue = true;
        shooterSO.ApplyModifiedProperties();

        // Configure EnemyHealth serialized properties (Health = 4)
        SerializedObject healthSO = new SerializedObject(health);
        var maxHpProp = healthSO.FindProperty("maxHealth");
        if (maxHpProp != null) maxHpProp.intValue = 4;
        healthSO.ApplyModifiedProperties();

        // Configure EnemyAI serialized properties
        SerializedObject aiSO = new SerializedObject(ai);
        aiSO.FindProperty("detectionRange").floatValue = 6f;
        aiSO.FindProperty("attackRange").floatValue = 4.5f;
        aiSO.FindProperty("attackCooldown").floatValue = 2.5f;
        aiSO.FindProperty("roamChangeInterval").floatValue = 2.5f;
        aiSO.ApplyModifiedProperties();

        // Configure EnemyPathfinding
        EnemyPathfinding ep = root.GetComponent<EnemyPathfinding>();
        SerializedObject epSO = new SerializedObject(ep);
        epSO.FindProperty("moveSpeed").floatValue = 1.8f;
        epSO.ApplyModifiedProperties();

        // Child Visual
        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform);
        visual.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = ghostSprite;
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 5;

        visual.AddComponent<GhostVisual>();

        // Child Shadow
        if (shadowSprite != null)
        {
            GameObject shadow = new GameObject("Shadow");
            shadow.transform.SetParent(root.transform);
            shadow.transform.localPosition = new Vector3(0f, -0.32f, 0f);

            SpriteRenderer shadowSr = shadow.AddComponent<SpriteRenderer>();
            shadowSr.sprite = shadowSprite;
            shadowSr.sortingLayerName = "Default";
            shadowSr.sortingOrder = 1;
            shadowSr.color = new Color(0f, 0f, 0f, 0.45f);
        }

        GameObject savedGhost = PrefabUtility.SaveAsPrefabAsset(root, ghostPrefabPath);
        Object.DestroyImmediate(root);
        return savedGhost;
    }

    private static void PopulateScene2(GameObject ghostPrefab)
    {
        Scene currentScene = EditorSceneManager.GetActiveScene();
        if (currentScene.path != Scene2Path)
        {
            currentScene = EditorSceneManager.OpenScene(Scene2Path, OpenSceneMode.Single);
        }

        // Remove any existing Ghost instances in the scene
        GameObject[] rootObjects = currentScene.GetRootGameObjects();
        foreach (var obj in rootObjects)
        {
            if (obj.name.StartsWith("Ghost"))
            {
                Object.DestroyImmediate(obj);
            }
        }

        // Instantiate 2 Ghost enemies
        GameObject ghost1 = (GameObject)PrefabUtility.InstantiatePrefab(ghostPrefab, currentScene);
        ghost1.name = "Ghost";
        ghost1.transform.position = new Vector3(-2f, 0f, 0f);

        GameObject ghost2 = (GameObject)PrefabUtility.InstantiatePrefab(ghostPrefab, currentScene);
        ghost2.name = "Ghost (1)";
        ghost2.transform.position = new Vector3(2f, 0f, 0f);

        EditorSceneManager.MarkSceneDirty(currentScene);
        EditorSceneManager.SaveScene(currentScene);

        Debug.Log($"Placed 2 Ghost instances into {Scene2Path} and saved.");
    }

    private static void CopyAssetIfMissing(string source, string target)
    {
        if (File.Exists(source) && !File.Exists(target))
        {
            File.Copy(source, target);
            Debug.Log($"Copied {source} -> {target}");
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
