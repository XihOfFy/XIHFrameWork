using Cysharp.Threading.Tasks;
using FairyGUI;
using System;
using System.Collections.Generic;
using UnityEngine;
using XiHUtil;
using YooAsset;

namespace XiHUI
{
    /// <summary>
    /// UI窗口管理器
    /// </summary>
    public partial class UIDialogManager
    {
        public async UniTask<UIDialog> OpenAsync(string dialogName, string packageName, string componentName, Mode layer = 0, bool isFull = true, bool isBlur = false)
        {
            if (!_dialogType.TryGetValue(dialogName, out var type))
                return null;

            if (!_layers.TryGetValue(layer, out var stack))
                return null;

            var dialog = stack.Get(dialogName);
            if (dialog != null && dialog.State == State.Loading)
                return null;

            if (dialog != null)
            {
                stack.Push(dialog);
                return dialog;
            }
            else
            {
                dialog = Activator.CreateInstance(type) as UIDialog;
                dialog.SetOpenParams(new DialogOpenParams(dialogName, packageName, componentName, layer, isFull, isBlur));
                stack.Push(dialog);
                return await CreateDialogAsync(dialogName, packageName, componentName, layer, isFull, isBlur);
            }
        }

        private async UniTask<UIDialog> CreateDialogAsync(string dialogName, string packageName, string componentName, Mode layer, bool isFull, bool isBlur)
        {
            if (!_layers.TryGetValue(layer, out var stack))
                return null;

            var dialog = stack.Get(dialogName);
            if (dialog == null || dialog.State != State.Loading)
                return null;

            var compent = await LoadUIComponentAsync(packageName, componentName, dialog);

            if (compent == null)
            {
                stack.Pop(dialog);
                return null;
            }

            // 加载完成了，打开之前再判断UI是否还处于打开状态
            if (dialog != null && dialog.State == State.Loading)
            {
                UIControlBinding.BindFields(dialog, compent);
                dialog.Open(compent, isFull, isBlur);
                stack.Push(dialog);
                dialog.Open();
            }
            return dialog;
        }

        private async UniTask<GComponent> LoadUIComponentAsync(string packageName, string componentName, UIDialog reference)
        {
            var package = await GetUIPackageAsync(packageName, reference);

            if (package == null)
            {
                return null;
            }

            var obj = package.Reference(null).CreateObject(componentName);
            return obj.asCom;
        }

        private async UniTask<UIPackageReference> GetUIPackageAsync(string packageName, UIDialog reference = null)
        {
            if (!_packages.TryGetValue(packageName, out var pkg))
            {
                pkg = await LoadUIPackageAsync(packageName, reference);
                _packages[packageName] = pkg;
            }
            else if (pkg != null)
                pkg.Reference(reference);

            return pkg;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fguiPath">不用包含_fgui和后缀</param>
        /// <param name="reference"></param>
        /// <returns></returns>
        private async UniTask<UIPackageReference> LoadUIPackageAsync(string packageName, UIDialog reference)
        {
            var _handles = new List<AssetHandle>(100);
            string locationPreffix = "Assets/Res/FairyRes/" + packageName + "/" + packageName;
            var yoores = YooAssets.GetPackage(Aot.AotConfig.PACKAGE_NAME);
            var handle = yoores.LoadAssetAsync<TextAsset>(locationPreffix + "_fui.bytes");
            await handle.ToUniTask();
            _handles.Add(handle);
            var uniHandles = new List<UniTask>(2);
            var pkg = UIPackage.AddPackage((handle.AssetObject as TextAsset).bytes, string.Empty,async (name, extension, type, item) =>
            {
                string path = locationPreffix + "_" + name + extension;
                var subHandle = yoores.LoadAssetAsync(path,type);
                var uniTask = subHandle.ToUniTask();
                uniHandles.Add(uniTask);
                _handles.Add(subHandle);
                await uniTask;
                /*if (type == typeof(Texture))
                {
                    if (item != null && item.texture != null)
                        item.texture.Reload(handle.AssetObject as Texture, null);
                }
                else if (type == typeof(AudioClip))
                {
                    if (item != null && item.audioClip != null)
                        item.audioClip.Reload(handle.AssetObject as AudioClip);
                }
                else
                {
                    Debug.LogError($"UIPackage LoadUIPackageItem:{path} error! resCfg:{name} {extension} {type}");
                }*/
                item.owner.SetItemAsset(item, subHandle.AssetObject, DestroyMethod.None);//注意：这里一定要设置为None
            });
            await UniTask.WhenAll(uniHandles);
            pkg.LoadAllAssets();
            var package = new UIPackageReference(pkg, _handles);
            package.Reference(reference);
            return package;

        }
        public async UniTask InitCommonPackageAsync(List<string> commonPackage)
        {
            if (_persistentPkg.Count > 0)
            {
                return;
            }
            _persistentPkg = commonPackage;
            var handles = new List<UniTask>();
            foreach (var pkg in _persistentPkg)
                handles.Add(GetUIPackageAsync(pkg));
            await UniTask.WhenAll(handles);
        }
        public async UniTask ReLoadAllAsync()
        {
            // 关闭所有UI并获取列表
            _recoverList.Clear();
            foreach (var stack in _layers.Values)
                _recoverList.AddRange(stack?.Clear());

            // 卸载所有UI资源包
            foreach (var pack in _packages)
            {
                UIPackage.RemovePackage(pack.Key);

            }
            _packages.Clear();

            // 恢复UI
            await RecoverUIAsync();
        }

        async UniTask RecoverUIAsync()
        {
            if (_persistentPkg != null && _persistentPkg.Count > 0)
                await InitCommonPackageAsync(_persistentPkg);

            foreach (var ui in _recoverList)
                Open(ui.DialogName, ui.PackageName, ui.ComponentName, ui.Layer, ui.IsFull, ui.IsBlur);

            _recoverList.Clear();
        }
    }
}
