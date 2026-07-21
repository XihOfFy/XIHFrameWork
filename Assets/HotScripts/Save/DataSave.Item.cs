using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Tmpl;
using UnityEngine;

namespace Hot
{
    public partial class DataSave
    {
        [NonSerialized] public Action<ItemEnum> ItemChangeAct;
        [NonSerialized] readonly HashSet<ItemEnum> invekItemSet = new HashSet<ItemEnum>(10);
        async void LazyInvokeItemChange(ItemEnum itemEnum)
        {
            if (invekItemSet.Contains(itemEnum)) return;
            invekItemSet.Add(itemEnum);
            await UniTask.Yield();
            invekItemSet.Remove(itemEnum);
            ItemChangeAct?.Invoke(itemEnum);
        }
        public int GetItemNum(ItemEnum itemEnum)
        {
            foreach (var item in itemDatas)
            {
                if (item.itemId == itemEnum) return item.num;
            }
            return 0;
        }
        public bool CanBuy(int costCoin)
        {
            return costCoin <= GetItemNum(ItemEnum.Coin);
        }
        public void AddCoin(int addCoin, int scene, int muti = 1)
        {
            ChangeItemNum(ItemEnum.Coin, addCoin, scene, muti);
        }
        public int ChangeItemNum(ItemEnum itemEnum, int dltNum, int scene = -1, int muti = 1)
        {
            ItemData itemData = null;
            foreach (var item in itemDatas)
            {
                if (item.itemId == itemEnum)
                {
                    itemData = item;
                    break;
                }
            }
            if (itemData == null)
            {
                itemData = new ItemData() { itemId = itemEnum };
                itemDatas.Add(itemData);
            }
            return ChangeItemNum(itemData, dltNum, scene, muti);
        }
        public int ChangeItemNum(ItemData itemData, int dltNum, int scene = -1, int muti = 1)
        {
            var oldNum = itemData.num;
            if (dltNum == 0) return oldNum;
            var total = itemData.num + dltNum;
            if (total < 0) total = 0;
            var cfg = Tables.Instance.TbProp.Get(itemData.itemId);
            itemData.num = Mathf.Min(total, cfg.MaxCount);
            if (oldNum != itemData.num)
            {
                SaveData();
                LazyInvokeItemChange(itemData.itemId);
            }
            if (itemData.itemId == ItemEnum.Coin)
            {
                if (scene == -1) Debug.LogError("暂时未添加此类场景下金币日志需求");
                TrackingReport.TrackGainCredits(dltNum);
            }
            return itemData.num;
        }
        /// <summary>
        ///  返回 是否可以使用 道具是否限制使用上限
        /// </summary>
        /// <param name="propCfg"></param>
        /// <returns></returns>
        public bool IsLmtUse(PropCfg propCfg)
        {
            if (itemUseAnalysic.TryGetValue(propCfg.ID, out var num))
            {
                if (num >= propCfg.MaxUseCount)
                {
                    return false;
                }
            }
            return true;
        }
        public bool IsLmtUse(ItemEnum itemEnum)
        {
            var cfg = Tables.Instance.TbProp.Get(itemEnum);
            return IsLmtUse(cfg);
        }
        public ItemData CanUseBagProp(ItemEnum itemId)
        {
            ItemData propRm = null;
            foreach (var prop in itemDatas)
            {
                if (prop.itemId == itemId)
                {
                    propRm = prop;
                    break;
                }
            }
            return propRm;
        }
        public void GMSetBagPropCount(ItemEnum itemId, int num)
        {
            ItemData propRm = null;
            foreach (var prop in itemDatas)
            {
                if (prop.itemId == itemId)
                {
                    propRm = prop;
                    break;
                }
            }
            if (propRm != null)
            {
                propRm.num = num;
            }
            else
            {
                itemDatas.Add(new ItemData() { num = num, itemId = itemId });
            }
            SaveData();
        }

    }
}
