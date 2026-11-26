using System.Collections;
using UnityEngine;

public class Flash : MonoBehaviour
{
    [SerializeField] private Material whiteFlashMat;
    [SerializeField] private float restoreDefaultMatTime = 0.15f;
    [SerializeField] private Color flashColor = new Color(1f, 0.4f, 0.4f, 1f);

    private Material defaultMat;
    private SpriteRenderer spriteRenderer;
    private Color defaultColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            defaultMat = spriteRenderer.material;
            defaultColor = spriteRenderer.color;
        }
    }

    public float GetRestoreMatTime() => restoreDefaultMatTime;

    public IEnumerator FlashRoutine()
    {
        if (spriteRenderer == null) yield break;

        if (whiteFlashMat != null)
        {
            spriteRenderer.material = whiteFlashMat;
            yield return new WaitForSeconds(restoreDefaultMatTime);
            spriteRenderer.material = defaultMat;
        }
        else
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(restoreDefaultMatTime);
            spriteRenderer.color = defaultColor;
        }
    }
}
