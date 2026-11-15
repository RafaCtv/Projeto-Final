using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public sealed class HUDLayout : MonoBehaviour
{
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private RectTransform baseCard;
    [SerializeField] private RectTransform playerCard;
    [SerializeField] private RectTransform waveCard;
    [SerializeField] private RectTransform resourcesCard;
    [SerializeField] private RectTransform[] feedbackLines;
    [Tooltip("Largura máxima da faixa superior, em unidades do Canvas.")]
    [SerializeField, Min(300)] private float maxWidth = 820;
    private float lastWidth = -1;
    private void OnEnable() { lastWidth = -1; RefreshLayout(); }
    private void OnRectTransformDimensionsChange() => lastWidth = -1;
    private void LateUpdate() => RefreshLayout();
    public void RefreshLayout()
    {
        if (canvasRect == null || baseCard == null || waveCard == null || resourcesCard == null || canvasRect.rect.width < 100) return;
        float width = Mathf.Min(maxWidth, canvasRect.rect.width - 24);
        if (Mathf.Abs(lastWidth - width) < .1f) return;
        lastWidth = width;
        var rect = (RectTransform)transform;
        bool narrow = width < 700;
        rect.sizeDelta = new Vector2(width, narrow ? 140 : 64);
        if (narrow)
        {
            float first = width * .43f;
            PlaceHealthCards(0, 0, first - 5, 70);
            Place(waveCard, first + 5, 0, width * .57f - 5, 70);
            Place(resourcesCard, 0, 80, width, 60);
        }
        else
        {
            float first = playerCard != null ? width * .42f : width * .3f, second = playerCard != null ? width * .3f : width * .38f;
            PlaceHealthCards(0, 0, first - 7, 64);
            Place(waveCard, first + 3, 0, second - 7, 64);
            Place(resourcesCard, first + second + 6, 0, width - first - second - 6, 64);
        }
        LayoutRebuilder.MarkLayoutForRebuild(resourcesCard);
        if (feedbackLines != null)
            for (int i = 0; i < feedbackLines.Length; i++)
                if (feedbackLines[i] != null) feedbackLines[i].anchoredPosition = new Vector2(0, -(narrow ? 168 : 92) - i * 28);
    }
    private void PlaceHealthCards(float x, float y, float width, float height)
    {
        if (playerCard == null) { Place(baseCard, x, y, width, height); return; }
        float half = (width - 6) * .5f;
        Place(playerCard, x, y, half, height);
        Place(baseCard, x + half + 6, y, half, height);
    }
    private static void Place(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(width, height);
    }
}
