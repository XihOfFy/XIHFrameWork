using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using YooAsset.Editor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public abstract class BaseAudioSchema : ScannerSchema
{
    public bool forceToMono = true;
    public bool loadInBackground = false;
    public AudioClipLoadType loadType = AudioClipLoadType.CompressedInMemory;
    public AudioCompressionFormat compressionFormat = AudioCompressionFormat.Vorbis;
    public float quality = 0.1f;
    public bool preloadAudioData = false;

    /// <summary>
    /// 获取用户指南信息
    /// </summary>
    public override string GetUserGuide()
    {
        return "规则介绍：检测音频设置。";
    }

    /// <summary>
    /// 运行生成扫描报告
    /// </summary>
    public override ScanReport RunScanner(AssetArtScanner scanner)
    {
        // 创建扫描报告
        string name = "扫描所有音频资产";
        string desc = GetUserGuide();
        var report = new ScanReport(name, desc);
        report.AddHeader("资源路径", 300, 200, 1000).SetStretchable().SetSearchable().SetSortable().SetCounter().SetHeaderType(EHeaderType.AssetPath);
        report.AddHeader("forceToMono", 100);
        report.AddHeader("loadInBackground", 100);
        report.AddHeader("loadType", 150,100,200);
        report.AddHeader("compressionFormat", 100);
        report.AddHeader("quality", 100).SetHeaderType(EHeaderType.DoubleValue);
        report.AddHeader("preloadAudioData", 100);
        report.AddHeader("错误信息", 500).SetStretchable();

        // 获取扫描资源集合
        var searchDirectorys = scanner.Collectors.Select(c => { return c.CollectPath; });
        string[] findAssets = EditorTools.FindAssets(EAssetSearchType.AudioClip, searchDirectorys.ToArray());

        // 开始扫描资源集合
        var results = SchemaTools.ScanAssets(findAssets, ScanAssetInternal);
        report.ReportElements.AddRange(results);
        return report;
    }
    private ReportElement ScanAssetInternal(string assetPath)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as AudioImporter;
        if (importer == null)
            return null;

        // 加载对象
        var assetGUID = AssetDatabase.AssetPathToGUID(assetPath);
        var defaultSampleSettings = importer.defaultSampleSettings;
        // 获取错误信息
        string errorInfo = string.Empty;
        {
            if (importer.forceToMono != this.forceToMono)
            {
                errorInfo += " | ";
                errorInfo += "forceToMono";
            }

            if (importer.loadInBackground != this.loadInBackground)
            {
                errorInfo += " | ";
                errorInfo += "loadInBackground";
            }

            if (defaultSampleSettings.loadType != this.loadType)
            {
                errorInfo += " | ";
                errorInfo += "loadType";
            }

            if (defaultSampleSettings.compressionFormat != this.compressionFormat)
            {
                errorInfo += " | ";
                errorInfo += "compressionFormat";
            }

            if (defaultSampleSettings.quality != this.quality)
            {
                errorInfo += " | ";
                errorInfo += "quality";
            }
#if UNITY_2022_3_OR_NEWER
            if (defaultSampleSettings.preloadAudioData != this.preloadAudioData)
#else
            if (importer.preloadAudioData != this.preloadAudioData)
#endif
            {
                errorInfo += " | ";
                errorInfo += "preloadAudioData";
            }
        }

        // 添加扫描信息
        ReportElement result = new ReportElement(assetGUID);
        result.AddScanInfo("资源路径", assetPath);
        result.AddScanInfo("forceToMono", importer.forceToMono.ToString());
        result.AddScanInfo("loadInBackground", importer.loadInBackground.ToString());
        result.AddScanInfo("loadType", defaultSampleSettings.loadType.ToString());
        result.AddScanInfo("compressionFormat", defaultSampleSettings.compressionFormat.ToString());
        result.AddScanInfo("quality", defaultSampleSettings.quality.ToString());
#if UNITY_2022_3_OR_NEWER
            result.AddScanInfo("preloadAudioData", defaultSampleSettings.preloadAudioData.ToString());
#else
        result.AddScanInfo("preloadAudioData", importer.preloadAudioData.ToString());
#endif
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
        var importer = AssetImporter.GetAtPath(assetPath) as AudioImporter;
        if (importer == null)
            return;

        importer.forceToMono = forceToMono;
        importer.loadInBackground = loadInBackground;
        var defaultSampleSettings = importer.defaultSampleSettings;
        defaultSampleSettings.loadType = loadType;
        defaultSampleSettings.compressionFormat = compressionFormat;
        defaultSampleSettings.quality = quality;
#if UNITY_2022_3_OR_NEWER
        defaultSampleSettings.preloadAudioData = preloadAudioData;
#else
        importer.preloadAudioData = preloadAudioData;
#endif
        importer.defaultSampleSettings = defaultSampleSettings;
        importer.SaveAndReimport();
        Debug.Log($"修复了 : {assetPath}");
    }

    /// <summary>
    /// 创建检视面板对
    /// </summary>
    public override SchemaInspector CreateInspector()
    {
        var container = new VisualElement();
        /*            public bool forceToMono = true;
            public bool loadInBackground = false;
            public AudioClipLoadType loadType = AudioClipLoadType.CompressedInMemory;
            public  compressionFormat = AudioCompressionFormat.Vorbis;
            public float  = 0.1f;
            public bool  = false;*/
        var forceToMonoField = new Toggle();
        forceToMonoField.label = "forceToMono";
        forceToMonoField.SetValueWithoutNotify(forceToMono);
        forceToMonoField.RegisterValueChangedCallback((evt) =>
        {
            forceToMono = evt.newValue;
        });
        container.Add(forceToMonoField);

        var loadInBackgroundField = new Toggle();
        loadInBackgroundField.label = "loadInBackground";
        loadInBackgroundField.SetValueWithoutNotify(loadInBackground);
        loadInBackgroundField.RegisterValueChangedCallback((evt) =>
        {
            loadInBackground = evt.newValue;
        });
        container.Add(loadInBackgroundField);
        
        var loadTypeField = new EnumField(loadType);
        loadTypeField.label = "loadType";
        loadTypeField.SetValueWithoutNotify(loadType);
        loadTypeField.RegisterValueChangedCallback((evt) =>
        {
            loadType = (AudioClipLoadType)evt.newValue;
        });
        container.Add(loadTypeField);

        var compressionFormatField = new EnumField(compressionFormat);
        compressionFormatField.label = "compressionFormat";
        compressionFormatField.SetValueWithoutNotify(compressionFormat);
        compressionFormatField.RegisterValueChangedCallback((evt) =>
        {
            compressionFormat = (AudioCompressionFormat)evt.newValue;
        });
        container.Add(compressionFormatField);
        
        var qualityField = new FloatField();
        qualityField.label = "quality";
        qualityField.SetValueWithoutNotify(quality);
        qualityField.RegisterValueChangedCallback((evt) =>
        {
            quality = evt.newValue;
        });
        container.Add(qualityField);

        var preloadAudioDataField = new Toggle();
        preloadAudioDataField.label = "preloadAudioData";
        preloadAudioDataField.SetValueWithoutNotify(preloadAudioData);
        preloadAudioDataField.RegisterValueChangedCallback((evt) =>
        {
            preloadAudioData = evt.newValue;
        });
        container.Add(preloadAudioDataField);

        SchemaInspector inspector = new SchemaInspector(container);
        return inspector;
    }
}
