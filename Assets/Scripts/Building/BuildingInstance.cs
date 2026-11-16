using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BuildingInstance : MonoBehaviour
{
    [Header("Obra (opcional)")]
    [Tooltip("Visual mostrado enquanto a construção está em obra.")]
    [SerializeField] private ConstructionSite constructionSite;
    [Tooltip("Visual da construção pronta; fica escondido durante a obra.")]
    [SerializeField] private GameObject finishedVisual;
    [Tooltip("Componentes que só funcionam depois da obra (torreta, produção...).")]
    [SerializeField] private Behaviour[] enableWhenBuilt = Array.Empty<Behaviour>();

    public BuildingData Definition { get; private set; }
    public bool IsUnderConstruction { get; private set; }
    public float ConstructionRemaining { get; private set; }
    public float ConstructionProgress => Definition == null || Definition.ConstructionTime <= 0 ? 1 : 1 - ConstructionRemaining / Definition.ConstructionTime;
    private Collider2D[] passThroughColliders;
    private Health health;
    public event Action<BuildingInstance> Destroyed;
    public event Action<BuildingInstance> Completed;

    public void Initialize(BuildingData definition, Collider2D[] survivorColliders)
    {
        Definition = definition;
        passThroughColliders = survivorColliders;
        float time = definition != null ? definition.ConstructionTime : 0;
        IsUnderConstruction = time > 0;
        ConstructionRemaining = time;
        ApplyState();
    }

    private void OnEnable()
    {
        health = GetComponent<Health>();
        if (health != null) health.Died += OnDied;
        if (passThroughColliders == null) return;
        foreach (var structure in GetComponentsInChildren<Collider2D>())
            foreach (var survivor in passThroughColliders)
                if (structure != null && survivor != null && survivor.isActiveAndEnabled)
                    Physics2D.IgnoreCollision(structure, survivor, true);
    }

    private void Update()
    {
        if (!IsUnderConstruction) return;
        ConstructionRemaining = Mathf.Max(0, ConstructionRemaining - Time.deltaTime);
        if (constructionSite != null) constructionSite.SetProgress(ConstructionProgress, ConstructionRemaining);
        if (ConstructionRemaining > 0) return;
        IsUnderConstruction = false;
        ApplyState();
        if (constructionSite != null) constructionSite.PlayCompleted();
        Completed?.Invoke(this);
    }

    // Termina a obra na hora (testes, cheats, upgrades futuros).
    public void CompleteConstruction()
    {
        if (!IsUnderConstruction) return;
        ConstructionRemaining = 0;
        Update();
    }

    private void ApplyState()
    {
        if (constructionSite != null)
        {
            constructionSite.gameObject.SetActive(IsUnderConstruction);
            if (IsUnderConstruction) constructionSite.SetProgress(ConstructionProgress, ConstructionRemaining);
        }
        if (finishedVisual != null) finishedVisual.SetActive(!IsUnderConstruction);
        foreach (var behaviour in enableWhenBuilt)
            if (behaviour != null) behaviour.enabled = !IsUnderConstruction;
    }

    private void OnDied(Health value)
    {
        foreach (var collider in GetComponentsInChildren<Collider2D>()) collider.enabled = false;
        Destroyed?.Invoke(this);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
    private void OnDisable() { if (health != null) health.Died -= OnDied; }
}
