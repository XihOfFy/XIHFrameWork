using UnityEditor;
using UnityEngine;

public class TextureAssetImport : AssetPostprocessor
{
    public void OnPreprocessTexture()
    {
        if (!this.assetPath.StartsWith("Assets/Res/")) return;

        TextureImporter importer = this.assetImporter as TextureImporter;
        if (importer == null) return;

        importer.GetSourceTextureWidthAndHeight(out var width, out var height);
        var nativeSize = Mathf.NextPowerOfTwo(Mathf.Max(width, height));
        var maxTextureSize = 4096;
        var textureSize = Mathf.Min(nativeSize, maxTextureSize);
        //if (importer.maxTextureSize == textureSize) return;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = textureSize;
        importer.textureCompression = TextureImporterCompression.CompressedLQ;

        /*{
            Debug.Log($"Spine 相关的图片处理：{this.assetPath}", importer);
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            //importer.textureCompression = TextureImporterCompression.Compressed;
        }*/

        var ads = importer.GetPlatformTextureSettings(BuildTargetGroup.Android.ToString());
        ads.overridden = true;
        ads.format = TextureImporterFormat.ASTC_6x6; // 越大越失真，推荐4*4-8*8

        var ios = importer.GetPlatformTextureSettings(BuildTargetGroup.iOS.ToString());
        ios.overridden = true;
        ios.format = TextureImporterFormat.ASTC_6x6; // 越大越失真，推荐4*4-8*8

        var wgs = importer.GetPlatformTextureSettings(BuildTargetGroup.WebGL.ToString());
        wgs.overridden = true;
        wgs.format = TextureImporterFormat.ASTC_8x8; // 越大越失真，推荐4*4-8*8
        //wgs.compressionQuality = 10;
        //importer.GetDefaultPlatformTextureSettings().overridden = false;

        if (!this.assetPath.StartsWith("Assets/Res/FairyRes/") && !this.assetPath.StartsWith("Assets/Res/Chapter/"))
        {
            textureSize >>= 1;
        }
        ads.maxTextureSize = textureSize;
        ios.maxTextureSize = textureSize;
        wgs.maxTextureSize = textureSize;
        importer.SetPlatformTextureSettings(ads);
        importer.SetPlatformTextureSettings(ios);
        importer.SetPlatformTextureSettings(wgs);

        importer.SaveAndReimport();
        /*var success = AssetDatabase.WriteImportSettingsIfDirty(importer.assetPath);
        AssetDatabase.Refresh();*/
        Debug.LogWarning($"{this.assetPath}图片原始大小{nativeSize}，将使用{textureSize}作为最大尺寸", importer);
    }
}
