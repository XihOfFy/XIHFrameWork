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
    static void ArtScanAndFixAll()
    {
        // 注意：AssetArtScannerSettingData.ScanAll() 只跑扫描，不会把报告写到 SaveDirectory。
        // 原先「扫完再读 ScannerOutput/*.json 再 Fix」会在清空 JSON 后拿不到任何报告，导致修复完全不生效。
        // 另外多个 Schema 的 ReportName/ReportDesc 可能相同，落盘时会互相覆盖。
        // 正确做法：逐个扫描器在内存中 Combine + FixAll，可选顺便落盘便于人工查看。
        var scanners = AssetArtScannerSettingData.Setting.Scanners;
        if (scanners == null || scanners.Count == 0)
        {
            Debug.LogWarning("ArtScanAndFixAll: 没有配置任何 Scanner。");
            return;
        }

        int okCount = 0;
        int failCount = 0;
        foreach (var scanner in scanners)
        {
            Debug.LogWarning($"开始扫描并修复: {scanner.ScannerName}");
            var scanResult = AssetArtScannerSettingData.Scan(scanner.ScannerGUID);
            if (!scanResult.Succeed || scanResult.Report == null)
            {
                failCount++;
                Debug.LogError($"{scanner.ScannerName} 扫描失败: {scanResult.ErrorInfo}\n{scanResult.ErrorStack}");
                continue;
            }

            if (!string.IsNullOrEmpty(scanner.SaveDirectory))
            {
                try
                {
                    if (!Directory.Exists(scanner.SaveDirectory))
                        Directory.CreateDirectory(scanner.SaveDirectory);
                    // 用 ScannerName 区分文件，避免同名报告互相覆盖
                    var savePath = Path.Combine(scanner.SaveDirectory, $"{scanner.ScannerName}.json");
                    ScanReportConfig.ExportJsonConfig(savePath, scanResult.Report);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"{scanner.ScannerName} 报告落盘失败（不影响修复）: {e.Message}");
                }
            }

            try
            {
                var reportCombiner = new ScanReportCombiner();
                if (!reportCombiner.Combine(scanResult.Report))
                {
                    failCount++;
                    Debug.LogError($"{scanner.ScannerName} Combine 报告失败");
                    continue;
                }
                reportCombiner.FixAll();
                okCount++;
            }
            catch (System.Exception e)
            {
                failCount++;
                Debug.LogError($"{scanner.ScannerName} 修复失败: {e}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.LogWarning($"ArtScanAndFixAll 执行完毕: 成功 {okCount}, 失败 {failCount}");
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
