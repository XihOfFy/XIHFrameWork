﻿using FairyGUI;
using System;
using System.Collections.Generic;
using UnityEngine;
using XiHUtil;
using YooAsset;

namespace XiHUI
{
    public struct DialogOpenParams
    {
        public string DialogName { get; private set; }
        public string PackageName { get; private set; }
        public string ComponentName { get; private set; }
        public Mode Layer { get; private set; }
        public bool IsFull { get; private set; }
        public bool IsBlur { get; private set; }

        public DialogOpenParams(string dialogName, string packageName, string componentName, Mode layer, bool isFull, bool isBlur)
        {
            DialogName = dialogName;
            PackageName = packageName;
            ComponentName = componentName;
            Layer = layer;
            IsFull = isFull;
            IsBlur = isBlur;
        }
    }
    /// <summary>
    /// UI窗口管理器
    /// </summary>
    public partial class UIDialogManager : MonoBehaviour
    {
        private static UIDialogManager instance;
        public static UIDialogManager Instance {
            get {
                if (instance == null) {
                    instance = new GameObject(nameof(UIDialogManager)).AddComponent<UIDialogManager>();
                    DontDestroyOnLoad(instance.gameObject);
                }
                return instance;
            }
        }

        private void Awake()
        {
            InitLayers();
            RegisterAllDialog();
            RefreshCameraRendering += UpdateCameraRendering;
        }

        void Update()
        {
            TickReference(true);
            foreach (var stack in _layers.Values)
                stack?.Poll();
        }
        class UIPackageReference
        {
            public UIPackageReference(UIPackage pkg, List<AssetHandle> _handles)
            {
                _package = pkg;
                _persistent = _persistentPkg.Contains(pkg.name);
                _cacheRemain = _pkgLifeTime;
                handles = _handles;
            }

            private int _useCount;
            private UIPackage _package;
            List<AssetHandle> handles;
            private List<UIDialog> _references = new List<UIDialog>();
            private float _cacheRemain;
            private bool _persistent;

            public UIPackage Reference(UIDialog dialog)
            {
                if (dialog != null && !_references.Contains(dialog))
                {
                    _references.Add(dialog);
                    dialog.OnDispose += Dispose;
                    dialog.OnDispose += _onDialogDispose;
                }

                if (dialog != null)
                    _useCount++;

                return _package;
            }

            private void Dispose(UIDialog dialog)
            {
                _references.Remove(dialog);

                //var t = _references.Count > 0 ? _references[0].dialogName : string.Empty;
                //Ark.Loggin.Log.Info($"[UIPackageReference] {dialog.dialogName} {_references.Count} {t}");

                if (_references.Count == 0)
                    _cacheRemain = _pkgLifeTime + _useCount * _pkgUseAddTime;
            }


            // 释放资源句柄列表
            public void ReleaseHandles()
            {
                foreach (var handle in handles)
                {
                    handle.Release();
                }
                handles.Clear();
            }

            public bool IsReference(bool considerTime)
            {
                if (_persistent || _references.Count > 0)
                    return true;

                if (considerTime)
                {
                    _cacheRemain -= UnityEngine.Time.deltaTime;
                    if (_cacheRemain > 0)
                        return true;
                }

                return false;
            }
        }

        private static List<string> _persistentPkg = new List<string>();
        private static List<string> _noAtlasPkg = new List<string>();
        private static float _pkgLifeTime = 30;
        private static float _pkgUseAddTime = 2;


        private readonly Dictionary<Mode, UIStack> _layers = new Dictionary<Mode, UIStack>();
        private readonly Dictionary<string, UIPackageReference> _packages = new Dictionary<string, UIPackageReference>();
        private readonly Dictionary<string, Type> _dialogType = new Dictionary<string, Type>();
        private readonly List<string> _removeList = new List<string>();
        private static Action<UIDialog> _onDialogDispose;

        public bool CameraRendering { get; private set; } = true;
        public Action RefreshCameraRendering;

