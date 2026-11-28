using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float knockbackThrust = 10f;

    private int currentHealth;
    private Knockback knockback;
    private Flash flash;
    private EnemyAI ai;
    private Collider2D col;
    private Rigidbody2D rb;

    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        knockback = GetComponent<Knockback>();
        flash = GetComponent<Flash>();
        ai = GetComponent<EnemyAI>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount, Vector2 hitDirection)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;

        if (knockback != null) knockback.GetKnockedBack(hitDirection, knockbackThrust);
        if (flash != null) StartCoroutine(flash.FlashRoutine());
        if (ai != null) ai.OnHurt();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (col != null) col.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (ai != null) ai.OnDeath();

        GetComponent<PickupSpawner>()?.DropItems();

        // Destroy gameobject after death animation completes
        Destroy(gameObject, 1.3f);
    }
}
