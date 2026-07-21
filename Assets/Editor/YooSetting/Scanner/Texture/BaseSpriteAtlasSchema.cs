using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using YooAsset.Editor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine.U2D;
using UnityEditor.U2D;
using System.IO;

public abstract class BaseSpriteAtlasSchema : ScannerSchema
{
    public TextureImporterFormat androidFormat = TextureImporterFormat.ASTC_4x4;
    public TextureImporterFormat iosFormat = TextureImporterFormat.ASTC_4x4;
    public TextureImporterFormat webglFormat = TextureImporterFormat.ASTC_6x6;
    public bool isReadable = false;
    public int androidCompressionQuality = (int)TextureCompressionQuality.Normal;
    public int iosCompressionQuality = (int)TextureCompressionQuality.Normal;
    public int webglCompressionQuality = (int)TextureCompressionQuality.Normal;
    int maxSize = 4096;
    public abstract int GetMaxTextSize(TextureImporter importer);
    /// <summary>
    /// 获取用户指南信息
    /// </summary>
    public override string GetUserGuide()
    {
        return "规则介绍：检测图集的格式，尺寸。";
    }

    /// <summary>
    /// 运行生成扫描报告
    /// </summary>
    public override ScanReport RunScanner(AssetArtScanner scanner)
    {
        // 创建扫描报告
        string name = "扫描所有纹理资产";
        string desc = GetUserGuide();
        var report = new ScanReport(name, desc);
        report.AddHeader("资源路径", 300, 200, 1000).SetStretchable().SetSearchable().SetSortable().SetCounter().SetHeaderType(EHeaderType.AssetPath);
        report.AddHeader("苹果格式", 100);
        report.AddHeader("安卓格式", 100);
        report.AddHeader("WebGL格式", 100);
        report.AddHeader("可读写", 50);
        report.AddHeader("错误信息", 500).SetStretchable();

        // 获取扫描资源集合
        var searchDirectorys = scanner.Collectors.Select(c => { return c.CollectPath; });
        string[] findAssets = SchemaTools.FindAssets("SpriteAtlas", searchDirectorys.ToArray());
        // 开始扫描资源集合
        var results = SchemaTools.ScanAssets(findAssets, ScanAssetInternal);
        report.ReportElements.AddRange(results);
        return report;
    }
    private ReportElement ScanAssetInternal(string assetPath)
    {
        var asset = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(assetPath);
        if (asset != null)
            return ScanAssetInternal(assetPath, asset);
        return null;
    }
    /// <summary>
    /// 修复扫描结果
    /// </summary>
    public override void FixResult(List<ReportElement> fixList)
    {
        SchemaTools.FixAssets(fixList, FixAssetInternal);
    }
    private void FixAssetInternal(ReportElement result)
    {
        var scanInfo = result.GetScanInfo("资源路径");
        var assetPath = scanInfo.ScanInfo;
        var asset = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(assetPath);
        if (asset != null)
        {
            FixAssetInternal(assetPath,asset);
        }
    }

    /// <summary>
    /// 创建检视面板对
    /// </summary>
    public override SchemaInspector CreateInspector()
    {
        var container = new VisualElement();

        var androidFormatField = new EnumField(androidFormat);
        androidFormatField.label = "Android格式";
        androidFormatField.SetValueWithoutNotify(androidFormat);
        androidFormatField.RegisterValueChangedCallback((evt) =>
        {
            androidFormat = (TextureImporterFormat)evt.newValue;
        });
        container.Add(androidFormatField);

        var iosFormatField = new EnumField(iosFormat);
        iosFormatField.label = "iOS格式";
        iosFormatField.SetValueWithoutNotify(iosFormat);
        iosFormatField.RegisterValueChangedCallback((evt) =>
        {
            iosFormat = (TextureImporterFormat)evt.newValue;
        });
        container.Add(iosFormatField);

        var webglFormatField = new EnumField(webglFormat);
        webglFormatField.label = "WebGL格式";
        webglFormatField.SetValueWithoutNotify(webglFormat);
        webglFormatField.RegisterValueChangedCallback((evt) =>
        {
            webglFormat = (TextureImporterFormat)evt.newValue;
        });
        container.Add(webglFormatField);
/*            public int  = (int)TextureCompressionQuality.Normal;
    public int  = (int)TextureCompressionQuality.Normal;
    public int  = (int)TextureCompressionQuality.Normal;*/
        var androidCompressionQualityField = new IntegerField(androidCompressionQuality);
        androidCompressionQualityField.label = "androidCompressionQuality";
        androidCompressionQualityField.SetValueWithoutNotify(androidCompressionQuality);
        androidCompressionQualityField.RegisterValueChangedCallback((evt) =>
        {
            androidCompressionQuality = evt.newValue;
        });
        container.Add(androidCompressionQualityField);

        var iosCompressionQualityField = new IntegerField(iosCompressionQuality);
        iosCompressionQualityField.label = "iosCompressionQuality";
        iosCompressionQualityField.SetValueWithoutNotify(iosCompressionQuality);
        iosCompressionQualityField.RegisterValueChangedCallback((evt) =>
        {
            iosCompressionQuality = evt.newValue;
        });
        container.Add(iosCompressionQualityField);

        var webglCompressionQualityField = new IntegerField(webglCompressionQuality);
        webglCompressionQualityField.label = "webglCompressionQuality";
        webglCompressionQualityField.SetValueWithoutNotify(webglCompressionQuality);
        webglCompressionQualityField.RegisterValueChangedCallback((evt) =>
        {
            webglCompressionQuality = evt.newValue;
        });
        container.Add(webglCompressionQualityField);

        var isReadableField = new Toggle();
        isReadableField.label = "是否可读写";
        isReadableField.SetValueWithoutNotify(isReadable);
        isReadableField.RegisterValueChangedCallback((evt) =>
        {
            isReadable = evt.newValue;
        });
        container.Add(isReadableField);

        SchemaInspector inspector = new SchemaInspector(container);
        return inspector;
    }

