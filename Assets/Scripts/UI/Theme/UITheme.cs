using TMPro;
using UnityEngine;

namespace ProjetoFinal.UI.Theme
{
    public enum ThemeColor
    {
        Background,
        Primary,
        Secondary,
        Accent,
        Highlight,
        TextLight,
        TextDark,
    }

    /// <summary>Fonte e paleta de cores globais, consumidas por <see cref="ThemedGraphic"/>.</summary>
    [CreateAssetMenu(fileName = "UITheme", menuName = "Projeto Final/UI/Theme")]
    public sealed class UITheme : ScriptableObject
    {
        public const string ResourcesPath = "UI/UITheme";

        [Header("Tipografia")]
        [Tooltip("Também é a fonte padrão do TextMeshPro (Project Settings > TextMesh Pro).")]
        [SerializeField] private TMP_FontAsset font;

        [Header("Paleta")]
        [SerializeField] private Color background = new Color32(0x1E, 0x10, 0x33, 0xFF);
        [SerializeField] private Color primary = new Color32(0x2E, 0xC4, 0xB6, 0xFF);
        [SerializeField] private Color secondary = new Color32(0xE5, 0x23, 0x7A, 0xFF);
        [SerializeField] private Color accent = new Color32(0xA4, 0xE0, 0x1C, 0xFF);
        [SerializeField] private Color highlight = new Color32(0xFF, 0xC9, 0x3C, 0xFF);
        [SerializeField] private Color textLight = new Color32(0xFF, 0xF6, 0xE0, 0xFF);
        [SerializeField] private Color textDark = new Color32(0x1B, 0x0B, 0x26, 0xFF);

        private static UITheme defaultTheme;

        public static UITheme Default =>
            defaultTheme != null ? defaultTheme : defaultTheme = Resources.Load<UITheme>(ResourcesPath);

        public TMP_FontAsset Font => font;

        public Color Get(ThemeColor role) => role switch
        {
            ThemeColor.Background => background,
            ThemeColor.Primary => primary,
            ThemeColor.Secondary => secondary,
            ThemeColor.Accent => accent,
            ThemeColor.Highlight => highlight,
            ThemeColor.TextLight => textLight,
            ThemeColor.TextDark => textDark,
            _ => Color.magenta,
        };
    }
}
