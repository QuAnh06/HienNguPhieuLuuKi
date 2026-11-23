using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SlimeSetup
{
    private const string EnemySpriteDir = "Assets/Sprites/Enemy/Blue Slime";
    private const string AnimDir = "Assets/Animations/Enemy/Slime";
    private const string PrefabDir = "Assets/Prefabs/Enemies";

    [MenuItem("Tools/Setup Blue Slime")]
    public static void SetupSlimeAll()
    {
        try
        {
            Debug.Log("[SlimeSetup] Starting SetupSlimeAll...");
            if (!Directory.Exists(AnimDir)) Directory.CreateDirectory(AnimDir);
            if (!Directory.Exists(PrefabDir)) Directory.CreateDirectory(PrefabDir);

            // 1. Build Animation Clips
            Debug.Log("[SlimeSetup] Creating Clips...");
            AnimationClip idleClip = CreateClip("Slime_Idle", $"{EnemySpriteDir}/spr_Blue_slime_idle.png", 8, true);
            AnimationClip walkClip = CreateClip("Slime_Walk", $"{EnemySpriteDir}/spr_Blue_slime_walk.png", 8, true);
            AnimationClip attackClip = CreateClip("Slime_Attack", $"{EnemySpriteDir}/spr_Blue_slime_attack.png", 12, false);
            AnimationClip hurtClip = CreateClip("Slime_Hurt", $"{EnemySpriteDir}/spr_Blue_slime_hurt.png", 10, false);
            AnimationClip deathClip = CreateClip("Slime_Death", $"{EnemySpriteDir}/spr_Blue_slime_death.png", 12, false);

            // 2. Build Animator Controller
            Debug.Log("[SlimeSetup] Creating Animator Controller...");
            AnimatorController controller = CreateSlimeAnimatorController(idleClip, walkClip, attackClip, hurtClip, deathClip);

            // 3. Create BlueSlime Prefab
            Debug.Log("[SlimeSetup] Creating Prefab...");
            GameObject prefab = CreateSlimePrefab(controller, idleClip);

            // 4. Setup Player Components (PlayerHealth, Knockback, Flash)
            Debug.Log("[SlimeSetup] Setting up Player components...");
            SetupPlayerComponents();

            // 5. Spawn Slimes in Scene
            Debug.Log("[SlimeSetup] Spawning Slimes in scene...");
            SpawnSlimesInScene(prefab);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();

            Debug.Log("[SlimeSetup] Successfully created Blue Slime animations, animator controller, prefab, and spawned instances in scene!");
        }
        catch (Exception ex)
        {
            Debug.LogError("[SlimeSetup] Exception during setup: " + ex);
        }
    }


    private static AnimationClip CreateClip(string clipName, string sheetPath, float frameRate, bool loop)
    {
        string clipPath = $"{AnimDir}/{clipName}.anim";

        // Load all sprites from sheet
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath)
            .OfType<Sprite>()
            .OrderBy(s => s.name, new NaturalSortComparer())
            .ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogError($"No sprites found at {sheetPath}!");
            return null;
        }

        AnimationClip clip = new AnimationClip();
        clip.frameRate = frameRate;

        EditorCurveBinding binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Length];
        float frameDuration = 1f / frameRate;

        for (int i = 0; i < sprites.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i * frameDuration,
                value = sprites[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        if (loop)
        {
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
        }

        AssetDatabase.CreateAsset(clip, clipPath);
        return clip;
    }

    private static AnimatorController CreateSlimeAnimatorController(
        AnimationClip idle, AnimationClip walk, AnimationClip attack, AnimationClip hurt, AnimationClip death)
    {
        string controllerPath = $"{AnimDir}/Slime.controller";
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        // Parameters
        controller.AddParameter("isWalking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Hurt", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("isDead", AnimatorControllerParameterType.Bool);

        AnimatorStateMachine sm = controller.layers[0].stateMachine;

        // States
        AnimatorState idleState = sm.AddState("Idle");
        idleState.motion = idle;

        AnimatorState walkState = sm.AddState("Walk");
        walkState.motion = walk;

        AnimatorState attackState = sm.AddState("Attack");
        attackState.motion = attack;

        AnimatorState hurtState = sm.AddState("Hurt");
        hurtState.motion = hurt;

        AnimatorState deathState = sm.AddState("Death");
        deathState.motion = death;

        // Default state
        sm.defaultState = idleState;

        // Idle <-> Walk
        var toWalk = idleState.AddTransition(walkState);
        toWalk.AddCondition(AnimatorConditionMode.If, 0, "isWalking");
        toWalk.hasExitTime = false;
        toWalk.duration = 0f;

        var toIdle = walkState.AddTransition(idleState);
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isWalking");
        toIdle.hasExitTime = false;
        toIdle.duration = 0f;

        // AnyState -> Attack
        var anyToAttack = sm.AddAnyStateTransition(attackState);
        anyToAttack.AddCondition(AnimatorConditionMode.If, 0, "Attack");
        anyToAttack.hasExitTime = false;
        anyToAttack.duration = 0f;

        var attackToIdle = attackState.AddTransition(idleState);
        attackToIdle.hasExitTime = true;
        attackToIdle.exitTime = 1f;
        attackToIdle.duration = 0f;

        // AnyState -> Hurt
        var anyToHurt = sm.AddAnyStateTransition(hurtState);
        anyToHurt.AddCondition(AnimatorConditionMode.If, 0, "Hurt");
        anyToHurt.hasExitTime = false;
        anyToHurt.duration = 0f;

        var hurtToIdle = hurtState.AddTransition(idleState);
        hurtToIdle.hasExitTime = true;
        hurtToIdle.exitTime = 1f;
        hurtToIdle.duration = 0f;

        // AnyState -> Death
        var anyToDeath = sm.AddAnyStateTransition(deathState);
        anyToDeath.AddCondition(AnimatorConditionMode.If, 0, "Death");
        anyToDeath.hasExitTime = false;
        anyToDeath.duration = 0f;

        return controller;
    }

    private static GameObject CreateSlimePrefab(AnimatorController controller, AnimationClip idleClip)
    {
        string prefabPath = $"{PrefabDir}/BlueSlime.prefab";

        GameObject go = new GameObject("BlueSlime");

        // 1. SpriteRenderer
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 0;

        // Grab first sprite of idle for visual preview
        Sprite[] idleSprites = AssetDatabase.LoadAllAssetsAtPath($"{EnemySpriteDir}/spr_Blue_slime_idle.png")
            .OfType<Sprite>().ToArray();
        if (idleSprites.Length > 0) sr.sprite = idleSprites[0];

        // 2. Animator
        Animator anim = go.AddComponent<Animator>();
        anim.runtimeAnimatorController = controller;

        // 3. Rigidbody2D
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.mass = 1f;
        rb.linearDamping = 2f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // 4. CircleCollider2D
        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.28f;
        col.offset = new Vector2(0f, 0.12f);

        // 5. Custom Enemy Components
        go.AddComponent<Knockback>();
        go.AddComponent<Flash>();
        go.AddComponent<EnemyPathfinding>();
        go.AddComponent<Slime>();
        go.AddComponent<EnemyAI>();
        go.AddComponent<EnemyHealth>();

        // Save as Prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        UnityEngine.Object.DestroyImmediate(go);
        return prefab;
    }

    private static void SetupPlayerComponents()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            PlayerController pc = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            if (pc != null) player = pc.gameObject;
        }

        if (player != null)
        {
            if (player.GetComponent<Knockback>() == null) player.AddComponent<Knockback>();
            if (player.GetComponent<Flash>() == null) player.AddComponent<Flash>();
            if (player.GetComponent<PlayerHealth>() == null) player.AddComponent<PlayerHealth>();
            EditorUtility.SetDirty(player);
        }
    }

    private static void SpawnSlimesInScene(GameObject prefab)
    {
        if (prefab == null) return;

        // Group under an Enemies parent container
        GameObject enemiesParent = GameObject.Find("Enemies");
        if (enemiesParent == null)
        {
            enemiesParent = new GameObject("Enemies");
            Undo.RegisterCreatedObjectUndo(enemiesParent, "Create Enemies Container");
        }

        // Clean up previous test slimes if any
        for (int i = enemiesParent.transform.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(enemiesParent.transform.GetChild(i).gameObject);
        }

        // Spawn 2 slimes in accessible grass clearings
        Vector3[] spawnPositions = new Vector3[]
        {
            new Vector3(-2.2f, 0.4f, 0f), // Clearing near central crossroad
            new Vector3(1.2f, -1.2f, 0f)  // Southern clearing
        };

        for (int i = 0; i < spawnPositions.Length; i++)
        {
            GameObject slime = (GameObject)PrefabUtility.InstantiatePrefab(prefab, enemiesParent.transform);
            slime.name = $"BlueSlime_{i + 1}";
            slime.transform.position = spawnPositions[i];
            Undo.RegisterCreatedObjectUndo(slime, "Spawn Blue Slime");
        }
    }

    // Custom comparer to sort frames naturally (e.g. _0, _1, ... _10)
    private class NaturalSortComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == null || y == null) return 0;
            string[] xParts = x.Split('_');
            string[] yParts = y.Split('_');
            if (xParts.Length > 0 && yParts.Length > 0 &&
                int.TryParse(xParts[xParts.Length - 1], out int xNum) &&
                int.TryParse(yParts[yParts.Length - 1], out int yNum))
            {
                return xNum.CompareTo(yNum);
            }
            return string.Compare(x, y, StringComparison.Ordinal);
        }
    }
}
