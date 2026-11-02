using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
[DisallowMultipleComponent]
public sealed class ResourceManager : MonoBehaviour
{
    [Serializable]
    private struct InitialStock
    {
        public ResourceData resource;
        [Min(0)] public int amount;
    }

    [SerializeField] private InitialStock[] initialStock = Array.Empty<InitialStock>();
    private readonly Dictionary<ResourceData, int> quantities = new Dictionary<ResourceData, int>();
    private bool purchaseInProgress;

    public event Action<ResourceData, int> ResourceChanged;

    private void Awake()
    {
        quantities.Clear();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in initialStock)
        {
            if (entry.resource == null || string.IsNullOrWhiteSpace(entry.resource.Id)
                || entry.amount < 0 || !ids.Add(entry.resource.Id))
            {
                quantities.Clear();
                Debug.LogError("ResourceManager: configure recursos com IDs únicos e quantidades iniciais não negativas.", this);
                enabled = false;
                return;
            }
            quantities.Add(entry.resource, entry.amount);
        }
    }

    public bool IsRegistered(ResourceData resource) => resource != null && quantities.ContainsKey(resource);

    public int GetAmount(ResourceData resource) =>
        resource != null && quantities.TryGetValue(resource, out int amount) ? amount : 0;

    public bool HasAmount(ResourceData resource, int amount) =>
        amount >= 0 && IsRegistered(resource) && GetAmount(resource) >= amount;

    public bool TryAdd(ResourceData resource, int amount)
    {
        if (purchaseInProgress || amount < 0 || !IsRegistered(resource)) return false;
        int current = quantities[resource];
        if (amount > int.MaxValue - current) return false;
        if (amount == 0) return true;
        quantities[resource] = current + amount;
        ResourceChanged?.Invoke(resource, quantities[resource]);
        return true;
    }

    public bool TryConsume(ResourceData resource, int amount)
    {
        if (purchaseInProgress || !HasAmount(resource, amount)) return false;
        if (amount == 0) return true;
        quantities[resource] -= amount;
        ResourceChanged?.Invoke(resource, quantities[resource]);
        return true;
    }

    public bool CanAfford(IReadOnlyList<ResourceCost> costs, out string reason) =>
        TryValidateCosts(costs, out _, out reason);

    public bool TryConsumeCosts(IReadOnlyList<ResourceCost> costs) => TryPurchase(costs, () => true);

    // All balances are committed before notifications. Installation failure restores them
    // without publishing an intermediate payment or allowing reentrant stock mutations.
    public bool TryPurchase(IReadOnlyList<ResourceCost> costs, Func<bool> install)
    {
        if (install == null || !TryValidateCosts(costs, out var totals, out _)) return false;
        purchaseInProgress = true;
        bool committed = false;
        try
        {
            foreach (var cost in totals) quantities[cost.Key] -= cost.Value;
            committed = install();
        }
        finally
        {
            if (!committed)
                foreach (var cost in totals) quantities[cost.Key] += cost.Value;
            purchaseInProgress = false;
        }
        if (!committed) return false;
        foreach (var cost in totals)
            if (cost.Value > 0) ResourceChanged?.Invoke(cost.Key, quantities[cost.Key]);
        return true;
    }

    private bool TryValidateCosts(IReadOnlyList<ResourceCost> costs,
        out Dictionary<ResourceData, int> totals, out string reason)
    {
        totals = new Dictionary<ResourceData, int>();
        reason = string.Empty;
        if (purchaseInProgress || costs == null)
        {
            reason = "Operação indisponível.";
            return false;
        }
        foreach (var cost in costs)
        {
            if (!IsRegistered(cost.Resource) || cost.Amount < 0)
            {
                reason = "Custo com recurso ou quantidade inválidos.";
                return false;
            }
            totals.TryGetValue(cost.Resource, out int previous);
            if (cost.Amount > int.MaxValue - previous)
            {
                reason = "Custo excede o limite permitido.";
                return false;
            }
            totals[cost.Resource] = previous + cost.Amount;
        }
        foreach (var cost in totals)
        {
            int current = quantities[cost.Key];
            if (current >= cost.Value) continue;
            reason = "Faltam " + (cost.Value - current) + " " + cost.Key.DisplayName + ".";
            return false;
        }
        return true;
    }
}
