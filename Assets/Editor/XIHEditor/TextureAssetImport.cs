using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TextureAssetImport : AssetPostprocessor
{
    public void OnPreprocessTexture()
    {
        if (!this.assetPath.StartsWith("Assets/Res/")) return;
        TextureImporter importer = this.assetImporter as TextureImporter;
        if (importer == null) return;

        importer.GetSourceTextureWidthAndHeight(out var width,out var height);
        var nativeSize = Mathf.NextPowerOfTwo(Mathf.Max(width, height));
        var maxTextureSize = 4096;
        var textureSize = Mathf.Min(nativeSize, maxTextureSize);
        if (importer.maxTextureSize == textureSize) return;
        importer.maxTextureSize = textureSize;

        /*if (assetPath.StartsWith("Assets/Res/Role/")) {
            TextureImporterSettings tis = new TextureImporterSettings();
            importer.ReadTextureSettings(tis);
            tis.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(tis);
            Debug.Log($"{assetPath}设置图片模式为{tis.spriteMeshType}");
        }*/

        Debug.LogWarning($"{this.assetPath}图片原始大小{nativeSize}，将使用{textureSize}作为最大尺寸");
        importer.SaveAndReimport();
    }
}
