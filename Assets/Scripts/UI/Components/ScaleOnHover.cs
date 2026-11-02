using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjetoFinal.UI.Components
{
    /// <summary>Escala o objeto em hover/seleção e ao pressionar.</summary>
    [DisallowMultipleComponent]
    public sealed class ScaleOnHover : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler,
        ISelectHandler, IDeselectHandler
    {
        [SerializeField] private float hoverScale = 1.08f;
        [SerializeField] private float pressedScale = 0.94f;
        [SerializeField] private float speed = 18f;

        private Vector3 baseScale = Vector3.one;
        private bool hovered;
        private bool pressed;

        private float TargetMultiplier => pressed ? pressedScale : hovered ? hoverScale : 1f;

        private void Awake() => baseScale = transform.localScale;

        private void OnDisable()
        {
            hovered = pressed = false;
            transform.localScale = baseScale;
        }

        private void Update()
        {
            var target = baseScale * TargetMultiplier;
            transform.localScale = Vector3.Lerp(transform.localScale, target, 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime));
        }

        public void OnPointerEnter(PointerEventData eventData) => hovered = true;
        public void OnPointerExit(PointerEventData eventData) => hovered = pressed = false;
        public void OnPointerDown(PointerEventData eventData) => pressed = true;
        public void OnPointerUp(PointerEventData eventData) => pressed = false;
        public void OnSelect(BaseEventData eventData) => hovered = true;
        public void OnDeselect(BaseEventData eventData) => hovered = false;
    }
}
