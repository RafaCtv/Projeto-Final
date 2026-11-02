using System;
using ProjetoFinal.Audio;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public sealed class ConstructionPoint : MonoBehaviour, IInteractable
{
    [SerializeField] private BuildingData building;
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private GameManager game;
    [SerializeField] private Transform installationPoint;
    [SerializeField] private SpriteRenderer availabilityVisual;
    [SerializeField] private InteractionPromptUI interactionUI;
    [SerializeField] private Color availableColor = new Color(0.55f, 1, 0.7f, 0.32f);
    [SerializeField] private Color insufficientColor = new Color(1, 0.65f, 0.4f, 0.26f);
    [SerializeField] private AudioClip[] buildSounds;

    private bool occupied;
    private bool constructing;
    public bool IsOccupied => occupied;
    public BuildingData Building => building;
    public BuildingInstance InstalledBuilding { get; private set; }

    public string InteractionDescription
    {
        get
        {
            if (building == null || resourceManager == null) return "Construção indisponível";
            string message = "Construir " + building.DisplayName + " - " + building.CostDescription;
            if (!resourceManager.CanAfford(building.Costs, out string reason)) message += "\n" + reason;
            return message;
        }
    }

    private void OnEnable()
    {
        if (resourceManager != null) resourceManager.ResourceChanged += OnResourceChanged;
        UpdateAvailability();
    }

    private void OnResourceChanged(ResourceData resource, int amount) => UpdateAvailability();

    private void UpdateAvailability()
    {
        if (availabilityVisual == null) return;
        availabilityVisual.enabled = !occupied;
        if (!occupied && building != null && resourceManager != null)
            availabilityVisual.color = resourceManager.CanAfford(building.Costs, out _) ? availableColor : insufficientColor;
    }

    public bool CanInteract(GameObject interactor) =>
        isActiveAndEnabled && !occupied && !constructing && building != null && resourceManager != null;

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;
        if (building.Prefab == null || installationPoint == null || interactor == null)
        {
            Feedback("Construção indisponível: configuração incompleta.");
            return;
        }
        if (!resourceManager.CanAfford(building.Costs, out string reason))
        {
            Feedback(reason);
            return;
        }

        constructing = true;
        GameObject staging = null;
        BuildingInstance prepared = null;
        try
        {
            // Stage inactive so preparation cannot produce visible or physical side effects.
            staging = new GameObject("Preparing_" + building.Id);
            staging.SetActive(false);
            staging.transform.SetParent(installationPoint, false);
            prepared = Instantiate(building.Prefab, staging.transform, false);
            prepared.gameObject.SetActive(false);
            prepared.transform.localPosition = Vector3.zero;
            prepared.transform.localRotation = Quaternion.identity;
            prepared.Initialize(building, interactor.GetComponentsInChildren<Collider2D>());
            var turret = prepared.GetComponent<TurretController>();
            if (turret != null) turret.Initialize(game);
            prepared.transform.SetParent(installationPoint, false);

            bool success = resourceManager.TryPurchase(building.Costs, () =>
            {
                prepared.gameObject.SetActive(true);
                if (!prepared.isActiveAndEnabled) return false;
                InstalledBuilding = prepared;
                prepared.Destroyed += OnBuildingDestroyed;
                occupied = true;
                if (availabilityVisual != null) availabilityVisual.enabled = false;
                return true;
            });
            if (!success)
            {
                Feedback("Não foi possível construir. Nenhum recurso foi consumido.");
                return;
            }
            Feedback(building.DisplayName + " construída!");
            if (buildSounds != null) foreach (var clip in buildSounds) SoundEffects.Play(clip);
        }
        catch (Exception exception)
        {
            // A committed instance stays installed even if an unrelated event subscriber throws.
            if (!occupied)
            {
                Feedback("Não foi possível construir. Nenhum recurso foi consumido.");
                Debug.LogWarning("Falha na preparação da construção: " + exception.Message, this);
            }
        }
        finally
        {
            if (!occupied && prepared != null) Destroy(prepared.gameObject);
            if (staging != null) Destroy(staging);
            constructing = false;
        }
    }

    private void Feedback(string message)
    {
        if (interactionUI != null) interactionUI.ShowFeedback(message);
    }

    private void OnDisable()
    {
        if (resourceManager != null) resourceManager.ResourceChanged -= OnResourceChanged;
        if (InstalledBuilding != null) InstalledBuilding.Destroyed -= OnBuildingDestroyed;
    }
    private void OnBuildingDestroyed(BuildingInstance instance)
    {
        instance.Destroyed -= OnBuildingDestroyed;
        if (InstalledBuilding != instance) return;
        InstalledBuilding = null;
        occupied = false;
        UpdateAvailability();
    }
}
