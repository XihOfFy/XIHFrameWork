using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AudioAssetImport : AssetPostprocessor
{
    /*
     小游戏：https://unity.cn/instantgame/docs/WechatMinigame/Optimization/
     AudioClip在解压时，会在JS层占用大量的内存，因此音频的加载类型应该选择Compressed Included Memory。另外强制使用单通道音频，也能在音频播放时节省一半的内存消耗。 大多数情况下，导入音频的Quality选择1 和100，在听觉上并没有什么区别，而音频数据却能大幅减小，因此推荐在WebGL平台，将所有音频的Quality都调整为1。 总结而言，音频资源的内存优化方式有：
加载方式使用Compressed Included Memory
勾选Force To Mono，强制使用单声道音频
将Quality调整为1
     */
    public void OnPreprocessAudio()
    {
        if (!this.assetPath.StartsWith("Assets/Res/Audio/")) return;
        var importer = this.assetImporter as AudioImporter;
        if (importer == null) return;

        importer.forceToMono = true;
#if !UNITY_2022_2_OR_NEWER
        importer.preloadAudioData = false;
#endif
        var set = importer.defaultSampleSettings;
        set.loadType = AudioClipLoadType.CompressedInMemory;
        set.compressionFormat = AudioCompressionFormat.Vorbis;
        set.quality = 0.01f;
        importer.defaultSampleSettings = set;
        Debug.LogWarning($"{this.assetPath} 音频调整方便webgl优化");
        importer.SaveAndReimport();
    }
}
