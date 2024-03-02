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
    public class SoundMgr : MonoBehaviour
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
        Queue<string> recentBgmUseQue;
        Queue<string> recentSoundUseQue;
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
            recentBgmUseQue = new Queue<string>();
            recentSoundUseQue = new Queue<string>();
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
        public void ReleaseAll() {
            var set=new HashSet<string>();
            foreach (var str in recentBgmUseQue) set.Add(str);
            foreach (var str in recentSoundUseQue) set.Add(str);
            var all = abHandles.Keys.ToList();
            foreach (var key in all) {
                if (set.Contains(key)) continue;
                abHandles[key].Release();
                abHandles.Remove(key);
            }
            YooAssets.GetPackage(Aot.AotConfig.PACKAGE_NAME).UnloadUnusedAssets();
        }
        public async UniTaskVoid PlayBGM(string bgmAB)
        {
            if (!bgmEnable) return;
            AssetHandle handle;
            recentBgmUseQue.Enqueue(bgmAB);
            int maxCount = 4;//自行控制数量
            if (recentBgmUseQue.Count > maxCount)
            {
                maxCount >>= 1;
                while (--maxCount > 0) recentBgmUseQue.Dequeue();
                ReleaseAll();
            }
            if (abHandles.ContainsKey(bgmAB))
            {
                handle = abHandles[bgmAB];
            }
            else
            {
                handle = YooAssets.LoadAssetAsync<AudioClip>(bgmAB);
                await handle.ToUniTask();
                //await可能触发多次,再判断一次
                if (abHandles.ContainsKey(bgmAB))
                {
                    handle.Release();
                    handle = abHandles[bgmAB];
                }
                else
                {
                    abHandles.Add(bgmAB, handle);
                }
            }
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
            recentSoundUseQue.Enqueue(soundAB);
            int maxCount = 8;//自行控制数量
            if (recentSoundUseQue.Count > maxCount)
            {
                maxCount >>= 1;
                while (--maxCount > 0) recentSoundUseQue.Dequeue();
                ReleaseAll();
            }
            if (abHandles.ContainsKey(soundAB))
            {
                handle = abHandles[soundAB];
            }
            else {
                handle = YooAssets.LoadAssetAsync<AudioClip>(soundAB);
                await handle.ToUniTask();
                //await可能触发多次,再判断一次
                if (abHandles.ContainsKey(soundAB))
                {
                    handle.Release();
                    handle = abHandles[soundAB];
                }
                else {
                    abHandles.Add(soundAB, handle);
                }
            }
            PlaySound(handle.AssetObject as AudioClip);
        }
        void PlaySound(AudioClip sound)
        {
            if (!soundEnable || sound == null) return;
            soundSource.PlayOneShot(sound, soundVolume);
            //AudioSource.PlayClipAtPoint(sound,Vector3.zero,1);
        }
        public async void PlayVibrate(int times=1) {
            while (--times >= 0) {
                if (!vibrate) return;
#if UNITY_ANDROID || UNITY_IOS
                Handheld.Vibrate();
#elif UNITY_WX
                WX.VibrateShort(new VibrateShortOption() { type = "medium" });
#endif
                await UniTask.Delay(500);
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
    }
}
