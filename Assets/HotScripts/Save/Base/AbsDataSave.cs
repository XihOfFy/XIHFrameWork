using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Tmpl;
using UnityEngine;
using XiHUtil;

namespace Hot
{
    public interface IDataSave {
        string SavePathKey { get; }
        void SaveData();
        void LoadData();
        void AfterLoad();
        IDataSave GetNewDataSave();
        bool CanUseRemote(IDataSave other);
        void SetInstanceByOther(IDataSave dataSave);
    }
    [Serializable]
    public class ItemData
    {
        public ItemEnum itemId;
        public int num;
    }
    public abstract partial class AbsDataSave<K> : Singleton<K> ,IDataSave where K : AbsDataSave<K>, new()
    {
        public abstract string SavePathKey { get; }
        protected bool saving;
        public void SaveData() {
            LazySave().Forget();
        }
        public void LoadData()
        {
            var json = PlayerPrefsUtil.Get(SavePathKey, "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var res = DataSaveAgent.SerializeByJson<K>(json);
                    /*var fileBytes = FileUtil.ReadFileBytes(SAVE_PATH);
                      if (fileBytes != null)
                      {
                          var bytes = Aot.XIHDecryptionServices.Decrypt(fileBytes);
                          var json = System.Text.Encoding.UTF8.GetString(bytes);
                          SetInstance(JsonUtility.FromJson<DataSave>(json));
                      }*/
                    SetInstance(res);
                    return;//正常返回
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            SetNewInstance();
        }
        public virtual void AfterLoad() {
            if (itemUseAnalysic == null) itemUseAnalysic = new Dictionary<ItemEnum, int>(10);
        }
        protected virtual async UniTaskVoid LazySave()
        {
            if (saving) return;
            saving = true;
            await UniTask.Yield();
            saving = false;
            try
            {
                var json = Instance.SerializeToJson();
                /*var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                var fileBytes = Aot.XIHDecryptionServices.Decrypt(bytes);
                FileUtil.WriteFile(SAVE_PATH, fileBytes);*/
                PlayerPrefsUtil.Set(SavePathKey, json);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public virtual bool CanUseRemote(IDataSave other)//主存档必须重写，方便后续存档进行同步
        {
            return false;
        }

        public void SetInstanceByOther(IDataSave dataSave)
        {
            if (dataSave == null) {
                dataSave = new K();
                dataSave.AfterLoad();
            }
            SetInstance(dataSave as K);
        }

        public IDataSave GetNewDataSave()
        {
            var data = new K();
            return data;
        }
        void SetNewInstance() {
            var data = new K();
            data.AfterLoad();
            SetInstance(data);
        }
    }
}
