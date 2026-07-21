namespace Hot
{
    public partial class DataSave
    {
        //主存档实现决策
        public override bool CanUseRemote(IDataSave other)
        {
            var otherData = other as DataSave;
            return otherData.stageId > stageId;
        }
    }
}
