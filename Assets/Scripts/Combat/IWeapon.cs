/// <summary>Contract implemented by every equippable weapon (Sword, Bow, Staff, …).</summary>
public interface IWeapon
{
    void Attack();
    WeaponInfo GetWeaponInfo();
}
