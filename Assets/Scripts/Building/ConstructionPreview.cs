using UnityEngine;

// Mostra a prévia da construção só quando o jogador está perto de um ponto livre.
// No editor (fora do Play) a prévia fica visível, para facilitar posicionar os pontos.
public sealed class ConstructionPreview : MonoBehaviour
{
    [SerializeField] private ConstructionPoint point;
    [Tooltip("Objeto da prévia; vazio = a prévia configurada no ConstructionPoint.")]
    [SerializeField] private GameObject preview;
    [SerializeField] private Transform viewer;
    [SerializeField] private float previewDistance = 3.4f;
    [SerializeField] private LineRenderer marker;

    public bool IsShowing => Preview != null && Preview.activeSelf;
    private GameObject Preview => preview != null ? preview : point != null ? point.PreviewObject : null;

    private void Awake()
    {
        if (point == null) point = GetComponent<ConstructionPoint>();
        if (viewer == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null) viewer = player.transform;
        }
    }

    private void LateUpdate()
    {
        var target = Preview;
        if (point == null || target == null) return;
        bool near = viewer != null && Mathf.Abs(viewer.position.x - transform.position.x) < previewDistance;
        bool visible = !point.IsOccupied && near;
        if (target.activeSelf != visible) target.SetActive(visible);
        if (marker != null)
        {
            marker.enabled = !point.IsOccupied;
            var color = point.CurrentColor;
            marker.startColor = marker.endColor = new Color(color.r, color.g, color.b, .7f);
        }
    }
}
