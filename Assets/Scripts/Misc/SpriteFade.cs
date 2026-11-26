using System.Collections;
using UnityEngine;

/// <summary>Fades a SpriteRenderer to transparent then destroys the GameObject.</summary>
public class SpriteFade : MonoBehaviour
{
    [SerializeField] private float fadeTime = 0.4f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public IEnumerator SlowFadeRoutine()
    {
        float elapsed = 0f;
        float startAlpha = spriteRenderer.color.a;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, 0f, elapsed / fadeTime);
            Color c = spriteRenderer.color;
            spriteRenderer.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        Destroy(gameObject);
    }
}
