using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPathfinding : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2.2f;

    private Rigidbody2D rb;
    private Vector2 moveDir;
    private Knockback knockback;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        knockback = GetComponent<Knockback>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (knockback != null && knockback.GettingKnockedBack) return;

        rb.MovePosition(rb.position + moveDir * (moveSpeed * Time.fixedDeltaTime));

        if (spriteRenderer != null)
        {
            if (moveDir.x < -0.01f) spriteRenderer.flipX = true;
            else if (moveDir.x > 0.01f) spriteRenderer.flipX = false;
        }
    }

    public void MoveTo(Vector2 direction) => moveDir = direction.normalized;
    public void StopMoving() => moveDir = Vector2.zero;
}
