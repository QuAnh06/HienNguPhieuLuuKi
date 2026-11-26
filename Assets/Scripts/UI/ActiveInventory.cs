using UnityEngine;

/// <summary>
/// Manages the three-slot weapon Hotbar (1 = Sword, 2 = Bow, 3 = Staff).
/// Listens to the Inventory.Keyboard action and equips the matching weapon via ActiveWeapon.
/// </summary>
public class ActiveInventory : MonoBehaviour
{
    public static ActiveInventory Instance { get; private set; }

    private int activeSlotIndexNum = 0;
    private PlayerControls playerControls;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        playerControls = new PlayerControls();
    }

    private void OnEnable()  => playerControls.Enable();
    private void OnDisable() => playerControls.Disable();

    private void Start()
    {
        playerControls.Inventory.Keyboard.performed +=
            ctx => ToggleActiveSlot((int)ctx.ReadValue<float>());

        EquipStartingWeapon();
    }

    public void EquipStartingWeapon() => ToggleActiveHighlight(0);

    // ── Private ───────────────────────────────────────────────────────────────

    private void ToggleActiveSlot(int numValue) => ToggleActiveHighlight(numValue - 1);

    private void ToggleActiveHighlight(int indexNum)
    {
        activeSlotIndexNum = indexNum;

        // Deactivate all border highlights
        foreach (Transform slot in transform)
        {
            if (slot.childCount > 0)
                slot.GetChild(0).gameObject.SetActive(false);
        }

        // Activate selected slot's border
        if (indexNum < transform.childCount)
            transform.GetChild(indexNum).GetChild(0).gameObject.SetActive(true);

        ChangeActiveWeapon();
    }

    private void ChangeActiveWeapon()
    {
        // Destroy the old prefab weapon (but NOT PlayerCombat which lives on Player)
        if (ActiveWeapon.Instance != null &&
            ActiveWeapon.Instance.CurrentActiveWeapon != null &&
            !(ActiveWeapon.Instance.CurrentActiveWeapon is PlayerCombat))
        {
            Destroy(ActiveWeapon.Instance.CurrentActiveWeapon.gameObject);
        }

        if (indexNum_outOfRange()) { ActiveWeapon.Instance?.WeaponNull(); return; }

        var slot = transform.GetChild(activeSlotIndexNum)
                            .GetComponentInChildren<InventorySlot>();
        WeaponInfo info = slot?.GetWeaponInfo();

        if (info == null) { ActiveWeapon.Instance?.WeaponNull(); return; }

        // Slot 0 (Sword) — reuse PlayerCombat that already lives on the Player
        if (activeSlotIndexNum == 0)
        {
            var sword = PlayerController.Instance != null
                ? PlayerController.Instance.GetComponent<PlayerCombat>()
                : null;
            if (sword != null) ActiveWeapon.Instance?.NewWeapon(sword);
            return;
        }

        // Slots 1+ — Instantiate weapon prefab under ActiveWeapon transform
        if (info.weaponPrefab != null && ActiveWeapon.Instance != null)
        {
            GameObject spawned = Instantiate(info.weaponPrefab,
                                             ActiveWeapon.Instance.transform);
            var mb = spawned.GetComponent<MonoBehaviour>();
            if (mb != null) ActiveWeapon.Instance.NewWeapon(mb);
        }
    }

    private bool indexNum_outOfRange() =>
        activeSlotIndexNum < 0 || activeSlotIndexNum >= transform.childCount;
}
