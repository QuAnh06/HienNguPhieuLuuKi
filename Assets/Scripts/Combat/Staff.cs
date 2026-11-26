using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Staff weapon — implements IWeapon.
/// Rotates to follow the mouse. On Attack(), plays the Attack anim and spawns a MagicLaser.
/// </summary>
public class Staff : MonoBehaviour, IWeapon
{
    [SerializeField] private WeaponInfo weaponInfo;
    [SerializeField] private GameObject magicLaserPrefab;
    [SerializeField] private Transform  laserSpawnPoint;

    private Animator myAnimator;
    static readonly int ATTACK_HASH = Animator.StringToHash("Attack");

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
        if (myAnimator != null) myAnimator.SetTrigger(ATTACK_HASH);
        // Spawn laser directly (also serves as fallback if no anim event)
        SpawnLaser();
    }

    // Called by Animation Event (optional override of direct spawn above)
    public void SpawnLaserAnimEvent() => SpawnLaser();

    // ── Private ───────────────────────────────────────────────────────────────

    private void SpawnLaser()
    {
        if (magicLaserPrefab == null || laserSpawnPoint == null) return;

        GameObject laser = Instantiate(magicLaserPrefab, laserSpawnPoint.position, Quaternion.identity);
        var ml = laser.GetComponent<MagicLaser>();
        if (ml != null)
        {
            int   dmg   = weaponInfo != null ? weaponInfo.weaponDamage : 1;
            float range = weaponInfo != null ? weaponInfo.weaponRange   : 8f;
            ml.Initialize(range, dmg);
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
