using UnityEngine;

public class Slime : MonoBehaviour, IEnemy
{
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackRadius = 0.9f;
    [SerializeField] private LayerMask targetLayers = ~0;

    public void Attack()
    {
        // Check for targets in melee attack range
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRadius, targetLayers);
        foreach (Collider2D hit in hits)
        {
            if (hit == null || hit.transform == transform || hit.transform.IsChildOf(transform)) continue;
            // Don't damage other enemies
            if (hit.GetComponentInParent<EnemyAI>() != null) continue;

            foreach (MonoBehaviour component in hit.GetComponentsInParent<MonoBehaviour>())
            {
                if (component is IDamageable damageable)
                {
                    Vector2 hitDir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
                    damageable.TakeDamage(attackDamage, hitDir);
                    break;
                }
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Contact damage when touching player directly
        if (collision.gameObject.GetComponent<PlayerController>() != null)
        {
            foreach (MonoBehaviour component in collision.gameObject.GetComponentsInParent<MonoBehaviour>())
            {
                if (component is IDamageable damageable)
                {
                    Vector2 hitDir = ((Vector2)collision.transform.position - (Vector2)transform.position).normalized;
                    damageable.TakeDamage(attackDamage, hitDir);
                    break;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
