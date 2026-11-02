using System;
using UnityEngine;

[Serializable]
public struct ResourceCost
{
    [SerializeField] private ResourceData resource;
    [SerializeField, Min(0)] private int amount;

    public ResourceData Resource => resource;
    public int Amount => amount;

    public ResourceCost(ResourceData resource, int amount)
    {
        this.resource = resource;
        this.amount = amount;
    }
}
