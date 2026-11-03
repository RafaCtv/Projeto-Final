using UnityEngine;

[CreateAssetMenu(menuName = "O Ultimo Bastiao/Game Cursor")]
public sealed class GameCursorSettings : ScriptableObject
{
    public Texture2D pointer;
    public Texture2D crosshair;
    public Color pointerColor = new Color(1, .82f, .42f);
    public Color crosshairColor = new Color(1, .2f, .2f);
    public Color outlineColor = new Color(.06f, .07f, .08f);
    public Vector2 pointerHotspot = new Vector2(.125f, .125f);
    public Vector2 crosshairHotspot = new Vector2(.5f, .5f);
    [Min(16)] public int size = 32;
}
