using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Presentation only: both the HUD and the shelter read the same Health instance.
public sealed class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Image fill;
    [SerializeField] private TMP_Text value;
    private float target;
    public static Color ColorFor(float ratio) => ratio > .6f ? new Color(.35f, .85f, .55f)
        : ratio >= .3f ? new Color(1, .77f, .27f) : new Color(1, .32f, .29f);
    private void OnEnable()
    {
        if (health == null || fill == null) return;
        health.Changed += Refresh; Refresh(health); fill.fillAmount = target; Paint();
    }
    private void Refresh(Health current)
    {
        target = Mathf.Clamp01((float)current.Current / current.Maximum);
        fill.color = ColorFor(target);
        if (value != null) value.text = current.Current + " / " + current.Maximum;
    }
    private void Update()
    {
        if (fill == null) return;
        fill.fillAmount = Mathf.Lerp(fill.fillAmount, target, 1 - Mathf.Exp(-12 * Time.unscaledDeltaTime));
        if (Mathf.Abs(fill.fillAmount - target) < .001f) fill.fillAmount = target;
        Paint();
    }
    private void Paint() => fill.rectTransform.localScale = new Vector3(fill.fillAmount, 1, 1);
    private void OnDisable() { if (health != null) health.Changed -= Refresh; }
}