        public static void RegisterDialogDispose(Action<UIDialog> action) => _onDialogDispose = action;


        public void InitConfig(List<string> noAtlasPkg, float pkgLifeTime, float pkgUseAddTime)
        {
            _noAtlasPkg = noAtlasPkg;
            _pkgLifeTime = pkgLifeTime;
            _pkgUseAddTime = pkgUseAddTime;
        }

        public void SetBlurParams(float blurBGScale, float blursize)
        {
            foreach (var layer in _layers)
                layer.Value?.SetBlurParams(blurBGScale, blursize);
        }

        public void InitCommonPackage(List<string> commonPackage)
        {
            if (_persistentPkg.Count>0)
            {
                return;
            }
            _persistentPkg = commonPackage;
            foreach (var pkg in _persistentPkg)
                GetUIPackage(pkg);
        }

        private void InitLayers()
        {
            for (var i = Mode.None; i < Mode.Max; i++)
                _layers[i] = new UIStack(i);

            _layers[Mode.Popup]?.SetBase(_layers[Mode.Stack]);
        }

        private void RegisterAllDialog()
        {
            var uidialogType = typeof(UIDialog);
            var fairyType = typeof(GObject);
            var extType = typeof(UIPackageItemExtensionAttribute);

            var types = ReflectUtil.GetAllTypes();
            foreach (var type in types)
            {
                if (!type.IsClass || type.IsAbstract || type.IsNested)
                    continue;
                if (fairyType.IsAssignableFrom(type))
                {
                    var attributes = type.GetCustomAttributes(extType, false);
                    for (int i = 0; i < attributes.Length; i++)
                    {
                        var attribute = (UIPackageItemExtensionAttribute)attributes[i];
                        UIObjectFactory.SetPackageItemExtension(attribute.url, type);
                    }
                }
                else if (uidialogType.IsAssignableFrom(type))
                {
                    _dialogType[type.Name] = type;
                }
            }
        }

        public void TryUnload()
        {
            TickReference(false);
        }


        void TickReference(bool considerTime)
        {
            _removeList.Clear();
            foreach (var key in _packages.Keys)
            {
                var pkg = _packages[key];

                if (pkg == null || !pkg.IsReference(considerTime)) { 
                    _removeList.Add(key);
                    pkg.ReleaseHandles();
                }
            }

            foreach (var key in _removeList)
            {
                UIPackage.RemovePackage(key);

                Debug.Log($"[UIDialogManager] remove {key}");
                _packages.Remove(key);	
            }
        }


