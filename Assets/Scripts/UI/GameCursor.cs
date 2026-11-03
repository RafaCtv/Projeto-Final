using System.Collections.Generic;
using ProjetoFinal.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// A single native cursor follows the menu/game scene transitions.
public sealed class GameCursor : MonoBehaviour
{
    private static GameCursor instance;
    private GameCursorSettings settings;
    private GameManager game;
    private Texture2D pointer;
    private Texture2D crosshair;
    private bool applied;
    private bool aiming;
    private readonly List<Collider2D> overlaps = new List<Collider2D>();
    public bool IsTargetingEnemy => aiming;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => instance = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void StartCursor()
    {
        if (instance != null) return;
        var existing = FindAnyObjectByType<GameCursor>();
        if (existing != null) { instance = existing; return; }
        new GameObject("GameCursor").AddComponent<GameCursor>();
    }

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
        settings = Resources.Load<GameCursorSettings>("UI/GameCursorSettings");
        if (settings == null || settings.pointer == null || settings.crosshair == null)
        {
            Debug.LogWarning("GameCursor: configure as duas texturas em UI/GameCursorSettings.", this);
            enabled = false;
            return;
        }
        pointer = CreateTexture(settings.pointer, settings.pointerColor);
        crosshair = CreateTexture(settings.crosshair, settings.crosshairColor);
    }

    private void OnEnable()
    {
        if (instance != this) return;
        SceneManager.sceneLoaded += OnSceneLoaded;
        game = FindAnyObjectByType<GameManager>();
        Apply(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        game = FindAnyObjectByType<GameManager>();
        applied = false;
        Apply(false);
    }

    private void LateUpdate()
    {
        if (!Application.isFocused || Mouse.current == null) return;
        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        Apply(CanAimAt(Mouse.current.position.ReadValue(), overUI));
    }

    public bool CanAimAt(Vector2 screenPosition, bool pointerOverUI)
    {
        if (pointerOverUI || game == null || !game.IsRunning || Time.timeScale <= 0 || GameInput.IsRebinding) return false;
        var camera = Camera.main;
        if (camera == null || !camera.pixelRect.Contains(screenPosition)) return false;
        var ray = camera.ScreenPointToRay(screenPosition);
        var plane = new Plane(Vector3.forward, Vector3.zero);
        if (!plane.Raycast(ray, out float distance)) return false;
        var filter = new ContactFilter2D { useTriggers = false };
        filter.SetLayerMask(1 << LayerMask.NameToLayer("Zombie"));
        overlaps.Clear();
        Physics2D.OverlapPoint(ray.GetPoint(distance), filter, overlaps);
        foreach (var hit in overlaps)
        {
            var zombie = hit.GetComponentInParent<ZombieController>();
            if (zombie != null && zombie.IsAlive) return true;
        }
        return false;
    }

    private void Apply(bool target)
    {
        if (pointer == null || crosshair == null || (applied && target == aiming)) return;
        aiming = target;
        applied = true;
        var texture = target ? crosshair : pointer;
        var hotspot = (target ? settings.crosshairHotspot : settings.pointerHotspot) * texture.width;
        hotspot.x = Mathf.Clamp(hotspot.x, 0, texture.width - 1);
        hotspot.y = Mathf.Clamp(hotspot.y, 0, texture.height - 1);
        Cursor.SetCursor(texture, hotspot, CursorMode.Auto);
    }

    private Texture2D CreateTexture(Texture2D source, Color tint)
    {
        int size = Mathf.Clamp(settings.size, 16, 64);
        var colors = new Color[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                colors[y * size + x] = source.GetPixelBilinear((x + .5f) / size, (y + .5f) / size) * tint;
        var outlined = (Color[])colors.Clone();
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int index = y * size + x;
                if (colors[index].a > .05f) continue;
                float alpha = 0;
                for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                        alpha = Mathf.Max(alpha, colors[Mathf.Clamp(y + dy, 0, size - 1) * size + Mathf.Clamp(x + dx, 0, size - 1)].a);
                var border = settings.outlineColor;
                // A solid one-pixel border keeps the small native cursor legible.
                border.a *= alpha > .1f ? 1 : 0;
                outlined[index] = border;
            }
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = source.name + "_Cursor";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(outlined);
        texture.Apply(false, false);
        return texture;
    }

    private void OnApplicationFocus(bool focused)
    {
        applied = false;
        if (focused) Apply(false);
        else Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void OnDisable()
    {
        if (instance != this) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        applied = aiming = false;
    }

    private void OnDestroy()
    {
        if (instance != this) return;
        instance = null;
        if (pointer != null) Destroy(pointer);
        if (crosshair != null) Destroy(crosshair);
    }
}
