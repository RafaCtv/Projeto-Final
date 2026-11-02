using System;
using System.Collections.Generic;
using UnityEngine;

// Quais layers entram no trigger é definido pela Layer Collision Matrix.
[RequireComponent(typeof(Collider2D))]
public sealed class TargetSensor : MonoBehaviour
{
    [Tooltip("Considera só colisores sólidos.")]
    [SerializeField] private bool ignoreTriggers = true;
    private readonly List<Collider2D> inside = new List<Collider2D>();
    private Rigidbody2D owner;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        owner = GetComponentInParent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((ignoreTriggers && other.isTrigger) || (owner != null && other.attachedRigidbody == owner) || inside.Contains(other)) return;
        inside.Add(other);
    }

    private void OnTriggerExit2D(Collider2D other) => inside.Remove(other);

    private void OnDisable() => inside.Clear();

    public Health FindTarget(Predicate<Health> accept)
    {
        for (int i = inside.Count - 1; i >= 0; i--)
        {
            var collider = inside[i];
            if (collider == null || !collider.enabled || !collider.gameObject.activeInHierarchy) { inside.RemoveAt(i); continue; }
            var health = collider.GetComponentInParent<Health>();
            if (health != null && health.IsAlive && (accept == null || accept(health))) return health;
        }
        return null;
    }
}
