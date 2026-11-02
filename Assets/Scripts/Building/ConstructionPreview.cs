using UnityEngine;

// A composed watchpost needs several renderers; the original point still owns the purchase.
[ExecuteAlways]
public sealed class ConstructionPreview : MonoBehaviour
{
    [SerializeField] private ConstructionPoint point;
    [SerializeField] private GameObject preview;
    [SerializeField] private Transform viewer;
    [SerializeField] private float previewDistance = 3.4f;
    [SerializeField] private LineRenderer marker;
    private SpriteRenderer[] parts;
    private GameObject cachedPreview;
    private void Awake() { if (preview != null) parts = preview.GetComponentsInChildren<SpriteRenderer>(true); }
    private void LateUpdate()
    {
        if (point == null || preview == null) return;
        bool visible = Application.isPlaying && !point.IsOccupied && (viewer == null || Mathf.Abs(viewer.position.x-transform.position.x) < previewDistance);
        if (marker != null) marker.enabled = !point.IsOccupied;
        if (preview.activeSelf != visible) preview.SetActive(visible);
        // The original single-sprite availability renderer is retained for its references.
        var source = transform.Find("AvailabilityPreview");
        if (source != null)
        {
            var renderer = source.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
                if (visible)
                {
                    if (cachedPreview != preview || parts == null || parts.Length == 0) { parts = preview.GetComponentsInChildren<SpriteRenderer>(true); cachedPreview = preview; }
                    foreach (var part in parts) { var color = renderer.color; color.a = .22f; part.color = color; }
                }
                if (marker != null) { marker.startColor = marker.endColor = new Color(renderer.color.r,renderer.color.g,renderer.color.b,.7f); }
            }
        }
    }
}
