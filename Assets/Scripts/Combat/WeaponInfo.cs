using UnityEngine;

[CreateAssetMenu(menuName = "New Weapon")]
public class WeaponInfo : ScriptableObject
{
    public GameObject weaponPrefab;
    public float weaponCooldown = 0.5f;
    public int weaponDamage = 1;
    public float weaponRange = 10f;
}
