using UnityEditor;
using UnityEngine;

namespace CatCatGo.Editor
{
    public class IconImportSettings : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.Contains("Resources/Icons/")) return;

            var importer = (TextureImporter)assetImporter;
            if (importer.textureType == TextureImporterType.Sprite
                && importer.spriteImportMode == SpriteImportMode.Single)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
        }

        [MenuItem("Tools/Reimport All Icons")]
        private static void ReimportAllIcons()
        {
            int fixedCount = 0;
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/_Project/Resources/Icons" });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                bool needsFix = importer.textureType != TextureImporterType.Sprite
                    || importer.spriteImportMode != SpriteImportMode.Single;

                if (needsFix)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.mipmapEnabled = false;
                    importer.SaveAndReimport();
                    fixedCount++;
                }
            }
            Debug.Log($"[IconImportSettings] Scanned {guids.Length} icons, fixed {fixedCount}");
        }
    }
}
