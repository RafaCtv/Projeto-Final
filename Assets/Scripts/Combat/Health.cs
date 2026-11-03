using System;
using UnityEngine;

[DefaultExecutionOrder(-50)]
[DisallowMultipleComponent]
public sealed class Health : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxHealth = 100;
    public int Maximum => maxHealth;
    public int Current { get; private set; }
    public bool IsAlive => Current > 0;
    public event Action<Health> Changed;
    public event Action<Health> Died;
    public event Action<Health, int> Damaged;
    private void Awake() => Initialize(maxHealth);
    // Spawn initialization; gameplay exposes damage, not healing or repair.
    public void Initialize(int maximum)
    {
        maxHealth = Mathf.Max(1, maximum);
        Current = maxHealth;
        Changed?.Invoke(this);
    }
    public bool TakeDamage(int amount)
    {
        if (amount <= 0 || !IsAlive || !isActiveAndEnabled) return false;
        int applied = Mathf.Min(amount, Current);
        Current -= applied;
        Changed?.Invoke(this);
        Damaged?.Invoke(this, applied);
        if (Current == 0) Died?.Invoke(this);
        return true;
    }
}
