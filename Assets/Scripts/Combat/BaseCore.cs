using System;
using UnityEngine;

[RequireComponent(typeof(Health))]
public sealed class BaseCore : MonoBehaviour
{
    public Health Health => GetComponent<Health>();
    public event Action Destroyed;
    private void OnEnable() => Health.Died += OnDied;
    private void OnDisable() => Health.Died -= OnDied;
    private void OnDied(Health health) => Destroyed?.Invoke();
}
