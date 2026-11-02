using System.Collections.Generic;
using ProjetoFinal.Input;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Transform interactionOrigin;
    [SerializeField, Min(0.05f)] private float interactionRange = 1.5f;
    [SerializeField] private LayerMask interactionLayers = ~0;
    [SerializeField] private InteractionPromptUI promptUI;

    private readonly List<Collider2D> overlaps = new List<Collider2D>();
    private InputAction interact;
    private IInteractable target;

    private Vector2 Origin => interactionOrigin != null ? interactionOrigin.position : transform.position;

    private void Awake() => interact = GameInput.Gameplay.FindAction("Interact", true);

    private void OnEnable() => interact?.Enable();

    private void Update()
    {
        RefreshTarget();
        // A press is immediate even if the source action has a Hold interaction.
        if (interact.WasPressedThisFrame()) TryInteract();
    }

    private void RefreshTarget()
    {
        target = FindNearest();
        if (promptUI != null) promptUI.SetTarget(target);
    }

    private IInteractable FindNearest()
    {
        var filter = new ContactFilter2D { useTriggers = true };
        filter.SetLayerMask(interactionLayers);
        overlaps.Clear();
        Physics2D.OverlapCircle(Origin, interactionRange, filter, overlaps);
        IInteractable nearest = null;
        float bestDistance = float.PositiveInfinity;
        EntityId bestId = default;
        foreach (var hit in overlaps)
        {
            if (hit == null || hit.transform.IsChildOf(transform)) continue;
            var candidate = hit.GetComponentInParent<IInteractable>();
            if (!(candidate is MonoBehaviour behaviour) || !behaviour.isActiveAndEnabled
                || !candidate.CanInteract(gameObject)) continue;
            float distance = (hit.ClosestPoint(Origin) - Origin).sqrMagnitude;
            if (distance > interactionRange * interactionRange) continue;
            EntityId id = hit.GetEntityId();
            if (distance < bestDistance || (distance == bestDistance && id.CompareTo(bestId) < 0))
            {
                bestDistance = distance;
                bestId = id;
                nearest = candidate;
            }
        }
        return nearest;
    }

    public bool TryInteract()
    {
        // Re-query so a moved, disabled or destroyed object cannot use stale focus.
        target = FindNearest();
        if (target == null)
        {
            if (promptUI != null) promptUI.SetTarget(null);
            return false;
        }
        target.Interact(gameObject);
        RefreshTarget();
        return true;
    }

    private void OnDisable()
    {
        interact?.Disable();
        target = null;
        if (promptUI != null) promptUI.SetTarget(null);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Origin, interactionRange);
    }
}
