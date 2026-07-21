using Aot.XiHUtil;
using Cysharp.Threading.Tasks;
using System;
using Tmpl;
using UnityEngine;
using XiHAsset;
using XiHUtil;

#if UNITY_WX
using WeChatWASM;
#endif

namespace Hot
{
    public static partial class WxTool
    {
#if UNITY_WX
        public static async UniTask WXCheckSaveAuth(string imgUrl, string imgName,int id)
        {
            var end = false;
            WeChatWASM.WX.GetSetting(new WeChatWASM.GetSettingOption()
            {
                fail = res =>
                {
                    Debug.LogError("获取授权信息失败: " + res.errMsg);
                    end = true;
                },
                success = async res =>
                {
                    if (res.authSetting.TryGetValue("scope.writePhotosAlbum", out var agree) && agree)
                    {
                        await WXSavePngToLocal(() => end = true, imgUrl, imgName, id);
                    }
                    else
                    {
                        // 未授权成功
                        WeChatWASM.WX.Authorize(new WeChatWASM.AuthorizeOption()
                        {
                            scope = "scope.writePhotosAlbum",
                            success = async res =>
                            {
                                await WXSavePngToLocal(() => end = true, imgUrl, imgName, id);
                            },
                            fail = res =>
                            {
                                Debug.LogError("未授权成功: " + res.errMsg);
                                end = true;
                            }
                        });
                    }
                }
            });
            await UniTask.WaitUntil(() => end);
        }

        static async UniTask WXSavePngToLocal(Action callback, string imgUrl, string imgName, int id)
        {
            var end = false;
            var fullPath = await SaveSprite2TmpPath(imgUrl, imgName);
            WeChatWASM.WX.SaveImageToPhotosAlbum(new WeChatWASM.SaveImageToPhotosAlbumOption
            {
                filePath = fullPath,
                success = res =>
                {
                    // 保存成功后
                    UIUtil.ShowSystemTip(760001.Translate());
                },
                fail = res =>
                {
                    Debug.LogError("保存图片失败: " + res.errMsg);
                },
                complete = res =>
                {
                    end = true;
                }
            });
            await UniTask.WaitUntil(() => end);
            callback?.Invoke();
        }

#endif
    }
}
