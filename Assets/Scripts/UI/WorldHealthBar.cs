using UnityEngine;
using TMPro;

// Reads the existing Health events; never changes damage or maximum health.
[DisallowMultipleComponent]
[RequireComponent(typeof(Health))]
public sealed class WorldHealthBar : MonoBehaviour
{
    [SerializeField] private Vector2 offset = new Vector2(0, 2.5f);
    [SerializeField, Min(.1f)] private float width = 1.25f;
    [SerializeField, Min(.02f)] private float height = .12f;
    private Health health;
    private Transform bar;
    private SpriteRenderer background;
    private SpriteRenderer fill;
    private Sprite pixel;
    private TextMeshPro value;

    public bool IsVisible => background != null && background.enabled;

    private void Awake()
    {
        health = GetComponent<Health>();
        pixel = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0, .5f), 1);
        var root = new GameObject("DamageHealthBar");
        root.transform.SetParent(transform, false);
        bar = root.transform;
        background = CreatePart("Background", new Color(.06f, .08f, .09f, .95f), 100);
        fill = CreatePart("Fill", Color.white, 101);
        var label = new GameObject("HealthValue");
        label.transform.SetParent(bar, false);
        value = label.AddComponent<TextMeshPro>();
        value.fontSize = 1.7f;
        value.alignment = TextAlignmentOptions.Center;
        value.rectTransform.sizeDelta = new Vector2(width + .5f, .35f);
        value.transform.localPosition = new Vector3(width * .5f, .25f, 0);
        value.renderer.sortingLayerName = "Foreground";
        value.renderer.sortingOrder = 102;
        Refresh(health);
    }

    private SpriteRenderer CreatePart(string partName, Color color, int order)
    {
        var part = new GameObject(partName);
        part.transform.SetParent(bar, false);
        var renderer = part.AddComponent<SpriteRenderer>();
        renderer.sprite = pixel;
        renderer.color = color;
        renderer.sortingLayerName = "Foreground";
        renderer.sortingOrder = order;
        return renderer;
    }

    private void OnEnable()
    {
        health.Changed += Refresh;
        Refresh(health);
    }

    private void Refresh(Health current)
    {
        bool visible = current.IsAlive && current.Current < current.Maximum;
        background.enabled = fill.enabled = visible;
        value.enabled = visible;
        value.text = current.Current + " / " + current.Maximum;
        float ratio = Mathf.Clamp01((float)current.Current / current.Maximum);
        background.transform.localScale = new Vector3(width, height, 1);
        fill.transform.localPosition = new Vector3(.025f, 0, 0);
        fill.transform.localScale = new Vector3((width - .05f) * ratio, Mathf.Max(.01f, height - .04f), 1);
        fill.color = HealthBarUI.ColorFor(ratio);
    }

    private void LateUpdate()
    {
        // World alignment keeps the bar readable regardless of character facing/scale.
        bar.position = transform.position + (Vector3)offset - Vector3.right * (width * .5f);
        bar.rotation = Quaternion.identity;
        Vector3 scale = transform.lossyScale;
        bar.localScale = new Vector3(scale.x == 0 ? 1 : 1 / scale.x, scale.y == 0 ? 1 : 1 / scale.y, 1);
    }

    private void OnDisable()
    {
        if (health != null) health.Changed -= Refresh;
        if (background != null) background.enabled = fill.enabled = false;
        if (value != null) value.enabled = false;
    }

    private void OnDestroy()
    {
        if (pixel != null) Destroy(pixel);
    }
}
