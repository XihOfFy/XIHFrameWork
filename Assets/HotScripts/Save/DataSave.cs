using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hot
{
    [Serializable]
    public partial class DataSave : AbsDataSave<DataSave>
    {
        public override string SavePathKey => "data0";
        public string userId;
        public int stageId;//当前挑战的关卡

        [SerializeField] public List<ItemData> itemDatas;
        public DataSave()
        {
        }

        public override void AfterLoad()
        {
            base.AfterLoad();
            if (stageId < 1) stageId = 1;
            if (string.IsNullOrEmpty(userId)) userId = GenUserId();
            if (itemDatas == null) itemDatas = new List<ItemData>(10);
        }
    }
}
