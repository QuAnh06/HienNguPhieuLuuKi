using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class AnimationAndVisualsSetup
{
    private const string HitSpriteSheetPath = "Assets/Sprites/Player/Side animations/spr_player_right_hit.png";
    private const string HitAnimPath = "Assets/Animations/Player/Player_Hit.anim";
    private const string PlayerControllerPath = "Assets/Animations/Player/Player.controller";
    private const string GoldCoinPrefabPath = "Assets/Prefabs/Gold Coin.prefab";
    private const string HeartPickupPrefabPath = "Assets/Prefabs/Pickups/HeartPickup.prefab";

    [MenuItem("Tools/Setup Visuals and Death System")]
    public static void RunVisualsSetup()
    {
        Debug.Log("Starting Player Hit Animation & Visuals Setup...");

        // 1. Create or update Player_Hit.anim
        AnimationClip hitClip = CreatePlayerHitClip();

        // 2. Add Hurt state and transitions to Player.controller
        if (hitClip != null)
        {
            SetupPlayerAnimator(hitClip);
        }

        // 3. Update Gold Coin Prefab (scale 0.7, sortingOrder 5)
        TuneGoldCoinPrefab();

        // 4. Update Heart Pickup Prefab (scale 0.85, sortingOrder 5, HeartVisual)
        TuneHeartPickupPrefab();

        Debug.Log("Visuals and Death System Setup completed successfully!");
    }

    private static AnimationClip CreatePlayerHitClip()
    {
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(HitSpriteSheetPath)
            .OfType<Sprite>()
            .OrderBy(s => s.name)
            .ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogError($"No sprites found at {HitSpriteSheetPath}!");
            return null;
        }

        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(HitAnimPath);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, HitAnimPath);
        }

        clip.frameRate = 12;

        EditorCurveBinding binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Length];
        float frameDuration = 1f / 12f;

        for (int i = 0; i < sprites.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i * frameDuration,
                value = sprites[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
        EditorUtility.SetDirty(clip);
        Debug.Log($"Created/Updated {HitAnimPath} with {sprites.Length} frames.");
        return clip;
    }

    private static void SetupPlayerAnimator(AnimationClip hitClip)
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(PlayerControllerPath);
        if (controller == null)
        {
            Debug.LogError($"Player controller not found at {PlayerControllerPath}!");
            return;
        }

        // Add Hurt trigger parameter if missing
        if (!controller.parameters.Any(p => p.name == "Hurt"))
        {
            controller.AddParameter("Hurt", AnimatorControllerParameterType.Trigger);
        }

        var rootSm = controller.layers[0].stateMachine;

        // Find or create Hurt state
        AnimatorState hurtState = rootSm.states.FirstOrDefault(s => s.state.name == "Hurt").state;
        if (hurtState == null)
        {
            hurtState = rootSm.AddState("Hurt");
        }
        hurtState.motion = hitClip;

        // AnyState -> Hurt transition
        bool hasAnyTransition = rootSm.anyStateTransitions.Any(t => t.destinationState == hurtState);
        if (!hasAnyTransition)
        {
            var anyTrans = rootSm.AddAnyStateTransition(hurtState);
            anyTrans.AddCondition(AnimatorConditionMode.If, 0, "Hurt");
            anyTrans.hasExitTime = false;
            anyTrans.duration = 0f;
        }

        // Hurt -> Idle transition
        AnimatorState idleState = rootSm.states.FirstOrDefault(s => s.state.name == "Idle").state;
        if (idleState != null)
        {
            bool hasIdleTransition = hurtState.transitions.Any(t => t.destinationState == idleState);
            if (!hasIdleTransition)
            {
                var exitTrans = hurtState.AddTransition(idleState);
                exitTrans.hasExitTime = true;
                exitTrans.exitTime = 1f;
                exitTrans.duration = 0.05f;
            }
        }

        EditorUtility.SetDirty(controller);
        Debug.Log("Configured Hurt state and transitions in Player.controller.");
    }

    private static void TuneGoldCoinPrefab()
    {
        if (!File.Exists(GoldCoinPrefabPath)) return;

        GameObject root = PrefabUtility.LoadPrefabContents(GoldCoinPrefabPath);
        root.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

        SpriteRenderer sr = root.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 5;
        }

        Pickup pickup = root.GetComponent<Pickup>();
        if (pickup != null)
        {
            SerializedObject so = new SerializedObject(pickup);
            so.FindProperty("pickUpDistance").floatValue = 2.5f;
            so.FindProperty("moveSpeed").floatValue = 2f;
            so.FindProperty("accelartionRate").floatValue = 1.2f;
            so.FindProperty("magnetDelay").floatValue = 0.5f;
            so.ApplyModifiedProperties();
        }

        PrefabUtility.SaveAsPrefabAsset(root, GoldCoinPrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        Debug.Log("Tuned Gold Coin.prefab (scale: 0.7, sortingOrder: 5, gentle magnet).");
    }

    private static void TuneHeartPickupPrefab()
    {
        if (!File.Exists(HeartPickupPrefabPath)) return;

        GameObject root = PrefabUtility.LoadPrefabContents(HeartPickupPrefabPath);
        root.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

        SpriteRenderer sr = root.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 5;
        }

        if (root.GetComponent<HeartVisual>() == null)
        {
            root.AddComponent<HeartVisual>();
        }

        Pickup pickup = root.GetComponent<Pickup>();
        if (pickup != null)
        {
            SerializedObject so = new SerializedObject(pickup);
            so.FindProperty("pickUpDistance").floatValue = 2.5f;
            so.FindProperty("moveSpeed").floatValue = 2f;
            so.FindProperty("accelartionRate").floatValue = 1.2f;
            so.FindProperty("magnetDelay").floatValue = 0.5f;
            so.ApplyModifiedProperties();
        }

        PrefabUtility.SaveAsPrefabAsset(root, HeartPickupPrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        Debug.Log("Tuned HeartPickup.prefab (scale: 0.85, sortingOrder: 5, HeartVisual, gentle magnet).");
    }
}
