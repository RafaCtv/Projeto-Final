using TMPro;
using UnityEngine;

// Contagem regressiva sobre um ponto de coleta esgotado.
public sealed class ResourceRespawnTimer : MonoBehaviour
{
    [SerializeField] private ResourceNode node;
    [SerializeField] private TMP_Text label;

    private void Awake()
    {
        if (node == null) node = GetComponentInParent<ResourceNode>();
    }

    private void LateUpdate()
    {
        if (node == null || label == null) return;
        bool show = node.IsRegenerating;
        if (label.enabled != show) label.enabled = show;
        if (!show) return;
        string text = Mathf.CeilToInt(node.RegenerationRemaining) + "s";
        if (label.text != text) label.text = text;
    }
}
