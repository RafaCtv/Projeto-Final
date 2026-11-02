using UnityEngine;
using UnityEngine.UI;

namespace ProjetoFinal.UI.Theme
{
    /// <summary>Aplica uma cor do <see cref="UITheme"/> ao Graphic, inclusive em modo de edição.</summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    public sealed class ThemedGraphic : MonoBehaviour
    {
        [SerializeField] private ThemeColor role = ThemeColor.Primary;
        [Tooltip("Vazio = tema padrão em Resources/UI/UITheme.")]
        [SerializeField] private UITheme theme;
        [Range(0f, 1f)]
        [SerializeField] private float alpha = 1f;

        public ThemeColor Role
        {
            get => role;
            set { role = value; Apply(); }
        }

        public void Apply()
        {
            var t = theme != null ? theme : UITheme.Default;
            if (t == null)
                return;

            var color = t.Get(role);
            color.a *= alpha;
            GetComponent<Graphic>().color = color;
        }

        private void OnEnable() => Apply();

#if UNITY_EDITOR
        private void OnValidate() => Apply();
#endif
    }
}
