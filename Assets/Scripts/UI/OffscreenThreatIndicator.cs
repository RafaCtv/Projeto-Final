using TMPro;
using UnityEngine;

// Mostra de que lado há zumbis fora da câmera; cada aviso some quando não resta zumbi escondido daquele lado.
public sealed class OffscreenThreatIndicator : MonoBehaviour
{
    [SerializeField] private Camera view;
    [SerializeField] private GameObject leftIndicator;
    [SerializeField] private TMP_Text leftCount;
    [SerializeField] private GameObject rightIndicator;
    [SerializeField] private TMP_Text rightCount;
    [Tooltip("Margem em viewport: o zumbi só conta como visível quando entra este tanto na tela.")]
    [SerializeField, Range(0, .2f)] private float margin = .02f;
    [SerializeField, Min(0)] private float pulseSpeed = 5;

    public int LeftCount { get; private set; }
    public int RightCount { get; private set; }

    private void Update()
    {
        var cam = view != null ? view : Camera.main;
        if (cam == null) return;
        int left = 0, right = 0;
        foreach (var zombie in ZombieController.Active)
        {
            if (zombie == null || !zombie.IsAlive) continue;
            float x = cam.WorldToViewportPoint(zombie.transform.position).x;
            if (x < -margin) left++;
            else if (x > 1 + margin) right++;
        }
        LeftCount = left; RightCount = right;
        float pulse = 1 + Mathf.Sin(Time.unscaledTime * pulseSpeed) * .06f;
        Show(leftIndicator, leftCount, left, pulse);
        Show(rightIndicator, rightCount, right, pulse);
    }

    private static void Show(GameObject indicator, TMP_Text count, int amount, float pulse)
    {
        if (indicator == null) return;
        bool visible = amount > 0;
        if (indicator.activeSelf != visible) indicator.SetActive(visible);
        if (!visible) return;
        indicator.transform.localScale = new Vector3(pulse, pulse, 1);
        if (count != null)
        {
            string text = amount.ToString();
            if (count.text != text) count.text = text;
        }
    }
}
