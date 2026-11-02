using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "O Ultimo Bastiao/Building", fileName = "Building")]
public sealed class BuildingData : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField, TextArea] private string description;
    [SerializeField] private Sprite icon;
    [SerializeField] private BuildingInstance prefab;
    [SerializeField] private ResourceCost[] costs = Array.Empty<ResourceCost>();

    public string Id => id;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;
    public BuildingInstance Prefab => prefab;
    public IReadOnlyList<ResourceCost> Costs => costs;

    public string CostDescription
    {
        get
        {
            var text = new StringBuilder();
            foreach (var cost in costs)
            {
                if (text.Length > 0) text.Append(" + ");
                text.Append(cost.Amount).Append(' ').Append(cost.Resource != null ? cost.Resource.DisplayName : "recurso inválido");
            }
            return text.Length == 0 ? "Sem custo" : text.ToString();
        }
    }
}
