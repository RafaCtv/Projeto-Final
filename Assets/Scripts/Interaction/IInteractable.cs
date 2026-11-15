using System;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    string InteractionDescription { get; }
    bool CanInteract(GameObject interactor);
    void Interact(GameObject interactor);
}

public enum InteractionItemsKind { Cost, Reward }

// Dados do aviso compacto de interação: "[E] Torre  🪵10  ⚙5".
public readonly struct InteractionPrompt
{
    public readonly string Action;
    public readonly IReadOnlyList<ResourceCost> Items;
    public readonly InteractionItemsKind Kind;

    public InteractionPrompt(string action, IReadOnlyList<ResourceCost> items, InteractionItemsKind kind)
    {
        Action = action;
        Items = items ?? Array.Empty<ResourceCost>();
        Kind = kind;
    }
}

// Opcional: interativos que mostram ícones de custo/recompensa em vez de texto longo.
public interface IInteractionPromptSource
{
    InteractionPrompt Prompt { get; }
}
