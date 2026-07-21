using System;
using Tmpl;
using Random = UnityEngine.Random;

namespace Hot
{
    public partial class DataSave
    {
        public override void InitStartGame()
        {
            base.InitStartGame();
            ItemChangeAct = null;
        }
        //         public bool GetStageCfg(out StageCfg stageCfg) {
        //             stageCfg = GetStageCfg(stageId);
        // /*            var energy = GetItemNum(ItemEnum.Energy);
        //             if (energy <= 0) {
        //                 UIUtil.OpenDialogAsync<EnergyBuyDialog>().Forget();
        //                 return false;
        //             }*/
        //             return true;
        //         }
        //         public StageCfg GetStageCfg(int stageId)
        //         {
        //             var maxStage = Tables.Instance.TbStage.DataList.Count;
        //             if (stageId > maxStage)
        //             {
        //                 var loopStart = Tables.Instance.TbParam.LoopStage;
        //                 var mod = maxStage - loopStart + 1;
        //                 stageId -= loopStart;
        //                 stageId %= mod;
        //                 stageId += loopStart;
        //             }
        //             return Tables.Instance.TbStage.Get(stageId);
        //         }
        //         //0 失败 1 胜利 2 放弃
        //         public void GameOver(GameResultEnum state) {
        //             stageId = stageId + 1;
        //             if (state == GameResultEnum.Pass)
        //             {
        //                 //上传排行榜数据
        //                 ChannelManager.Instance.SetImRankData(0, stageId);
        //             }
        //             SaveData();
        //         }
        string GenUserId()
        {
            var time = DateTime.UtcNow.Ticks;
            var random = Random.Range(0, 10000);
            return TbApp.AppCfg.ID + "_" + time + "_" + random;
        }
    }
}
