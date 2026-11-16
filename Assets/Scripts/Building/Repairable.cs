using System;
using ProjetoFinal.Audio;
using TMPro;
using UnityEngine;

// Estrutura danificada pode ser consertada com E, pagando recursos proporcionais ao dano.
[RequireComponent(typeof(Health))]
public sealed class Repairable : MonoBehaviour, IInteractable, IInteractionPromptSource
{
    [Tooltip("Custo para consertar de 0 até a vida máxima; o custo real é proporcional ao dano.")]
    [SerializeField] private ResourceCost[] fullRepairCost = Array.Empty<ResourceCost>();
    [SerializeField] private ResourceManager resources;
    [SerializeField] private AudioClip repairSound;
    [SerializeField] private Vector2 popupOffset = new Vector2(0, 2.5f);
    private Health health;
    private ResourceCost[] currentCost;
    private InteractionPromptUI feedback;

    public Health Health => health;

    private void Awake()
    {
        health = GetComponent<Health>();
        if (resources == null) resources = FindAnyObjectByType<ResourceManager>();
        currentCost = new ResourceCost[fullRepairCost.Length];
    }

    public ResourceCost[] CurrentCost()
    {
        float missing = health.Maximum > 0 ? 1 - (float)health.Current / health.Maximum : 0;
        for (int i = 0; i < fullRepairCost.Length; i++)
        {
            int full = fullRepairCost[i].Amount;
            int amount = full > 0 && missing > 0 ? Mathf.Max(1, Mathf.CeilToInt(full * missing)) : 0;
            currentCost[i] = new ResourceCost(fullRepairCost[i].Resource, amount);
        }
        return currentCost;
    }

    public string InteractionDescription => "Consertar";
    public bool CanInteract(GameObject interactor) =>
        isActiveAndEnabled && health != null && health.IsAlive && health.Current < health.Maximum && resources != null;
    public InteractionPrompt Prompt => new InteractionPrompt("Consertar", CurrentCost(), InteractionItemsKind.Cost);

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;
        if (!resources.TryConsumeCosts(CurrentCost()))
        {
            if (feedback == null) feedback = FindAnyObjectByType<InteractionPromptUI>();
            if (feedback != null) feedback.ShowFeedback("Recursos insuficientes");
            return;
        }
        int repaired = health.Repair(health.Maximum - health.Current);
        SoundEffects.Play(repairSound);
        var popup = new GameObject("RepairPopup");
        popup.transform.position = transform.position + (Vector3)popupOffset;
        var text = popup.AddComponent<TextMeshPro>();
        text.text = "+" + repaired;
        text.fontSize = 3.5f;
        text.alignment = TextAlignmentOptions.Center;
        text.rectTransform.sizeDelta = new Vector2(3, 1);
        text.color = new Color(.45f, .9f, .55f);
        text.renderer.sortingLayerName = "Foreground";
        text.renderer.sortingOrder = 110;
        popup.AddComponent<FloatingDamageNumber>();
    }
}
