// 编写自定义打包规则，然后将脚本放在Editor目录下。
// 然后在AssetBundleCollector界面对视频文件使用扩展的打包规则。
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using XiHAsset;
using YooAsset.Editor;

[DisplayName("收集非预制体")]
public class CollectUnPrefab : IFilterRule
{
    public string FindAssetType => EAssetSearchType.All.ToString();
    public bool IsCollectAsset(FilterRuleData data)
    {
        return Path.GetExtension(data.AssetPath) != ".prefab";
    }
}
[DisplayName("收集精灵类型的纹理")]
public class CollectSpriteAtlas : IFilterRule
{
    public string FindAssetType => EAssetSearchType.All.ToString();
    public bool IsCollectAsset(FilterRuleData data)
    {
        /*        var mainAssetType = AssetDatabase.GetMainAssetTypeAtPath(data.AssetPath);
                return mainAssetType == typeof(SpriteAtlas);*/
        return Path.GetExtension(data.AssetPath) == Path.GetExtension(XiHAssetBaseMgr.ATLAS);
    }
}
[DisplayName("收集非精灵类型的纹理")]
public class CollectUnSpriteAtlas : IFilterRule
{
    public string FindAssetType => EAssetSearchType.All.ToString();
    public bool IsCollectAsset(FilterRuleData data)
    {
        return Path.GetExtension(data.AssetPath) != Path.GetExtension(XiHAssetBaseMgr.ATLAS);
    }
}
[DisplayName("收集首关")]
public class PackFirstAB : IPackRule
{
    public string FindAssetType => EAssetSearchType.All.ToString();
    public PackRuleResult GetPackRuleResult(PackRuleData data)
    {
        var path = data.AssetPath;
        foreach (var group in PackUtil.PackGroup)
        {
            var abName = group.Key;
            var paths = group.Value;
            foreach (var pre in paths)
            {
                if (path.StartsWith(pre))
                {
                    var res = new PackRuleResult(abName, DefaultPackRule.AssetBundleFileExtension);
                    return res;
                }
            }
        }
        return new PackRuleResult(PackUtil.FIRST_GAME, DefaultPackRule.AssetBundleFileExtension);
    }
}

[DisplayName("首包路径或SubLayer分包")]
public class PackFirstOrSubLayer : IPackRule
{
    const string ResRoot = "Assets/Res/";

    public PackRuleResult GetPackRuleResult(PackRuleData data)
    {
        var path = data.AssetPath.Replace('\\', '/');

        foreach (var group in PackUtil.PackGroup)
        {
            var abName = group.Key;
            foreach (var item in group.Value)
            {
                if (IsPackGroupMatch(path, item))
                    return new PackRuleResult(abName, DefaultPackRule.AssetBundleFileExtension);
            }
        }

        var bundleName = GetSubLayerBundleName(path);
        return new PackRuleResult(bundleName, DefaultPackRule.AssetBundleFileExtension);
    }

    static bool IsPackGroupMatch(string assetPath, string packPath)
    {
        if (packPath.EndsWith("/"))
            return assetPath.StartsWith(packPath, StringComparison.OrdinalIgnoreCase);
        return assetPath.Equals(packPath, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Assets/Res/RootLayer/SubLayer/... → AB 名为 Assets/Res/RootLayer/SubLayer
    /// 仅 RootLayer 一层时 → AB 名为 Assets/Res/RootLayer
    /// </summary>
    static string GetSubLayerBundleName(string assetPath)
    {
        if (!assetPath.StartsWith(ResRoot, StringComparison.OrdinalIgnoreCase))
            throw new Exception($"资源路径不在 {ResRoot} 下，无法按 SubLayer 分包: {assetPath}");

        var relative = assetPath.Substring(ResRoot.Length);
        var splits = relative.Split('/');
        if (splits.Length < 1 || string.IsNullOrEmpty(splits[0]))
            throw new Exception($"路径层级不足，需要 Assets/Res/RootLayer 结构: {assetPath}");

        if (splits.Length < 2 || string.IsNullOrEmpty(splits[1]))
            return $"{ResRoot}{splits[0]}";

        return $"{ResRoot}{splits[0]}/{splits[1]}";
    }
}

public static class PackUtil
{
    public const string FIRST_GAME = "FirstGame";
    public const string FIRST_GAME2 = "FirstGame2";
    public static Dictionary<string, List<string>> PackGroup = new Dictionary<string, List<string>>()
    {
        [FIRST_GAME] = new List<string>() {
            "Assets/Res/Tmpl/",
            "Assets/Res/Aot2Hot/Font/",
            "Assets/Res/FairyRes/Common/"
        },
        [FIRST_GAME2] = new List<string>()
        {
            "Assets/Res/PoolObj/"
        },
    };
}
