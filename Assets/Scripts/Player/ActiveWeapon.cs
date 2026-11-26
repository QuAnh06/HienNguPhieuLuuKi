using System.Collections;
using UnityEngine;

/// <summary>
/// Singleton that sits on a child GameObject called "ActiveWeapon" under Player.
/// Routes attack input to whichever IWeapon is currently equipped.
/// Dash input remains in PlayerCombat.
/// </summary>
public class ActiveWeapon : MonoBehaviour
{
    public static ActiveWeapon Instance { get; private set; }

    public MonoBehaviour CurrentActiveWeapon { get; private set; }

    private PlayerControls playerControls;
    private float timeBetweenAttacks;
    private bool attackButtonDown;
    private bool isAttacking;

    protected void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        playerControls = new PlayerControls();
    }

    private void OnEnable()  => playerControls.Enable();
    private void OnDisable() => playerControls.Disable();

    private void Start()
    {
        playerControls.Combat.Attack.started  += _ => attackButtonDown = true;
        playerControls.Combat.Attack.canceled += _ => attackButtonDown = false;
        AttackCooldown();
    }

    private void Update()
    {
        if (attackButtonDown && !isAttacking && CurrentActiveWeapon != null)
        {
            AttackCooldown();
            (CurrentActiveWeapon as IWeapon)?.Attack();
        }
    }

    // ── Public API ──────────────────────────────────────────────────────────

    public void NewWeapon(MonoBehaviour newWeapon)
    {
        CurrentActiveWeapon = newWeapon;
        AttackCooldown();
        timeBetweenAttacks = (newWeapon as IWeapon)?.GetWeaponInfo()?.weaponCooldown ?? 0.5f;

        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        // Show or hide the sword visual depending on weapon type
        bool isSword = newWeapon is PlayerCombat;
        GetComponentInParent<PlayerCombat>()?.SetSwordActive(isSword);
    }

    public void WeaponNull()
    {
        CurrentActiveWeapon = null;
        GetComponentInParent<PlayerCombat>()?.SetSwordActive(false);
    }

    // ── Internal ─────────────────────────────────────────────────────────────

    private void AttackCooldown()
    {
        isAttacking = true;
        StopAllCoroutines();
        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(timeBetweenAttacks);
        isAttacking = false;
    }
}
