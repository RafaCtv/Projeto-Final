using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ResourceHUD : MonoBehaviour
{
    [Serializable]
    private struct Entry
    {
        public ResourceData resource;
        public Image icon;
        public Text label;
    }

    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private Entry[] entries = Array.Empty<Entry>();
    [SerializeField] private ResourceNode[] resourceNodes = Array.Empty<ResourceNode>();
    [SerializeField] private Text collectionFeedback;
    [SerializeField] private bool compactValues;
    [SerializeField, Min(0.1f)] private float feedbackDuration = 2;
    private Coroutine feedback;

    private void OnEnable()
    {
        if (resourceManager != null) resourceManager.ResourceChanged += OnResourceChanged;
        foreach (var node in resourceNodes)
            if (node != null) node.Collected += OnCollected;
        RefreshAll();
    }

    private void OnResourceChanged(ResourceData resource, int amount)
    {
        foreach (var entry in entries)
            if (entry.resource == resource && entry.label != null)
                entry.label.text = Format(resource, amount);
    }

    private void RefreshAll()
    {
        foreach (var entry in entries)
        {
            if (entry.resource == null) continue;
            if (entry.icon != null)
            {
                entry.icon.sprite = entry.resource.Icon;
                entry.icon.preserveAspect = true;
            }
            if (entry.label != null)
                entry.label.text = Format(entry.resource, resourceManager != null ? resourceManager.GetAmount(entry.resource) : 0);
        }
    }

    private string Format(ResourceData resource, int amount) => compactValues ? amount.ToString() : resource.DisplayName + "  " + amount;

    private void OnCollected(ResourceNode node, ResourceData resource, int amount)
    {
        if (collectionFeedback == null) return;
        if (feedback != null) StopCoroutine(feedback);
        collectionFeedback.text = "+" + amount + " " + resource.DisplayName;
        feedback = StartCoroutine(ClearFeedback());
    }

    private IEnumerator ClearFeedback()
    {
        yield return new WaitForSeconds(feedbackDuration);
        if (collectionFeedback != null) collectionFeedback.text = string.Empty;
        feedback = null;
    }

    private void OnDisable()
    {
        if (resourceManager != null) resourceManager.ResourceChanged -= OnResourceChanged;
        foreach (var node in resourceNodes)
            if (node != null) node.Collected -= OnCollected;
        if (feedback != null) StopCoroutine(feedback);
        feedback = null;
        if (collectionFeedback != null) collectionFeedback.text = string.Empty;
    }
}
