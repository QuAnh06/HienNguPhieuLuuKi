using System.Collections;
using UnityEngine;

public class Knockback : MonoBehaviour
{
    public bool GettingKnockedBack { get; private set; }

    [SerializeField] private float knockBackTime = 0.2f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void GetKnockedBack(Vector2 hitDirection, float knockBackThrust)
    {
        if (rb == null) return;
        GettingKnockedBack = true;
        hitDirection.Normalize();
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(hitDirection * (knockBackThrust * rb.mass), ForceMode2D.Impulse);
        StartCoroutine(KnockRoutine());
    }

    public void GetKnockedBack(Transform damageSource, float knockBackThrust)
    {
        if (damageSource == null) return;
        Vector2 diff = ((Vector2)transform.position - (Vector2)damageSource.position).normalized;
        GetKnockedBack(diff, knockBackThrust);
    }

    private IEnumerator KnockRoutine()
    {
        yield return new WaitForSeconds(knockBackTime);
        if (rb != null) rb.linearVelocity = Vector2.zero;
        GettingKnockedBack = false;
    }
}
