using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YooAsset.Editor;

public class SchemaTools
{
    #region 一键完成全部扫描和设置
    [MenuItem("YooAsset/ArtScanAndFixAll (耗时)")]
    static void ArtScanAndFixAll() {
        var files = Directory.GetFiles("Assets/Editor/YooSetting/ScannerOutput", "*.json");
        foreach (var file in files) File.Delete(file);
        AssetArtScannerSettingData.ScanAll();
        files = Directory.GetFiles("Assets/Editor/YooSetting/ScannerOutput", "*.json");
        foreach (var file in files) {
            Debug.LogWarning("开始执行:"+file);
            var _reportCombiner = new ScanReportCombiner();
            try
            {
                var scanReport = ScanReportConfig.ImportJsonConfig(file);
                _reportCombiner.Combine(scanReport);
                _reportCombiner.FixAll();
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError(e.StackTrace);
            }
        }
        Debug.LogWarning("执行完毕");
    }
    #endregion


    #region EditorUtility
    /// <summary>
    /// 搜集资源
    /// </summary>
    /// <param name="searchType">搜集的资源类型</param>
    /// <param name="searchInFolders">指定搜索的文件夹列表</param>
    /// <returns>返回搜集到的资源路径列表</returns>
    public static string[] FindAssets(string searchType, string[] searchInFolders)
    {
        // 注意：AssetDatabase.FindAssets()不支持末尾带分隔符的文件夹路径
        for (int i = 0; i < searchInFolders.Length; i++)
        {
            string folderPath = searchInFolders[i];
            searchInFolders[i] = folderPath.TrimEnd('/');
        }

        // 注意：获取指定目录下的所有资源对象（包括子文件夹）
        string[] guids;
        if (searchType == EAssetSearchType.All.ToString())
            guids = AssetDatabase.FindAssets(string.Empty, searchInFolders);
        else
            guids = AssetDatabase.FindAssets($"t:{searchType}", searchInFolders);

        // 注意：AssetDatabase.FindAssets()可能会获取到重复的资源
        HashSet<string> result = new HashSet<string>();
        for (int i = 0; i < guids.Length; i++)
        {
            string guid = guids[i];
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (result.Contains(assetPath) == false)
            {
                result.Add(assetPath);
            }
        }

        // 返回结果
        return result.ToArray();
    }
    #endregion

    /// <summary>
    /// 通用扫描快捷方法
    /// </summary>
    public static List<ReportElement> ScanAssets(string[] scanAssetList, System.Func<string, ReportElement> scanFun, int unloadAssetLimit = int.MaxValue)
    {
        int scanNumber = 0;
        int progressCount = 0;
        int totalCount = scanAssetList.Length;
        List<ReportElement> results = new List<ReportElement>(totalCount);

        EditorTools.ClearProgressBar();
        foreach (string assetPath in scanAssetList)
        {
            scanNumber++;
            progressCount++;
            EditorTools.DisplayProgressBar("扫描中...", progressCount, totalCount);
            var scanResult = scanFun.Invoke(assetPath);
            if (scanResult != null)
                results.Add(scanResult);

            // 释放编辑器未使用的资源
            if (scanNumber >= unloadAssetLimit)
            {
                scanNumber = 0;
                EditorUtility.UnloadUnusedAssetsImmediate(true);
            }
        }
        EditorTools.ClearProgressBar();

        return results;
    }

    /// <summary>
    /// 通用修复快捷方法
    /// </summary>
    public static void FixAssets(List<ReportElement> fixAssetList, System.Action<ReportElement> fixFun, int unloadAssetLimit = int.MaxValue)
    {
        int scanNumber = 0;
        int totalCount = fixAssetList.Count;
        int progressCount = 0;
        EditorTools.ClearProgressBar();
        foreach (var scanResult in fixAssetList)
        {
            scanNumber++;
            progressCount++;
            EditorTools.DisplayProgressBar("修复中...", progressCount, totalCount);
            fixFun.Invoke(scanResult);

            // 释放编辑器未使用的资源
            if (scanNumber >= unloadAssetLimit)
            {
                scanNumber = 0;
                EditorUtility.UnloadUnusedAssetsImmediate(true);
            }
        }
        EditorTools.ClearProgressBar();
    }
}
