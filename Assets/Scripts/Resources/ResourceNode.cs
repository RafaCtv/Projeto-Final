using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public sealed class ResourceNode : MonoBehaviour, IInteractable
{
    [SerializeField] private ResourceData resource;
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField, Min(1)] private int capacity = 30;
    [SerializeField, Min(1)] private int amountPerInteraction = 10;
    [Tooltip("Zero desabilita a regeneração. O intervalo começa quando o ponto se esgota.")]
    [SerializeField, Min(0)] private float regenerationDelay = 15;
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private Color depletedColor = new Color(0.35f, 0.35f, 0.35f, 0.6f);

    private int remaining;
    private Color availableColor;
    private Coroutine regeneration;

    public ResourceData Resource => resource;
    public int Remaining => remaining;
    public bool IsAvailable => isActiveAndEnabled && remaining > 0 && amountPerInteraction > 0
        && resourceManager != null && resourceManager.isActiveAndEnabled && resourceManager.IsRegistered(resource);
    public string InteractionDescription => resource == null ? string.Empty
        : "Coletar " + resource.DisplayName + " (" + remaining + " disponíveis)";

    public event Action<ResourceNode, ResourceData, int> Collected;

    private void Awake()
    {
        remaining = Mathf.Max(0, capacity);
        availableColor = visual != null ? visual.color : Color.white;
        UpdateVisual();
    }

    private void OnEnable() => StartRegenerationIfNeeded();

    public bool CanInteract(GameObject interactor) => IsAvailable;

    public void Interact(GameObject interactor)
    {
        if (!IsAvailable) return;
        int amount = Mathf.Min(amountPerInteraction, remaining);
        // Deduct from the node only after the shared stock accepted the addition.
        if (!resourceManager.TryAdd(resource, amount)) return;
        remaining -= amount;
        UpdateVisual();
        StartRegenerationIfNeeded();
        Collected?.Invoke(this, resource, amount);
    }

    private void StartRegenerationIfNeeded()
    {
        if (isActiveAndEnabled && remaining == 0 && regenerationDelay > 0 && regeneration == null)
            regeneration = StartCoroutine(Regenerate());
    }

    private IEnumerator Regenerate()
    {
        yield return new WaitForSeconds(regenerationDelay);
        remaining = Mathf.Max(0, capacity);
        regeneration = null;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (visual != null) visual.color = remaining > 0 ? availableColor : depletedColor;
    }

    private void OnDisable()
    {
        if (regeneration != null) StopCoroutine(regeneration);
        regeneration = null;
    }
}
