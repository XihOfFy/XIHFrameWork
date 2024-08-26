using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XiHUtil;
using YooAsset;
using Tmpl;

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

        const string BGM_ENABLE = "BGMEnable";
        const string SOUND_ENABLE = "SoundEnable";
        const string VIBRATE_ENABLE = "VibrateEnable";
        const string BGM_VOLUME = "BgmVolume";
        const string SOUND_VOLUME = "SoundVolume";

        AudioListener listener;
        AudioSource soundSource;
        AudioSource musicSource;
        //AudioMixer audioMixer;
        bool bgmEnable;
        bool soundEnable;
        bool vibrate;
        public bool BgmEnable
        {
            get => bgmEnable; set
            {
                PlayerPrefsUtil.Set(BGM_ENABLE, value);
                bgmEnable = value;
                BgmVolume = value ? 1 : 0;
            }
        }
        public bool SoundEnable
        {
            get => soundEnable; set
            {
                PlayerPrefsUtil.Set(SOUND_ENABLE, value);
                soundEnable = value;
                SoundVolume = value ? 1 : 0;
            }
        }
        public bool Vibrate
        {
            get => vibrate;
            set
            {
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

        Dictionary<int, AssetHandle> abHandles;
        LRU recentBgmUseQue;
        LRU recentSoundUseQue;
        int curBgId;
        private void Awake()
        {
            listener = gameObject.AddComponent<AudioListener>();
            musicSource = gameObject.AddComponent<AudioSource>();
            soundSource = gameObject.AddComponent<AudioSource>();
            bgmEnable = PlayerPrefsUtil.Get(BGM_ENABLE, true);
            soundEnable = PlayerPrefsUtil.Get(SOUND_ENABLE, true);
            vibrate = PlayerPrefsUtil.Get(VIBRATE_ENABLE, true);
            abHandles = new Dictionary<int, AssetHandle>();
            recentBgmUseQue = new LRU(1);
            recentSoundUseQue = new LRU(16);
            InitAudioMixer();
            curBgId = int.MinValue;
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
        public void PlayBGM(int bgId)
        {
            if (curBgId == bgId) return;
            curBgId = bgId;
            PlayBGMById(bgId).Forget();
        }
        async UniTaskVoid PlayBGMById(int bgId)
        {
            //if (!bgmEnable) return;
            var cfg = Tables.Instance.TbAudio.Get(bgId);
            if (cfg == null)
            {
                StopBGM();
                Debug.LogError("无此音乐ID：" + bgId);
                return;
            }
            AssetHandle handle;
            if (abHandles.ContainsKey(bgId))
            {
                handle = abHandles[bgId];
                await UniTask.WaitUntil(() => handle.IsDone || !handle.IsValid);
                if (!handle.IsValid)
                {
                    abHandles.Remove(bgId);
                    PlayBGMById(bgId).Forget();
                    Debug.LogWarning($"此音效ID {bgId} 已经释放，无法继续播放");
                    return;
                }
            }
            else
            {
                handle = YooAssets.LoadAssetAsync<AudioClip>(cfg.Path);
                abHandles.Add(bgId, handle);
                await handle.ToUniTask();
            }
            recentBgmUseQue.InAndOut(bgId, abHandles);
            if (curBgId == bgId) PlayBGM(handle.AssetObject as AudioClip);
        }
        public void StopBGM()
        {
            //if (!bgmEnable) return;
            musicSource.Stop();
            curBgId = int.MinValue;
        }
        public void PauseBGM()
        {
            musicSource.Pause();
        }
        public void UnPause()
        {
            musicSource.UnPause();
        }
        void PlayBGM(AudioClip bgm)
        {
            //if (!bgmEnable || bgm == null) return;
            if (bgm == null) return;
            musicSource.Stop();
            musicSource.volume = bgmVolume;
            musicSource.clip = bgm;
            musicSource.loop = true;
            musicSource.Play();
        }
        public void PlaySound(int soundId)
        {
            PlaySoundById(soundId).Forget();
        }
        async UniTaskVoid PlaySoundById(int soundId)
        {
            if (!soundEnable) return;
            var cfg = Tables.Instance.TbAudio.Get(soundId);
            if (cfg == null)
            {
                Debug.LogError("无此音效ID：" + soundId);
                return;
            }
            AssetHandle handle;

            if (abHandles.ContainsKey(soundId))
            {
                handle = abHandles[soundId];
                await UniTask.WaitUntil(() => handle.IsDone || !handle.IsValid);
                if (!handle.IsValid)
                {
                    abHandles.Remove(soundId);
                    PlaySoundById(soundId).Forget();
                    Debug.LogWarning($"此音效ID {soundId} 已经释放，无法继续播放");
                    return;
                }
            }
            else
            {
                handle = YooAssets.LoadAssetAsync<AudioClip>(cfg.Path);
                abHandles.Add(soundId, handle);
                await handle.ToUniTask();
            }
            recentSoundUseQue.InAndOut(soundId, abHandles);
            PlaySound(handle.AssetObject as AudioClip);
        }
        void PlaySound(AudioClip sound)
        {
            if (!soundEnable || sound == null) return;
            soundSource.PlayOneShot(sound, soundVolume);
            //AudioSource.PlayClipAtPoint(sound,Vector3.zero,1);
        }
        public void PlayVibrate(int times = 1)
        {
            while (--times >= 0)
            {
                if (!vibrate) return;
                PlatformUtil.Vibrate();
            }
        }
        class LRU
        {
            Dictionary<int, uint> dic;
            int mCount;
            uint order = 0;
            public LRU(int maxCount)
            {
                dic = new Dictionary<int, uint>();
                mCount = maxCount < 1 ? 1 : maxCount;
            }
            public void InAndOut(int item, Dictionary<int, AssetHandle> abHandles)
            {
                dic[item] = ++order;
                if (dic.Count > mCount)
                {
                    var kvs = dic.OrderBy(kv => kv.Value).ToList();
                    var maxIdx = (mCount + 1) >> 1;
                    bool gc = false;
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
                                gc = true;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (gc) PlatformUtil.TriggerGC();
                }
            }
        }
    }
}
