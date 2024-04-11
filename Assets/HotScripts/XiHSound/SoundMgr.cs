using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XiHUtil;
using YooAsset;
#if UNITY_WX
using WeChatWASM;
#endif

namespace XiHSound
{
    public partial class SoundMgr : MonoBehaviour
    {
        protected static SoundMgr instance;
        public static SoundMgr Instance
        {
            get
            {
                if (instance == null)
                {
                    var obj = new GameObject(nameof(SoundMgr));
                    instance = obj.AddComponent<SoundMgr>();
                    DontDestroyOnLoad(obj);
                }
                return instance;
            }
        }

        public const string BGM_ENABLE = "BGMEnable";
        public const string SOUND_ENABLE = "SoundEnable";
        public const string VIBRATE_ENABLE = "VibrateEnable";
        public const string BGM_VOLUME = "BgmVolume";
        public const string SOUND_VOLUME = "SoundVolume";

        AudioListener listener;
        AudioSource soundSource;
        AudioSource musicSource;
        //AudioMixer audioMixer;
        bool bgmEnable;
        bool soundEnable;
        bool vibrate;
        WaitForSecondsRealtime unitSec;
        public bool BgmEnable
        {
            get => bgmEnable; set
            {
                PlayerPrefsUtil.Set(BGM_ENABLE, value);
                bgmEnable = value;
                MuteBgm(value);
            }
        }
        public bool SoundEnable
        {
            get => soundEnable; set
            {
                PlayerPrefsUtil.Set(SOUND_ENABLE, value);
                soundEnable = value;
            }
        }
        public bool Vibrate {
            get => vibrate;
            set{
                PlayerPrefsUtil.Set(VIBRATE_ENABLE, value);
                vibrate = value;
            }
        }



        float bgmVolume;
        float soundVolume;
        //db  -80 到 20
        public float BgmVolume
        {
            get => bgmVolume; set
            {
                PlayerPrefsUtil.Set(BGM_VOLUME, value);
                bgmVolume = value;
                //audioMixer.SetFloat(BGM_VOLUME, value - 80);
                musicSource.volume = value;
            }
        }
        public float SoundVolume
        {
            get => soundVolume; set
            {
                PlayerPrefsUtil.Set(SOUND_VOLUME, value);
                soundVolume = value;
                //audioMixer.SetFloat(SOUND_VOLUME, value - 80);
                soundSource.volume = value;
            }
        }

