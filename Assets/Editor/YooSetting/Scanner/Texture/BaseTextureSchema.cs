using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using YooAsset.Editor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public abstract class BaseTextureSchema : ScannerSchema
{
    public TextureImporterFormat androidFormat = TextureImporterFormat.ASTC_4x4;
    public TextureImporterFormat iosFormat = TextureImporterFormat.ASTC_4x4;
    public TextureImporterFormat webglFormat = TextureImporterFormat.ASTC_6x6;
    public bool isReadable = false;

    public abstract int GetMaxTextSize(TextureImporter importer);
    /// <summary>
    /// 获取用户指南信息
    /// </summary>
    public override string GetUserGuide()
    {
        return "规则介绍：检测图片的格式，尺寸。";
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
        report.AddHeader("图片宽度", 100).SetSortable().SetHeaderType(EHeaderType.LongValue);
        report.AddHeader("图片高度", 100).SetSortable().SetHeaderType(EHeaderType.LongValue);
        report.AddHeader("内存大小", 120).SetSortable().SetUnits("bytes").SetHeaderType(EHeaderType.LongValue);
        report.AddHeader("苹果格式", 100);
        report.AddHeader("安卓格式", 100);
        report.AddHeader("WebGL格式", 100);
        report.AddHeader("可读写", 50);
        report.AddHeader("错误信息", 500).SetStretchable();

        // 获取扫描资源集合
        var searchDirectorys = scanner.Collectors.Select(c => { return c.CollectPath; });
        string[] findAssets = EditorTools.FindAssets(EAssetSearchType.Texture, searchDirectorys.ToArray());

        // 开始扫描资源集合
        var results = SchemaTools.ScanAssets(findAssets, ScanAssetInternal);
        report.ReportElements.AddRange(results);
        return report;
    }
    private ReportElement ScanAssetInternal(string assetPath)
    {
        var importer = TextureTools.GetAssetImporter(assetPath);
        if (importer == null)
            return null;

        // 加载纹理对象
        var texture = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
        var assetGUID = AssetDatabase.AssetPathToGUID(assetPath);
        var iosFormat = TextureTools.GetPlatformIOSFormat(importer);
        var androidFormat = TextureTools.GetPlatformAndroidFormat(importer);
        var webglFormat = TextureTools.GetPlatformWebGLFormat(importer);
        var memorySize = TextureTools.GetStorageMemorySize(texture);

        // 获取错误信息
        string errorInfo = string.Empty;
        {
            // 苹果格式
            if (iosFormat != this.iosFormat)
            {
                errorInfo += " | ";
                errorInfo += "苹果格式不对";
            }

            // 安卓格式
            if (androidFormat != this.androidFormat)
            {
                errorInfo += " | ";
                errorInfo += "安卓格式不对";
            }

            // WebGL格式
            if (webglFormat != this.webglFormat)
            {
                errorInfo += " | ";
                errorInfo += "WebGL格式不对";
            }

            // 多级纹理
            if (importer.isReadable != this.isReadable)
            {
                errorInfo += " | ";
                errorInfo += "可读写不匹配";
            }

            // 纹理
            var textureSize = GetMaxTextSize(importer);
            // 苹果格式，只要一种不匹配即可说明错误
            var iosPlatformSetting = TextureTools.GetPlatformIOSSettings(importer);
            if (iosPlatformSetting.maxTextureSize != textureSize)
            {
                errorInfo += " | ";
                errorInfo += "纹理不匹配";
            }
        }

        // 添加扫描信息
        ReportElement result = new ReportElement(assetGUID);
        result.AddScanInfo("资源路径", assetPath);
        result.AddScanInfo("图片宽度", texture.width);
        result.AddScanInfo("图片高度", texture.height);
        result.AddScanInfo("内存大小", memorySize);
        result.AddScanInfo("苹果格式", iosFormat.ToString());
        result.AddScanInfo("安卓格式", androidFormat.ToString());
        result.AddScanInfo("WebGL格式", webglFormat.ToString());
        result.AddScanInfo("可读写", importer.isReadable.ToString());
        result.AddScanInfo("错误信息", errorInfo);

        // 判断是否通过
        result.Passes = string.IsNullOrEmpty(errorInfo);
        return result;
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
        var importer = TextureTools.GetAssetImporter(assetPath);
        if (importer == null)
            return;

        var textureSize = GetMaxTextSize(importer);

        // 苹果格式
        var iosPlatformSetting = TextureTools.GetPlatformIOSSettings(importer);
        iosPlatformSetting.format = this.iosFormat;
        iosPlatformSetting.overridden = true;
        iosPlatformSetting.maxTextureSize = textureSize;

        // 安卓格式
        var androidPlatformSetting = TextureTools.GetPlatformAndroidSettings(importer);
        androidPlatformSetting.format = this.androidFormat;
        androidPlatformSetting.overridden = true;
        androidPlatformSetting.maxTextureSize = textureSize;

        // WebGL格式
        var webGLPlatformSetting = TextureTools.GetPlatformWebGLSettings(importer);
        webGLPlatformSetting.format = this.webglFormat;
        webGLPlatformSetting.overridden = true;
        webGLPlatformSetting.maxTextureSize = textureSize;

        // 可读写
        importer.isReadable = this.isReadable;

        // 保存配置
        importer.SetPlatformTextureSettings(iosPlatformSetting);
        importer.SetPlatformTextureSettings(androidPlatformSetting);
        importer.SetPlatformTextureSettings(webGLPlatformSetting);
        importer.SaveAndReimport();
        Debug.Log($"修复了 : {assetPath}", importer);
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
}
