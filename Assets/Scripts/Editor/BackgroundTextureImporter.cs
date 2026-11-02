using UnityEditor;

namespace ProjetoFinal.Editor
{
    /// <summary>Importa imagens de Resources/Backgrounds como Sprite sem compressão e sem mipmaps.</summary>
    public sealed class BackgroundTextureImporter : AssetPostprocessor
    {
        private const string Folder = "Assets/Resources/Backgrounds/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Folder))
                return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
        }
    }
}
