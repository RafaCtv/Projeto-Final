using UnityEngine;
using UnityEngine.UI;

// Barra de estamina: amarela normalmente, vermelha quando exausto; esmaece quando cheia.
public sealed class StaminaBarUI : MonoBehaviour
{
    private static readonly Color Ready = new Color(1, .82f, .3f);
    private static readonly Color Exhausted = new Color(1, .36f, .3f);

    [SerializeField] private PlayerController player;
    [SerializeField] private Image fill;
    [SerializeField] private CanvasGroup group;
    [SerializeField, Range(0, 1)] private float idleAlpha = .45f;

    private void Update()
    {
        if (player == null || player.Stamina == null || fill == null) return;
        var stamina = player.Stamina;
        fill.rectTransform.localScale = new Vector3(stamina.Normalized, 1, 1);
        fill.color = stamina.IsExhausted ? Exhausted : Ready;
        if (group != null)
        {
            float target = stamina.Normalized >= .999f && !player.IsSprinting ? idleAlpha : 1;
            group.alpha = Mathf.MoveTowards(group.alpha, target, Time.unscaledDeltaTime * 3);
        }
    }
}
