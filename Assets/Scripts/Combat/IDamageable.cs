using UnityEngine;

/// <summary>Implemented by actors that can receive melee damage.</summary>
public interface IDamageable
{
    void TakeDamage(int amount, Vector2 hitDirection);
}