    private ReportElement ScanAssetInternal(string assetPath, SpriteAtlas importerAsset)
    {
        var assetGUID = AssetDatabase.AssetPathToGUID(assetPath);
        var ads = importerAsset.GetPlatformSettings(BuildTargetGroup.Android.ToString());
        var ios = importerAsset.GetPlatformSettings(BuildTargetGroup.iOS.ToString());
        var wgs = importerAsset.GetPlatformSettings(BuildTargetGroup.WebGL.ToString());
        //var packingSettings = importerAsset.GetPackingSettings();
        var textureSettings = importerAsset.GetTextureSettings();

        // 获取错误信息
        string errorInfo = string.Empty;
        {
            // 苹果格式
            if (ios.format != this.iosFormat || !ios.overridden || ios.compressionQuality != iosCompressionQuality)// || ios.maxTextureSize != maxSize
            {
                errorInfo += " | ";
                errorInfo += "苹果格式不对";
            }

            // 安卓格式
            if (ads.format != this.androidFormat || !ads.overridden || ads.compressionQuality != androidCompressionQuality)
            {
                errorInfo += " | ";
                errorInfo += "安卓格式不对";
            }

            // WebGL格式
            if (wgs.format != this.webglFormat || !wgs.overridden || wgs.compressionQuality != webglCompressionQuality)
            {
                errorInfo += " | ";
                errorInfo += "WebGL格式不对";
            }

            // 多级纹理
            if (textureSettings.readable != this.isReadable)
            {
                errorInfo += " | ";
                errorInfo += "可读写不匹配";
            }

            // 多级纹理
            if (Path.GetFileName(assetPath) != XiHAsset.XiHAssetBaseMgr.ATLAS)
            {
                errorInfo += " | ";
                errorInfo += "命名错误";
            }
        }

        // 添加扫描信息
        ReportElement result = new ReportElement(assetGUID);
        result.AddScanInfo("资源路径", assetPath);
        result.AddScanInfo("苹果格式", iosFormat.ToString());
        result.AddScanInfo("安卓格式", androidFormat.ToString());
        result.AddScanInfo("WebGL格式", webglFormat.ToString());
        result.AddScanInfo("可读写", textureSettings.readable.ToString());
        result.AddScanInfo("错误信息", errorInfo);

        // 判断是否通过
        result.Passes = string.IsNullOrEmpty(errorInfo);
        return result;
    }

    private void FixAssetInternal(string assetPath, SpriteAtlas importerAsset)
    {
        importerAsset.SetIncludeInBuild(true);
        var src = importerAsset.GetPackingSettings();
        //避免V1版本，获取该精灵在其纹理上使用的矩形。如果该精灵紧密打包在图集中，则引发异常。
        src.enableTightPacking = false;//https://docs.unity.cn/cn/2020.2/ScriptReference/Sprite-textureRect.html
        importerAsset.SetPackingSettings(src);

        var ads = importerAsset.GetPlatformSettings(BuildTargetGroup.Android.ToString());
        ads.overridden = true;
        ads.format = androidFormat; // 越大越失真，推荐4*4-8*8
        ads.compressionQuality = androidCompressionQuality;

        var ios = importerAsset.GetPlatformSettings(BuildTargetGroup.iOS.ToString());
        ios.overridden = true;
        ios.format = iosFormat; // 越大越失真，推荐4*4-8*8
        ios.compressionQuality = iosCompressionQuality;

        var wgs = importerAsset.GetPlatformSettings(BuildTargetGroup.WebGL.ToString());
        wgs.overridden = true;
        wgs.format = webglFormat; // 越大越失真，推荐4*4-8*8
        wgs.compressionQuality = webglCompressionQuality;

        importerAsset.SetPlatformSettings(ads);
        importerAsset.SetPlatformSettings(ios);
        importerAsset.SetPlatformSettings(wgs);

        //这个设置跟压缩有关系，填大无关系，小了可能有影响
        ads.maxTextureSize = maxSize;
        ios.maxTextureSize = maxSize;
        wgs.maxTextureSize = maxSize;

        var textureSettings = importerAsset.GetTextureSettings();
        textureSettings.readable = this.isReadable;
        importerAsset.SetTextureSettings(textureSettings);

        EditorUtility.SetDirty(importerAsset);
        AssetDatabase.SaveAssetIfDirty(importerAsset);
        var fName = Path.GetFileName(assetPath);
        var tName = XiHAsset.XiHAssetBaseMgr.ATLAS;
        if (!fName.Equals(tName))
        {
            var newPath = assetPath.Substring(0, assetPath.Length - fName.Length) + tName;
            AssetDatabase.MoveAsset(assetPath, newPath);
        }
        /*var success = AssetDatabase.WriteImportSettingsIfDirty(importer.assetPath);
        AssetDatabase.Refresh();*/
        Debug.LogWarning($"图集设置完毕 {assetPath}", importerAsset);
    }
}
