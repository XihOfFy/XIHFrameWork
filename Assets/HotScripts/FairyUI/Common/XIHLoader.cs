using FairyGUI;

namespace Hot
{
    public class XIHLoader : GLoader
    {
        async override protected void LoadExternal()
        {
            /*
            开始外部载入，地址在url属性
            载入完成后调用OnExternalLoadSuccess
            载入失败调用OnExternalLoadFailed
            注意：如果是外部载入，在载入结束后，调用OnExternalLoadSuccess或OnExternalLoadFailed前，
            比较严谨的做法是先检查url属性是否已经和这个载入的内容不相符。
            如果不相符，表示loader已经被修改了。
            这种情况下应该放弃调用OnExternalLoadSuccess或OnExternalLoadFailed。
            */
            //url一定不为null或空，因为调用这个方法前已经在父类判断了
            //当url置空，这里不会执行！！！所以不能使用缓存preUrl判断
            var tmpUrl = url;
            var tex = await XiHAsset.XiHAssetBaseMgr.BaseInstance.GetOneSpriteInAtlas(tmpUrl);
            if (!tmpUrl.Equals(url))
            {
                return;
            }
            if (isDisposed) return;
            if (tex != null)
                onExternalLoadSuccess(new NTexture(tex));
            else
                onExternalLoadFailed();
        }
        override protected void FreeExternal(NTexture texture)
        {
            //释放外部载入的资源
            //无需处理，通过XiHAssetBaseMgr场景销毁对应AB
            texture.destroyMethod = DestroyMethod.None;
            texture.Unload();
        }
    }
}
