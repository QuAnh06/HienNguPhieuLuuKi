using UnityEngine;

/// <summary>
/// Arrow / bullet projectile. Moves along transform.right, destroys on range exceeded,
/// Indestructible objects, or any IDamageable (enemy / player depending on isEnemyProjectile).
/// </summary>
public class Projectile : MonoBehaviour
{
    [SerializeField] private float moveSpeed       = 22f;
    [SerializeField] private bool  isEnemyProjectile = false;
    [SerializeField] private float projectileRange  = 10f;
    [SerializeField] private int   damage           = 1;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        MoveProjectile();
        DetectFireDistance();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void UpdateProjectileRange(float range) => projectileRange = range;
    public void UpdateMoveSpeed(float speed)       => moveSpeed = speed;
    public void SetDamage(int dmg)                 => damage = dmg;

    // ── Collision ─────────────────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger) return;

        // Stop on hard walls
        if (other.GetComponent<Indestructible>())
        {
            Destroy(gameObject);
            return;
        }

        // Try to damage – walk up the parent chain (same pattern as MeleeHitbox)
        IDamageable damageable = null;
        foreach (var mb in other.GetComponentsInParent<MonoBehaviour>())
        {
            if (mb is IDamageable d) { damageable = d; break; }
        }

        if (damageable == null) return;

        bool hitsPlayer = other.GetComponentInParent<PlayerHealth>() != null;

        // Enemy projectile hits player; player projectile hits enemy (not player)
        if (isEnemyProjectile == hitsPlayer)
        {
            damageable.TakeDamage(damage, transform.right);
            Destroy(gameObject);
        }
    }

    // ── Movement ──────────────────────────────────────────────────────────────

    private void MoveProjectile()
    {
        transform.Translate(Vector3.right * (moveSpeed * Time.deltaTime));
    }

    private void DetectFireDistance()
    {
        if (Vector3.Distance(transform.position, startPosition) > projectileRange)
            Destroy(gameObject);
    }
}
