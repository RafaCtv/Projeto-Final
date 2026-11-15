using System;
using UnityEngine;

[RequireComponent(typeof(Health))]
public sealed class BaseCore : MonoBehaviour
{
    [Tooltip("Colisores que atravessam a base (o sobrevivente), já que ela fica no meio do mapa.")]
    [SerializeField] private Collider2D[] passThroughColliders;
    public Health Health => GetComponent<Health>();
    public event Action Destroyed;
    private void OnEnable()
    {
        Health.Died += OnDied;
        if (passThroughColliders == null) return;
        foreach (var structure in GetComponentsInChildren<Collider2D>())
            foreach (var other in passThroughColliders)
                if (structure != null && other != null) Physics2D.IgnoreCollision(structure, other, true);
    }
    private void OnDisable() => Health.Died -= OnDied;
    private void OnDied(Health health) => Destroyed?.Invoke();
}
