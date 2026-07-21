using System.Collections.Generic;
using System;
using Tmpl;

namespace Hot
{
    public abstract partial class AbsDataSave<K>
    {
        [NonSerialized] public int useItemNum;//当前战斗使用的道具数量
        [NonSerialized] public int rebirthCount;//复活次数
        [NonSerialized] public Dictionary<ItemEnum, int> itemUseAnalysic;//道具使用统计
        public virtual void InitStartGame() {
            useItemNum = 0;
            rebirthCount = 0;
            itemUseAnalysic.Clear();
        }
    }
}
