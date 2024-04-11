using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        //if (importer.maxTextureSize == textureSize) return;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = textureSize;
        importer.textureCompression = TextureImporterCompression.CompressedLQ;

        if (this.assetPath.StartsWith("Assets/Res/Chapter/"))
        {
            Debug.Log($"Spine 相关的图片处理：{this.assetPath}", importer);
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Compressed;
        }

        var wgs = importer.GetPlatformTextureSettings(BuildTargetGroup.WebGL.ToString());
        wgs.overridden=true;
        wgs.maxTextureSize = (textureSize >> 1);
        //wgs.compressionQuality = 10;
        importer.SetPlatformTextureSettings(wgs);

        importer.SaveAndReimport();
        /*var success = AssetDatabase.WriteImportSettingsIfDirty(importer.assetPath);
        AssetDatabase.Refresh();*/
       Debug.LogWarning($"{this.assetPath}图片原始大小{nativeSize}，将使用{textureSize}作为最大尺寸 WEBGL为其一半", importer);
    }
}
