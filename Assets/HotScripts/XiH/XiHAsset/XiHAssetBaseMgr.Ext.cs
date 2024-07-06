using Cysharp.Threading.Tasks;
using FairyGUI;
using UnityEngine;

namespace XiHAsset
{
    public  abstract partial class XiHAssetBaseMgr
    {
        class MonoDestoryedException : System.Exception
        {
        }
        /// <summary>
        /// 给FGUI的loader添加unity物体（特效）
        /// </summary>
        /// <param name="loader"></param>
        /// <param name="effPath">路径</param>
        /// <param name="destoryOld">是否销毁旧的再重新生成新的预制物体</param>
        /// <returns></returns>
        public async UniTask SetEffect4GLoader3D(GLoader3D loader, string effPath, bool destoryOld = false)
        {
            if (loader.wrapTarget == null)
            {
                var effHandle = await GetHandle<GameObject>(effPath);
                if (loader == null || loader.isDisposed) return;
                loader.SetWrapTarget(effHandle.InstantiateSync(), false, 1, 1);
            }
            else
            {
                if (destoryOld)
                {
                    var effHandle = await GetHandle<GameObject>(effPath);
                    if (loader == null || loader.isDisposed) return;
                    GameObject.Destroy(loader.wrapTarget);
                    loader.SetWrapTarget(effHandle.InstantiateSync(), false, 1, 1);
                }
            }
        }
    }
}
