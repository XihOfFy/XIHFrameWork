using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

public class SpriteAltasUtil
{
    internal static readonly string[] AtlasDirs = new string[] {
        "Assets/Res/UITex",
    };
    const string AtlasName = XiHAsset.XiHAssetBaseMgr.ATLAS;
    [MenuItem("GameUtil/CreateSpriteAtlas")]
    static void CreateSpriteAtlas()
    {
        var res = new List<SpriteAtlas>();
        Debug.Log($"CreateSpriteAtlas Start");
        foreach (var root in AtlasDirs)
        {
            CreateSpriteAtlas(root, res);
        }
        Debug.LogWarning($"CreateSpriteAtlas End ，记得将{nameof(AtlasDirs)}目录全部放在YooAsset的Scanner的SpriteAtlasHalfSchema里面调整和修复错误");
        //SpriteAtlasUtility.PackAtlases(res.ToArray(), BuildTarget.WebGL);
        AssetDatabase.Refresh();
    }
    static void CreateSpriteAtlas(string rootDir, List<SpriteAtlas> res)
    {
        var dirs = Directory.GetDirectories(rootDir);
        if (dirs.Length == 0)
        {
            //最里层级文件夹才进行生成图集
            var fPath = $"{rootDir}/{AtlasName}";
            if (File.Exists(fPath))
            {
                res.Add(ResetSetting(rootDir, AtlasName, fPath));
                return;
                //File.Delete(fPath);
            }
            res.Add(GenSpriteAtlas(rootDir, AtlasName, fPath));
        }
        else
        {
            foreach (var dir in dirs) CreateSpriteAtlas(dir, res);
        }
    }
    static SpriteAtlas ResetSetting(string dir, string name, string fPath)
    {
        var asset = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(fPath);
        asset.name = Path.GetFileNameWithoutExtension(name);
        var dirObj = AssetDatabase.LoadMainAssetAtPath(dir);
        var objs = asset.GetPackables();
        asset.Remove(objs);
        asset.Add(new Object[] { dirObj });
        AssetDatabase.WriteImportSettingsIfDirty(fPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(fPath);
        //OnPreprocessAsset(asset);
        //SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] { asset },BuildTarget.NoTarget);
        return asset;
    }

    static SpriteAtlas GenSpriteAtlas(string dir, string name, string fPath)
    {
        var asset = new SpriteAtlas();
        asset.name = Path.GetFileNameWithoutExtension(name);
        var dirObj = AssetDatabase.LoadMainAssetAtPath(dir);
        asset.Add(new Object[] { dirObj });
        AssetDatabase.CreateAsset(asset, fPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(fPath);
        //OnPreprocessAsset(asset);
        //SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] { asset },BuildTarget.NoTarget);
        return asset;
    }
    /*private static void OnPreprocessAsset(SpriteAtlas importerAsset)
    {

        importerAsset.SetIncludeInBuild(true);
        var src = importerAsset.GetPackingSettings();
        //避免V1版本，获取该精灵在其纹理上使用的矩形。如果该精灵紧密打包在图集中，则引发异常。
        src.enableTightPacking = false;//https://docs.unity.cn/cn/2020.2/ScriptReference/Sprite-textureRect.html
        importerAsset.SetPackingSettings(src);

        var defaultSetting = importerAsset.GetPlatformSettings("DefaultTexturePlatform");
        defaultSetting.format = TextureImporterFormat.RGBA32;
        defaultSetting.textureCompression = TextureImporterCompression.Compressed;


        var ads = importerAsset.GetPlatformSettings(BuildTargetGroup.Android.ToString());
        ads.overridden = true;
        ads.format = TextureImporterFormat.ASTC_4x4; // 越大越失真，推荐4*4-8*8
        ads.compressionQuality = (int)TextureCompressionQuality.Normal;

        var ios = importerAsset.GetPlatformSettings(BuildTargetGroup.iOS.ToString());
        ios.overridden = true;
        ios.format = TextureImporterFormat.ASTC_4x4; // 越大越失真，推荐4*4-8*8
        ios.compressionQuality = (int)TextureCompressionQuality.Normal;

        var wgs = importerAsset.GetPlatformSettings(BuildTargetGroup.WebGL.ToString());
        wgs.overridden = true;
        wgs.format = TextureImporterFormat.ASTC_4x4; // 越大越失真，推荐4*4-8*8
                                                     //wgs.compressionQuality = 10;
                                                     //importer.GetDefaultPlatformTextureSettings().overridden = false;

        importerAsset.SetPlatformSettings(defaultSetting);
        importerAsset.SetPlatformSettings(ads);
        importerAsset.SetPlatformSettings(ios);
        importerAsset.SetPlatformSettings(wgs);

        defaultSetting.maxTextureSize = 4096;//这个设置跟压缩有关系，填大无关系，小了可能有影响
        ads.maxTextureSize = 4096;
        ios.maxTextureSize = 4096;
        wgs.maxTextureSize = 4096;


        EditorUtility.SetDirty(importerAsset);
        AssetDatabase.SaveAssetIfDirty(importerAsset);
        *//*var success = AssetDatabase.WriteImportSettingsIfDirty(importer.assetPath);
        AssetDatabase.Refresh();*//*
        Debug.LogWarning($"图集设置完毕", importerAsset);
    }*/
}
