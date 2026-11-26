using UnityEngine;

/// <summary>
/// Controls Ghost visual effects: floating sine-wave hover and horizontal sprite flipping.
/// </summary>
public class GhostVisual : MonoBehaviour
{
    [Header("Hover Bobbing")]
    [SerializeField] private float hoverFrequency = 2.5f;
    [SerializeField] private float hoverAmplitude = 0.08f;

    private float initialLocalY;
    private SpriteRenderer sr;
    private Rigidbody2D parentRb;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        initialLocalY = transform.localPosition.y;
        if (transform.parent != null)
        {
            parentRb = transform.parent.GetComponent<Rigidbody2D>();
        }
    }

    private void Update()
    {
        // Smooth sine wave hovering
        float newY = initialLocalY + Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
        transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);

        // Face player if exists, or face parent movement velocity
        if (sr != null)
        {
            if (PlayerController.Instance != null)
            {
                float dx = PlayerController.Instance.transform.position.x - transform.position.x;
                if (Mathf.Abs(dx) > 0.05f)
                {
                    sr.flipX = (dx < 0);
                }
            }
            else if (parentRb != null && Mathf.Abs(parentRb.linearVelocity.x) > 0.05f)
            {
                sr.flipX = (parentRb.linearVelocity.x < 0);
            }
        }
    }
}
