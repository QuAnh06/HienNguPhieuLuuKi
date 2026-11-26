using UnityEngine;

/// <summary>
/// Sets a custom crosshair cursor for the game using the RPG cursor sprite.
/// </summary>
public class CursorManager : MonoBehaviour
{
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Vector2 hotspot = new Vector2(16f, 16f);

    private void Start()
    {
        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
        }
    }
}
