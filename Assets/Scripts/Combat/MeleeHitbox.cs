using System.Collections.Generic;
using UnityEngine;

/// <summary>Applies one melee hit per damageable during an attack.</summary>
public sealed class MeleeHitbox : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private LayerMask targetLayers = ~0;
    [SerializeField] private Vector2 boxSize = new Vector2(0.9f, 0.75f);
    [SerializeField] private float forwardOffset = 0.55f;
    private readonly HashSet<IDamageable> hitThisSwing = new HashSet<IDamageable>();

    public void Strike(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.001f) direction = Vector2.right;
        direction.Normalize();
        Vector2 center = (Vector2)transform.position + direction * forwardOffset;
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, boxSize, 0f, targetLayers);
        hitThisSwing.Clear();
        foreach (Collider2D hit in hits)
        {
            if (!hit || hit.transform == transform || hit.transform.IsChildOf(transform)) continue;
            foreach (MonoBehaviour component in hit.GetComponentsInParent<MonoBehaviour>())
                if (component is IDamageable damageable && hitThisSwing.Add(damageable))
                {
                    damageable.TakeDamage(damage, direction);
                    break;
                }
        }
    }
}
