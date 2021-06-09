using ILRuntime.Runtime.CLRBinding;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XIHBasic
{
    public static class HotFixBridge
    {
        public static Action Update { get; set; } = () => { };
        public static Action FixedUpdate { get; set; } = () => { };
        public static Action<bool> OnApplicationFocus { get; set; } = (_) => { };
        public static Action<bool> OnApplicationPause { get; set; } = (_) => { };

        public static ILRuntime.Runtime.Enviorment.AppDomain Appdomain { get; }
        static MemoryStream fs;
        static MemoryStream p;
        static HotFixBridge()
        {
            Appdomain = new ILRuntime.Runtime.Enviorment.AppDomain();
        }
        public static bool Start(byte[] dll, byte[] pdb)
        {
            fs = new MemoryStream(dll);
            try
            {
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
                if (pdb == null)
                {
                    Appdomain.LoadAssembly(fs);
                }
                else
                {
                    p = new MemoryStream(pdb);
                    Appdomain.LoadAssembly(fs, p, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
                    Appdomain.UnityMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
                    Appdomain.DebugService.StartDebugService(56000);
                }
#else
                Appdomain.LoadAssembly(fs);
#endif
            }
            catch (Exception e)
            {
                Debug.LogError($"加载热更DLL失败\r\n{e}");
                return false;
            }
            //在CLR绑定代码生成之后，需要将这些绑定代码注册到AppDomain中才能使CLR绑定生效，
            //但是一定要记得将CLR绑定的注册写在CLR重定向的注册后面，因为同一个方法只能被重定向一次，只有先注册的那个才能生效
            InitILRuntime(Appdomain);//重定向和跨域绑定
            CLRBindingUtils.Initialize(Appdomain);//CLR绑定的注册
            Appdomain.Invoke("XIHHotFix.HotFixInit", "Init", null);
            return true;
        }
        public static void InitILRuntime(ILRuntime.Runtime.Enviorment.AppDomain domain)
        {
            LitJson.JsonMapper.RegisterILRuntimeCLRRedirection(domain);
            domain.RegisterValueTypeBinder(typeof(Vector3), new Vector3Binder());
            domain.RegisterValueTypeBinder(typeof(Quaternion), new QuaternionBinder());
            domain.RegisterValueTypeBinder(typeof(Vector2), new Vector2Binder());
            //这里需要注册所有热更DLL中用到的跨域继承Adapter，否则无法正确抓取引用
            domain.RegisterCrossBindingAdaptor(new IAsyncStateMachineAdapter());
            domain.DelegateManager.RegisterMethodDelegate<System.Boolean>();
            domain.DelegateManager.RegisterMethodDelegate<Collision2D>();
            domain.DelegateManager.RegisterMethodDelegate<Collider2D>();
            domain.DelegateManager.RegisterMethodDelegate<Collision>();
            domain.DelegateManager.RegisterMethodDelegate<Collider>();
            domain.DelegateManager.RegisterMethodDelegate<BaseEventData>();
            domain.DelegateManager.RegisterMethodDelegate<PointerEventData>();
            domain.DelegateManager.RegisterMethodDelegate<AxisEventData>();
            domain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction>((act) =>
            {
                return new UnityEngine.Events.UnityAction(() =>
                {
                    ((Action)act)();
                });
            });
            domain.DelegateManager.RegisterFunctionDelegate<System.Threading.Tasks.Task>();
            domain.DelegateManager.RegisterFunctionDelegate<UnityEngine.AddressableAssets.ResourceLocators.IResourceLocator, System.String>();
            domain.DelegateManager.RegisterFunctionDelegate<System.Object, System.String>();
        }
        internal static void ShutDown()
        {
            try
            {
                fs?.Close();
                p?.Close();
                fs = null;
                p = null;
            }
            catch
            {
            }
        }
    }
}

