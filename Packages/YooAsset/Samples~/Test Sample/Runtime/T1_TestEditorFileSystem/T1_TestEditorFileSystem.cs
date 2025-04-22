﻿using System;
using System.IO;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using UnityEngine.TestTools;
using NUnit.Framework;
using YooAsset;

public class T1_TestEditorFileSystem : IPrebuildSetup, IPostBuildCleanup
{
    private const string ASSET_BUNDLE_PACKAGE_ROOT_KEY = "T1_ASSET_BUNDLE_PACKAGE_ROOT_KEY";
    private const string RAW_BUNDLE_PACKAGE_ROOT_KEY = "T1_RAW_BUNDLE_PACKAGE_ROOT_KEY";

    void IPrebuildSetup.Setup()
    {
#if UNITY_EDITOR
        // 构建资源包
        {
            var simulateParams = new PackageInvokeBuildParam(TestDefine.AssetBundlePackageName);
            simulateParams.BuildPipelineName = "EditorSimulateBuildPipeline";
            simulateParams.InvokeAssmeblyName = "YooAsset.Test.Editor";
            simulateParams.InvokeClassFullName = "TestPackageBuilder";
            simulateParams.InvokeMethodName = "BuildPackage";
            var simulateResult = PakcageInvokeBuilder.InvokeBuilder(simulateParams);
            UnityEditor.EditorPrefs.SetString(ASSET_BUNDLE_PACKAGE_ROOT_KEY, simulateResult.PackageRootDirectory);
        }

        // 构建资源包
        {
            var simulateParams = new PackageInvokeBuildParam(TestDefine.RawBundlePackageName);
            simulateParams.BuildPipelineName = "EditorSimulateBuildPipeline";
            simulateParams.InvokeAssmeblyName = "YooAsset.Test.Editor";
            simulateParams.InvokeClassFullName = "TestPackageBuilder";
            simulateParams.InvokeMethodName = "BuildPackage";
            var simulateResult = PakcageInvokeBuilder.InvokeBuilder(simulateParams);
            UnityEditor.EditorPrefs.SetString(RAW_BUNDLE_PACKAGE_ROOT_KEY, simulateResult.PackageRootDirectory);
        }
#endif
    }
    void IPostBuildCleanup.Cleanup()
    {
    }

    [UnityTest]
    public IEnumerator A_InitializePackage()
    {
        // 初始化资源包
        {
            string packageRoot = string.Empty;
#if UNITY_EDITOR
            packageRoot = UnityEditor.EditorPrefs.GetString(ASSET_BUNDLE_PACKAGE_ROOT_KEY);
#endif
            if (Directory.Exists(packageRoot) == false)
                throw new Exception($"Not found package root : {packageRoot}");

            var package = YooAssets.CreatePackage(TestDefine.AssetBundlePackageName);

            // 初始化资源包
            var initParams = new EditorSimulateModeParameters();
            initParams.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            var initializeOp = package.InitializeAsync(initParams);
            yield return initializeOp;
            if (initializeOp.Status != EOperationStatus.Succeed)
                Debug.LogError(initializeOp.Error);
            Assert.AreEqual(EOperationStatus.Succeed, initializeOp.Status);

            // 请求资源版本
            var requetVersionOp = package.RequestPackageVersionAsync();
            yield return requetVersionOp;
            if (requetVersionOp.Status != EOperationStatus.Succeed)
                Debug.LogError(requetVersionOp.Error);
            Assert.AreEqual(EOperationStatus.Succeed, requetVersionOp.Status);

            // 更新资源清单
            var updateManifestOp = package.UpdatePackageManifestAsync(requetVersionOp.PackageVersion);
            yield return updateManifestOp;
            if (updateManifestOp.Status != EOperationStatus.Succeed)
                Debug.LogError(updateManifestOp.Error);
            Assert.AreEqual(EOperationStatus.Succeed, updateManifestOp.Status);
        }

        // 初始化资源包
        {
            string packageRoot = string.Empty;
#if UNITY_EDITOR
            packageRoot = UnityEditor.EditorPrefs.GetString(RAW_BUNDLE_PACKAGE_ROOT_KEY);
#endif
            if (Directory.Exists(packageRoot) == false)
                throw new Exception($"Not found package root : {packageRoot}");

            var package = YooAssets.CreatePackage(TestDefine.RawBundlePackageName);

            // 初始化资源包
            var initParams = new EditorSimulateModeParameters();
            initParams.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            var initializeOp = package.InitializeAsync(initParams);
            yield return initializeOp;
            if (initializeOp.Status != EOperationStatus.Succeed)
                Debug.LogError(initializeOp.Error);
            Assert.AreEqual(EOperationStatus.Succeed, initializeOp.Status);

            // 请求资源版本
            var requetVersionOp = package.RequestPackageVersionAsync();
            yield return requetVersionOp;
            if (requetVersionOp.Status != EOperationStatus.Succeed)
                Debug.LogError(requetVersionOp.Error);
            Assert.AreEqual(EOperationStatus.Succeed, requetVersionOp.Status);

            // 更新资源清单
            var updateManifestOp = package.UpdatePackageManifestAsync(requetVersionOp.PackageVersion);
            yield return updateManifestOp;
            if (updateManifestOp.Status != EOperationStatus.Succeed)
                Debug.LogError(updateManifestOp.Error);
            Assert.AreEqual(EOperationStatus.Succeed, updateManifestOp.Status);
        }
    }

    [UnityTest]
    public IEnumerator B1_TestLoadAsyncTask()
    {
        var tester = new TestLoadPanel();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B2_TestLoadAudio()
    {
        var tester = new TestLoadAudio();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B3_TestLoadImage()
    {
        var tester = new TestLoadImage();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B4_TestLoadPrefab()
    {
        var tester = new TestLoadPrefab();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B5_TestLoadSpriteAtlas()
    {
        var tester = new TestLoadSpriteAtlas();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B6_TestLoadScriptableObject()
    {
        var tester = new TestLoadScriptableObject();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B7_TestLoadScene()
    {
        var tester = new TestLoadScene();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B8_TestLoadRawFile()
    {
        var tester = new TestLoadRawFile();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator B9_TestLoadVideo()
    {
        var tester = new TestLoadVideo();
        yield return tester.RuntimeTester();
    }

    [UnityTest]
    public IEnumerator C_DestroyPackage()
    {
        // 销毁旧资源包
        {
            var package = YooAssets.GetPackage(TestDefine.AssetBundlePackageName);
            var destroyOp = package.DestroyAsync();
            yield return destroyOp;
            if (destroyOp.Status != EOperationStatus.Succeed)
                Debug.LogError(destroyOp.Error);
            Assert.AreEqual(EOperationStatus.Succeed, destroyOp.Status);

            bool result = YooAssets.RemovePackage(TestDefine.AssetBundlePackageName);
            Assert.IsTrue(result);
        }

        // 销毁旧资源包
        {
            var package = YooAssets.GetPackage(TestDefine.RawBundlePackageName);
            var destroyOp = package.DestroyAsync();
            yield return destroyOp;
            if (destroyOp.Status != EOperationStatus.Succeed)
                Debug.LogError(destroyOp.Error);
            Assert.AreEqual(EOperationStatus.Succeed, destroyOp.Status);

            bool result = YooAssets.RemovePackage(TestDefine.RawBundlePackageName);
            Assert.IsTrue(result);
        }
    }
}