        public UIDialog Open(string dialogName,string packageName, string componentName, Mode layer = 0, bool isFull = true, bool isBlur = false)
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
                return CreateDialog(dialogName, packageName, componentName, layer, isFull, isBlur);
            }
        }

        private UIDialog CreateDialog(string dialogName, string packageName, string componentName, Mode layer, bool isFull, bool isBlur)
        {
            if (!_layers.TryGetValue(layer, out var stack))
                return null;

            var dialog = stack.Get(dialogName);
            if (dialog == null || dialog.State != State.Loading)
                return null;

            var compent = LoadUIComponent(packageName, componentName, dialog);

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

        private GComponent LoadUIComponent(string packageName, string componentName, UIDialog reference)
        {
            var package = GetUIPackage(packageName, reference);

            if (package == null)
            {
                return null;
            }

            var obj = package.Reference(null).CreateObject(componentName);
            return obj.asCom;
        }

        private UIPackageReference GetUIPackage(string packageName, UIDialog reference = null)
        {
            if (!_packages.TryGetValue(packageName, out var pkg))
            {
                pkg = LoadUIPackage(packageName, reference);
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
        private UIPackageReference LoadUIPackage(string packageName,  UIDialog reference)
        {
            var _handles = new List<AssetHandle>(100);
            object LoadFunc(string name, string extension, System.Type type, out DestroyMethod method)
            {
                method = DestroyMethod.None; //注意：这里一定要设置为None
                string location = "Assets/Res/FairyRes/" + packageName + "/" + name + extension;
                var package = YooAssets.GetPackage(Aot.AotConfig.PACKAGE_NAME);
                var handle = package.LoadAssetSync(location, type);
                _handles.Add(handle);
                return handle.AssetObject;
            }

            // 执行FairyGUI的添加包函数
            var pkg = UIPackage.AddPackage(packageName, LoadFunc);
            pkg.LoadAllAssets();
            var package = new UIPackageReference(pkg, _handles);
            package.Reference(reference);
            return package;
        }

        public UIDialog GetDialog(string dialogName)
        {
            foreach (var stack in _layers.Values)
            {
                var dialog = stack?.Get(dialogName);
                if (dialog == null)
                    continue;

                return dialog;
            }

            return null;
        }

        public bool IsDialogOpen(string dialogName)
        {
            return GetDialog(dialogName) != null;
        }

        public void Close(UIDialog dialog)
        {
            foreach (var stack in _layers.Values)
                stack?.Pop(dialog);
        }

        public void Close(string name)
        {
            foreach (var stack in _layers.Values)
            {
                var dialog = stack?.Get(name);
                if (dialog == null)
                    continue;

                stack?.Pop(dialog);
            }
        }

        public void Close(string name, Mode layer)
        {
            if (!_layers.TryGetValue(layer, out var stack))
            {
                Close(name);
                return;
            }

            var dialog = stack.Get(name);
            if (dialog == null)
                return;

            stack?.Pop(dialog);
        }

        public void CloseAll(HashSet<string> exceptNames)
        {
            foreach (var stack in _layers.Values)
                stack?.Clear(exceptNames);
        }
        public void CloseAllPopUI(HashSet<string> exceptNames)
        {
            foreach (var kv in _layers)
            {
                if (kv.Key == Mode.Stack) continue;
                kv.Value.Clear(exceptNames);
            }
        }

        private static List<DialogOpenParams> _recoverList = new List<DialogOpenParams>();
        public void ReLoadAll()
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
            RecoverUI();
        }

        void RecoverUI()
        {
            if (_persistentPkg != null && _persistentPkg.Count > 0)
                InitCommonPackage(_persistentPkg);

            foreach (var ui in _recoverList)
                Open(ui.DialogName, ui.PackageName, ui.ComponentName, ui.Layer, ui.IsFull, ui.IsBlur);

            _recoverList.Clear();
        }

        public void GetLayerList(Mode mode, ref IList<UIDialog> result)
        {
            if (result == null)
                result = new List<UIDialog>();
            else
                result.Clear();

            _layers.TryGetValue(mode,out var stack);
            stack?.GetAll(ref result);
        }

        public static bool TEST_CAMERARENDERING;

        private void UpdateCameraRendering()
        {
            var preRendering = CameraRendering;
            CameraRendering = true;
            foreach (var stack in _layers.Values)
            {
                if (stack.HasFullScreenDialog)
                {
                    CameraRendering = false;
                    break;
                }
            }
            if (TEST_CAMERARENDERING)
            {
                CameraRendering = false;
            }

            if (CameraRendering != preRendering)
                SetCameraRendering(CameraRendering);
        }

        internal static void SetCameraRendering(bool enable)
        {
            var cam = GetMainCam();
            if (cam == null)
                return;
            //UI相机不关闭渲染
            if ((cam.cullingMask >> LayerMask.NameToLayer("UI") & 1) == 1)
                return;

            if (enable)
            {
                cam.cullingMask = ~(1 << LayerMask.NameToLayer("UI") | 1 << LayerMask.NameToLayer("Hidden"));
            }
            else
            {
                cam.cullingMask = 0;
            }
        }

        static Camera GetMainCam()
        {
            var cam = Camera.main;
            if (cam == null)
                cam = UnityEngine.Object.FindObjectOfType<Camera>();
            return cam;
        }
    }
    
}