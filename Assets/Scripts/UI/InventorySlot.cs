using UnityEngine;

/// <summary>Attached to each slot in the Hotbar; holds the weapon data for that slot.</summary>
public class InventorySlot : MonoBehaviour
{
    [SerializeField] private WeaponInfo weaponInfo;

    public WeaponInfo GetWeaponInfo() => weaponInfo;
}
