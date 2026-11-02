using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text label;
    [SerializeField] private TMP_Text feedbackLabel;
    [SerializeField, Min(0.1f)] private float feedbackDuration = 2;
    private Coroutine feedback;

    private void Awake() => SetTarget(null);

    public void SetTarget(IInteractable target)
    {
        if (panel == null || label == null) return;
        bool show = target != null;
        if (panel.activeSelf != show) panel.SetActive(show);
        if (show)
        {
            string message = "Pressione E para interagir\n" + target.InteractionDescription;
            if (label.text != message) label.text = message;
        }
    }

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

    private void OnDisable()
    {
        if (feedback != null) StopCoroutine(feedback);
        feedback = null;
        if (feedbackLabel != null) feedbackLabel.text = string.Empty;
    }
}
