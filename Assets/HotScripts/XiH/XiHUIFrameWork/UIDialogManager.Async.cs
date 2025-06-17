using Cysharp.Threading.Tasks;
using FairyGUI;
using System;
using System.Collections.Generic;
using UnityEngine;
using Aot.XiHUtil;
using Tmpl;
using Object = UnityEngine.Object;

namespace XiHUI
{
    /// <summary>
    /// UI窗口管理器
    /// </summary>
    public partial class UIDialogManager
    {
        public async UniTask<UIDialog> OpenAsync(UIParam param)
        {
            if (!_dialogType.TryGetValue(param.DialogName, out var type))
                return null;

            if (!_layers.TryGetValue(param.Layer, out var stack))
                return null;

            var dialog = stack.Get(param.DialogName);
            if (dialog != null && dialog.State == State.Loading)
            { 
                await UniTask.WaitUntil(()=>dialog.State != State.Loading);
                if (dialog.State == State.Close)
                {
                    dialog = null;
                }
                else {
                    return dialog;
                }
            }

            if (dialog != null)
            {
                stack.Push(dialog);
                return dialog;
            }
            else
            {
                dialog = Activator.CreateInstance(type) as UIDialog;
                dialog.SetOpenParams(param);
                stack.Push(dialog);
                return await CreateDialogAsync(dialog, stack, param);
            }
        }

        private async UniTask<UIDialog> CreateDialogAsync(UIDialog dialog, UIStack stack, UIParam param)
        {
            if (param.DependencyPackages.Count > 0) {
                var tks = new List<UniTask>();
                foreach (var dependency in param.DependencyPackages)
                {
                    tks.Add(GetDependenceUIPackageAsync(dependency, dialog));
                }
                await UniTask.WhenAll(tks);
            }

            var compent = await LoadUIComponentAsync(param.PackageName, param.ComponentName, dialog);

            if (compent == null)
            {
                stack.Pop(dialog);
                return null;
            }

            UIControlBinding.BindFields(dialog, compent);
            dialog.Open(compent, param.IsFull, param.IsBlur,param.UseBatch);
            stack.Push(dialog);
            dialog.Open();

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
        public UniTask GetDependenceUIPackageAsync(string packageName, UIDialog reference)
        {
            return GetUIPackageAsync(packageName, reference);
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
            var _handles = new List<AssetRef>(64);
            string locationPreffix = "Assets/Res/FairyRes/" + packageName + "/" + packageName;
            var handle = AssetLoadUtil.LoadAssetAsync<TextAsset>(locationPreffix + "_fui.bytes");
            await handle.ToUniTask();
            _handles.Add(handle);
            //var uniHandles = new List<UniTask>(2);
            var pkg = UIPackage.AddPackage((handle.GetAsset<TextAsset>()).bytes, string.Empty,async (name, extension, type, item) =>
            {
                string path = locationPreffix + "_" + name + extension;
                var subHandle = AssetLoadUtil.LoadAssetAsync(path,type);
                var uniTask = subHandle.ToUniTask();
                //uniHandles.Add(uniTask);
                _handles.Add(subHandle);
                await uniTask;
                item.owner.SetItemAsset(item, subHandle.GetAsset<Object>(), DestroyMethod.None);//注意：这里一定要设置为None
            });
            //await UniTask.WhenAll(uniHandles);
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
                Open(ui);

            _recoverList.Clear();
        }
    }
}
