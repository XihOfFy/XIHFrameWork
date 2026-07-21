using System.Collections.Generic;
using UnityEngine;

namespace Hot
{
    public static partial class DataSaveAgent
    {
        public readonly static List<IDataSave> AllData = new List<IDataSave>();
        public static void InitData()
        {
            DataSave.Instance.LoadData();
            //PuzzleSave.Instance.LoadData();
            ReAddAllData();
        }
        static void ReAddAllData() {
            AllData.Clear();
            AllData.Add(DataSave.Instance);//主存档必须第一个Add，方便后续远程存档进行对比
            //AllData.Add(PuzzleSave.Instance);
        }
        public static void ClearAllData() {
            var data = new DataSave();
            data.AfterLoad();
            DataSave.SetInstance(data);
/*            var data2 = new PuzzleSave();
            data2.AfterLoad();
            PuzzleSave.SetInstance(data2);*/
            ReAddAllData();
        }
        public static T SerializeByJson<T>(string dataJson) where T : class, IDataSave, new()
        {
            var remoteData = JsonUtility.FromJson<T>(dataJson);
            if (remoteData == null) remoteData = new T();
            remoteData.AfterLoad();
            return remoteData;
        }
        public static IDataSave SerializeByJson(string dataJson, IDataSave other)
        {
            var remoteData = JsonUtility.FromJson(dataJson, other.GetType()) as IDataSave;
            if (remoteData == null) remoteData = other.GetNewDataSave();
            remoteData.AfterLoad();
            return remoteData;
        }
        public static string SerializeToJson(this IDataSave other)
        {
            var remoteData = JsonUtility.ToJson(other);
            return remoteData;
        }
    }
}