        Dictionary<string, AssetHandle> abHandles;
        FIFO recentBgmUseQue;
        FIFO recentSoundUseQue;
        private void Awake()
        {
            unitSec = new WaitForSecondsRealtime(0.1f);
            listener = gameObject.AddComponent<AudioListener>();
            musicSource = gameObject.AddComponent<AudioSource>();
            soundSource = gameObject.AddComponent<AudioSource>();
            bgmEnable = PlayerPrefsUtil.Get(BGM_ENABLE, true);
            soundEnable =PlayerPrefsUtil.Get(SOUND_ENABLE, true);
            vibrate = PlayerPrefsUtil.Get(VIBRATE_ENABLE, true);
            abHandles = new Dictionary<string, AssetHandle>();
            recentBgmUseQue = new FIFO(1);
            recentSoundUseQue = new FIFO(16);
            InitAudioMixer();
        }
        void InitAudioMixer()
        {
            /*var asset = YooAssets.LoadAssetAsync<AudioMixer>("Assets/Res/Audio/AudioMixer.mixer");
            await asset.ToUniTask();
            audioMixer=asset.AssetObject as AudioMixer;
            musicSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Master/Music")[0];
            soundSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Master/Sound")[0];*/
            bgmVolume = PlayerPrefsUtil.Get(BGM_VOLUME, 1.0f);
            musicSource.volume = bgmVolume;
            soundVolume = PlayerPrefsUtil.Get(SOUND_VOLUME, 1.0f);
            soundSource.volume = soundVolume;
        }
        public async UniTaskVoid PlayBGM(string bgmAB)
        {
            if (!bgmEnable) return;
            AssetHandle handle;
            if (abHandles.ContainsKey(bgmAB))
            {
                handle = abHandles[bgmAB];
                await UniTask.WaitUntil(() => handle.IsDone || !handle.IsValid);
                if (!handle.IsValid)
                {
                    abHandles.Remove(bgmAB);
                    PlayBGM(bgmAB).Forget();
                    Debug.LogWarning($"此音效ID {bgmAB} 已经释放，无法继续播放");
                    return;
                }
            }
            else
            {
                handle = YooAssets.LoadAssetAsync<AudioClip>(bgmAB);
                abHandles.Add(bgmAB, handle);
                await handle.ToUniTask();
            }
            recentBgmUseQue.InAndOut(bgmAB, abHandles);
            PlayBGM(handle.AssetObject as AudioClip);
        }
        public void StopBGM() {
            if (!bgmEnable) return;
            musicSource.Stop();
            //StartCoroutine(nameof(IEStopBGM));
        }
        void PlayBGM(AudioClip bgm)
        {
            if (!bgmEnable || bgm == null) return;
            musicSource.Stop();
            musicSource.volume = bgmVolume;
            musicSource.clip = bgm;
            musicSource.loop = true;
            musicSource.Play();
        }
        public async UniTaskVoid PlaySound(string soundAB)
        {
            if (!soundEnable) return;
            AssetHandle handle;

            if (abHandles.ContainsKey(soundAB))
            {
                handle = abHandles[soundAB];
                await UniTask.WaitUntil(() => handle.IsDone || !handle.IsValid);
                if (!handle.IsValid)
                {
                    abHandles.Remove(soundAB);
                    PlaySound(soundAB).Forget();
                    Debug.LogWarning($"此音效ID {soundAB} 已经释放，无法继续播放");
                    return;
                }
            }
            else {
                handle = YooAssets.LoadAssetAsync<AudioClip>(soundAB);
                abHandles.Add(soundAB, handle);
                await handle.ToUniTask();
            }
            recentSoundUseQue.InAndOut(soundAB, abHandles);
            PlaySound(handle.AssetObject as AudioClip);
        }
        void PlaySound(AudioClip sound)
        {
            if (!soundEnable || sound == null) return;
            soundSource.PlayOneShot(sound, soundVolume);
            //AudioSource.PlayClipAtPoint(sound,Vector3.zero,1);
        }
        public void PlayVibrate(int times=1) {
            while (--times >= 0) {
                if (!vibrate) return;
#if UNITY_ANDROID || UNITY_IOS
                Handheld.Vibrate();
#elif UNITY_DY
                if (Application.platform == RuntimePlatform.Android && !StarkSDKSpace.CanIUse.Vibrate)
                {
                    UnityEngine.Debug.LogError("当前宿主的Container版本过低，不可使用该接口");
                }
                else {
                    long[] pattern = { 0, 100, 1000, 300 };
                    StarkSDKSpace.StarkSDK.API.Vibrate(pattern);
                }
#elif UNITY_WX
                WX.VibrateShort(new VibrateShortOption() { type = "medium" });
#endif
            }
        }
        Coroutine bgmCor;
        void MuteBgm(bool mute)
        {
            if (bgmCor != null) StopCoroutine(bgmCor);
            if (mute)
            {
                bgmCor = StartCoroutine(nameof(MuteCor));
            }
            else
            {
                bgmCor = StartCoroutine(nameof(UnMuteCor));
            }
        }
        IEnumerator MuteCor()
        {
            float dlt = 0.05f;
            var unit = new WaitForSecondsRealtime(0.1f);
            while (musicSource.volume > 0)
            {
                musicSource.volume -= dlt;
                yield return unit;
            }
            musicSource.Stop();
            bgmCor = null;
        }
        IEnumerator UnMuteCor()
        {
            float dlt = 0.05f;
            var unit = new WaitForSecondsRealtime(0.1f);
            musicSource.Play();
            while (musicSource.volume < bgmVolume)
            {
                musicSource.volume += dlt;
                yield return unit;
            }
            musicSource.volume = bgmVolume;
            bgmCor = null;
        }
        IEnumerator IEPlayBGM()
        {
            float dlt = 0.04f;
            musicSource.volume = 0;
            musicSource.Play();
            while (musicSource.volume < bgmVolume)
            {
                musicSource.volume += dlt;
                yield return unitSec;
            }
            musicSource.volume = bgmVolume;
            bgmCor = null;
        }
        IEnumerator IEStopBGM()
        {
            float dlt = -0.04f;
            while (musicSource.volume > 0.05f)
            {
                musicSource.volume += dlt;
                yield return unitSec;
            }
            musicSource.Stop();
            bgmCor = null;
        }


        class FIFO {
            Dictionary<string, uint> dic;
            int mCount;
            uint order = 0;
            public FIFO(int maxCount)
            {
                dic = new Dictionary<string, uint>();
                mCount = maxCount<1?1:maxCount;
            }
            public void InAndOut(string item, Dictionary<string, AssetHandle> abHandles)
            {
                dic[item] = ++order;
                if (dic.Count > mCount)
                {
                    var kvs = dic.OrderBy(kv => kv.Value).ToList();
                    var maxIdx = (mCount + 1) >> 1;
                    foreach (var kv in kvs)
                    {
                        if (--maxIdx >= 0)
                        {
                            var key = kv.Key;
                            var tmp = abHandles[key];
                            if (tmp.IsDone)
                            {
                                tmp.Release();
                                abHandles.Remove(key);
                                dic.Remove(key);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    YooAssets.GetPackage(Aot.AotConfig.PACKAGE_NAME).UnloadUnusedAssets();
                }
            }
        }
    }
}
