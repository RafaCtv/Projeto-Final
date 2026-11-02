using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BuildingInstance : MonoBehaviour
{
    public BuildingData Definition { get; private set; }
    private Collider2D[] passThroughColliders;
    private Health health;
    public event Action<BuildingInstance> Destroyed;

    public void Initialize(BuildingData definition, Collider2D[] survivorColliders)
    {
        Definition = definition;
        passThroughColliders = survivorColliders;
    }

    private void OnEnable()
    {
        health = GetComponent<Health>();
        if (health != null) health.Died += OnDied;
        if (passThroughColliders == null) return;
        foreach (var structure in GetComponentsInChildren<Collider2D>())
            foreach (var survivor in passThroughColliders)
                if (structure != null && survivor != null && survivor.isActiveAndEnabled)
                    Physics2D.IgnoreCollision(structure, survivor, true);
    }
    private void OnDied(Health value)
    {
        foreach (var collider in GetComponentsInChildren<Collider2D>()) collider.enabled = false;
        Destroyed?.Invoke(this);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
    private void OnDisable() { if (health != null) health.Died -= OnDied; }
}
