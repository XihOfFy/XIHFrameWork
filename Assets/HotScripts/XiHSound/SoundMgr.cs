using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XiHUtil;
using YooAsset;

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

        AudioListener listener;
        AudioSource source;
        private const float VOLUME = 1;//内部设置，默认为1，外部只能静音或者外放
        bool bgmEnable;
        bool soundEnable;
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

        Dictionary<string, AssetHandle> abHandles;
        Queue<string> recentBgmUseQue;
        Queue<string> recentSoundUseQue;
        private void Awake()
        {
            unitSec = new WaitForSecondsRealtime(0.1f);
            listener = gameObject.AddComponent<AudioListener>();
            source = gameObject.AddComponent<AudioSource>();
            bgmEnable = PlayerPrefsUtil.Get(BGM_ENABLE, true);
            soundEnable =PlayerPrefsUtil.Get(SOUND_ENABLE, true);
            abHandles = new Dictionary<string, AssetHandle>();
            recentBgmUseQue = new Queue<string>();
            recentSoundUseQue = new Queue<string>();
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
                abHandles.Add(bgmAB, handle);
            }
            PlayBGM(handle.AssetObject as AudioClip);
        }
        public void PlayBGM(AudioClip bgm)
        {
            if (!bgmEnable || bgm == null) return;
            source.Stop();
            source.volume = VOLUME;
            source.clip = bgm;
            source.loop = true;
            source.Play();
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
                abHandles.Add(soundAB, handle);
            }
            PlaySound(handle.AssetObject as AudioClip);
        }
        public void PlaySound(AudioClip sound)
        {
            if (!soundEnable || sound == null) return;
            source.PlayOneShot(sound, 1.0f);
            //AudioSource.PlayClipAtPoint(sound,Vector3.zero,1);
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
            while (source.volume > 0)
            {
                source.volume -= dlt;
                yield return unit;
            }
            source.Stop();
            bgmCor = null;
        }
        IEnumerator UnMuteCor()
        {
            float dlt = 0.05f;
            var unit = new WaitForSecondsRealtime(0.1f);
            source.Play();
            while (source.volume < VOLUME)
            {
                source.volume += dlt;
                yield return unit;
            }
            source.volume = VOLUME;
            bgmCor = null;
        }
        IEnumerator IEPlayBGM()
        {
            float dlt = 0.04f;
            var volume = source.volume;
            source.volume = 0;
            source.Play();
            while (source.volume < volume)
            {
                source.volume += dlt;
                yield return unitSec;
            }
            source.volume = volume;
            bgmCor = null;
        }
        IEnumerator IEStopBGM()
        {
            float dlt = -0.04f;
            while (source.volume > 0.05f)
            {
                source.volume += dlt;
                yield return unitSec;
            }
            source.Stop();
            bgmCor = null;
        }
    }
}
