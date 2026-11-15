using System.Collections;
using System.Collections.Generic;
using ProjetoFinal.Input;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Aviso compacto: [E] Torre  🪵10  ⚙5 — ícones em vermelho quando falta recurso.
[DisallowMultipleComponent]
public sealed class InteractionPromptUI : MonoBehaviour
{
    private static readonly Color Affordable = Color.white;
    private static readonly Color Missing = new Color(1, .42f, .37f);
    private static readonly Color Reward = new Color(.55f, .9f, .62f);

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text keyLabel;
    [SerializeField] private TMP_Text label;
    [Tooltip("Modelo (inativo) de um ícone + quantidade; é clonado conforme a necessidade.")]
    [SerializeField] private RectTransform itemTemplate;
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private TMP_Text feedbackLabel;
    [SerializeField, Min(0.1f)] private float feedbackDuration = 2;
    private readonly List<(GameObject root, Image icon, TMP_Text amount)> items = new List<(GameObject, Image, TMP_Text)>();
    private Coroutine feedback;
    private string interactKey;

    private void Awake()
    {
        if (itemTemplate != null) itemTemplate.gameObject.SetActive(false);
        SetTarget(null);
    }

    public void SetTarget(IInteractable target)
    {
        if (panel == null || label == null) return;
        bool show = target != null;
        if (panel.activeSelf != show) panel.SetActive(show);
        if (!show) return;
        if (keyLabel != null) SetText(keyLabel, interactKey ??= InteractKey());
        if (target is IInteractionPromptSource source && !string.IsNullOrEmpty(source.Prompt.Action))
        {
            var prompt = source.Prompt;
            SetText(label, prompt.Action);
            ShowItems(prompt);
        }
        else
        {
            SetText(label, target.InteractionDescription);
            HideItemsFrom(0);
        }
    }

    private void ShowItems(InteractionPrompt prompt)
    {
        for (int i = 0; i < prompt.Items.Count; i++)
        {
            var item = GetItem(i);
            if (item.root == null) return;
            var cost = prompt.Items[i];
            if (!item.root.activeSelf) item.root.SetActive(true);
            if (item.icon.sprite != (cost.Resource != null ? cost.Resource.Icon : null)) item.icon.sprite = cost.Resource != null ? cost.Resource.Icon : null;
            bool reward = prompt.Kind == InteractionItemsKind.Reward;
            SetText(item.amount, (reward ? "+" : "") + cost.Amount);
            item.amount.color = reward ? Reward : HasEnough(cost) ? Affordable : Missing;
        }
        HideItemsFrom(prompt.Items.Count);
    }

    private bool HasEnough(ResourceCost cost) =>
        resourceManager == null || cost.Resource == null || resourceManager.HasAmount(cost.Resource, cost.Amount);

    private (GameObject root, Image icon, TMP_Text amount) GetItem(int index)
    {
        while (items.Count <= index && itemTemplate != null)
        {
            var clone = Instantiate(itemTemplate, itemTemplate.parent);
            clone.name = "Item_" + items.Count;
            items.Add((clone.gameObject, clone.GetComponentInChildren<Image>(true), clone.GetComponentInChildren<TMP_Text>(true)));
        }
        return index < items.Count ? items[index] : default;
    }

    private void HideItemsFrom(int start)
    {
        for (int i = start; i < items.Count; i++) if (items[i].root.activeSelf) items[i].root.SetActive(false);
    }

    private static string InteractKey()
    {
        var action = GameInput.Gameplay.FindAction("Interact", false);
        if (action == null) return "E";
        for (int i = 0; i < action.bindings.Count; i++)
            if (action.bindings[i].groups == "Keyboard") return GameInput.GetBindingDisplay(action, i);
        return "E";
    }

    private static void SetText(TMP_Text text, string value) { if (text.text != value) text.text = value; }

    public void ShowFeedback(string message)
    {
        if (feedbackLabel == null || !isActiveAndEnabled) return;
        if (feedback != null) StopCoroutine(feedback);
        feedbackLabel.text = message;
        feedback = StartCoroutine(ClearFeedback());
    }

    private IEnumerator ClearFeedback()
    {
        yield return new WaitForSeconds(feedbackDuration);
        if (feedbackLabel != null) feedbackLabel.text = string.Empty;
        feedback = null;
    }

    private void OnEnable() => GameInput.BindingsChanged += ForgetKey;
    private void ForgetKey() => interactKey = null;

    private void OnDisable()
    {
        GameInput.BindingsChanged -= ForgetKey;
        if (feedback != null) StopCoroutine(feedback);
        feedback = null;
        if (feedbackLabel != null) feedbackLabel.text = string.Empty;
    }
}
