using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Bow weapon — implements IWeapon.
/// Rotates to follow the mouse. On Attack(), plays the Fire anim and spawns an Arrow.
/// </summary>
public class Bow : MonoBehaviour, IWeapon
{
    [SerializeField] private WeaponInfo weaponInfo;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform arrowSpawnPoint;

    private Animator myAnimator;
    static readonly int FIRE_HASH = Animator.StringToHash("Fire");

    private void Awake()
    {
        myAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        MouseFollowWithOffset();
    }

    public WeaponInfo GetWeaponInfo() => weaponInfo;

    public void Attack()
    {
        if (myAnimator != null) myAnimator.SetTrigger(FIRE_HASH);
        SpawnArrow();
    }

    // Called by Animation Event (optional); also called directly from Attack() as fallback.
    public void SpawnArrowAnimEvent() => SpawnArrow();

    // ── Private ───────────────────────────────────────────────────────────────

    private void SpawnArrow()
    {
        if (arrowPrefab == null || arrowSpawnPoint == null) return;

        GameObject arrow = Instantiate(
            arrowPrefab,
            arrowSpawnPoint.position,
            ActiveWeapon.Instance != null
                ? ActiveWeapon.Instance.transform.rotation
                : transform.rotation);

        var proj = arrow.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.UpdateProjectileRange(weaponInfo != null ? weaponInfo.weaponRange : 10f);
            proj.SetDamage(weaponInfo != null ? weaponInfo.weaponDamage : 1);
        }
    }

    private void MouseFollowWithOffset()
    {
        if (Mouse.current == null || Camera.main == null) return;
        if (PlayerController.Instance == null || ActiveWeapon.Instance == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mousePos);
        mouseWorld.z = 0f;

        Vector2 dir = (mouseWorld - ActiveWeapon.Instance.transform.position).normalized;
        if (dir.sqrMagnitude > 0.001f)
        {
            ActiveWeapon.Instance.transform.right = dir;
            ActiveWeapon.Instance.transform.localScale = dir.x < 0
                ? new Vector3(1f, -1f, 1f)
                : new Vector3(1f, 1f, 1f);
        }
    }
}
