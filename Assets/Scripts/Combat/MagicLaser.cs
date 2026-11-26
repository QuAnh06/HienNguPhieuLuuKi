using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Magic laser beam that grows toward the mouse, damages every enemy it passes through
/// (once per enemy), stops on Indestructible walls, then fades out.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(SpriteFade))]
public class MagicLaser : MonoBehaviour
{
    [SerializeField] private float laserGrowTime = 2f;
    [SerializeField] private Transform laserHead;

    private float laserRange;
    private int   damage;
    private bool  isGrowing = true;

    private SpriteRenderer   sr;
    private CapsuleCollider2D cap;

    // Track enemies already hit so we only deal damage once per target
    private readonly HashSet<IDamageable> alreadyHit = new HashSet<IDamageable>();

    private void Awake()
    {
        sr  = GetComponent<SpriteRenderer>();
        cap = GetComponent<CapsuleCollider2D>();
    }

    /// <summary>Call immediately after Instantiate.</summary>
    public void Initialize(float range, int dmg)
    {
        laserRange = range;
        damage     = dmg;
        LaserFaceMouse();
        StartCoroutine(GrowRoutine());
    }

    // ── Trigger ───────────────────────────────────────────────────────────────

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.isTrigger) return;

        // Stop growing if the laser hits a wall
        if (other.GetComponent<Indestructible>())
        {
            isGrowing = false;
            return;
        }

        // Damage each IDamageable exactly once
        foreach (var mb in other.GetComponentsInParent<MonoBehaviour>())
        {
            if (mb is IDamageable damageable && alreadyHit.Add(damageable))
            {
                damageable.TakeDamage(damage, transform.right);
                break;
            }
        }
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private IEnumerator GrowRoutine()
    {
        float elapsed = 0f;

        while (sr.size.x < laserRange && isGrowing)
        {
            elapsed += Time.deltaTime;
            float x = Mathf.Lerp(1f, laserRange, elapsed / laserGrowTime);

            sr.size = new Vector2(x, 1f);

            cap.size   = new Vector2(x,       cap.size.y);
            cap.offset = new Vector2(x / 2f,  cap.offset.y);

            if (laserHead != null)
                laserHead.localPosition = new Vector3(x, 0f, 0f);

            yield return null;
        }

        // Fade out head and laser beam
        if (laserHead != null)
        {
            var headFade = laserHead.GetComponent<SpriteFade>();
            if (headFade != null) StartCoroutine(headFade.SlowFadeRoutine());
        }

        StartCoroutine(GetComponent<SpriteFade>().SlowFadeRoutine());
    }

    private void LaserFaceMouse()
    {
        if (Mouse.current == null || Camera.main == null) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(
            Mouse.current.position.ReadValue());
        mouseWorld.z = 0f;

        Vector3 dir = mouseWorld - transform.position;
        if (dir.sqrMagnitude > 0.001f)
            transform.right = dir;
    }
}
