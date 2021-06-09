using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Generated
{
    unsafe class UnityEngine_AddressableAssets_Addressables_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(UnityEngine.AddressableAssets.Addressables);
            args = new Type[]{typeof(System.Object), typeof(UnityEngine.SceneManagement.LoadSceneMode), typeof(System.Boolean), typeof(System.Int32)};
            method = type.GetMethod("LoadSceneAsync", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LoadSceneAsync_0);
            Dictionary<string, List<MethodInfo>> genericMethods = new Dictionary<string, List<MethodInfo>>();
            List<MethodInfo> lst = null;                    
            foreach(var m in type.GetMethods())
            {
                if(m.IsGenericMethodDefinition)
                {
                    if (!genericMethods.TryGetValue(m.Name, out lst))
                    {
                        lst = new List<MethodInfo>();
                        genericMethods[m.Name] = lst;
                    }
                    lst.Add(m);
                }
            }
            args = new Type[]{typeof(UnityEngine.TextAsset)};
            if (genericMethods.TryGetValue("LoadAssetAsync", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.TextAsset>), typeof(System.Object)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, LoadAssetAsync_1);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(UnityEngine.TextAsset)};
            if (genericMethods.TryGetValue("Release", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.TextAsset>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Release_2);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Collections.Generic.IEnumerable<System.String>), typeof(System.Boolean)};
            method = type.GetMethod("UpdateCatalogs", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, UpdateCatalogs_3);
            args = new Type[]{};
            method = type.GetMethod("get_ResourceLocators", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_ResourceLocators_4);
            args = new Type[]{typeof(System.Collections.IEnumerable), typeof(UnityEngine.AddressableAssets.Addressables.MergeMode), typeof(System.Type)};
            method = type.GetMethod("LoadResourceLocationsAsync", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LoadResourceLocationsAsync_5);
            args = new Type[]{typeof(System.Collections.Generic.IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation>), typeof(System.Boolean)};
            method = type.GetMethod("DownloadDependenciesAsync", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, DownloadDependenciesAsync_6);
            args = new Type[]{typeof(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle)};
            method = type.GetMethod("Release", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Release_7);


        }


        static StackObject* LoadSceneAsync_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 @priority = ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Boolean @activateOnLoad = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            UnityEngine.SceneManagement.LoadSceneMode @loadMode = (UnityEngine.SceneManagement.LoadSceneMode)typeof(UnityEngine.SceneManagement.LoadSceneMode).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.Object @key = (System.Object)typeof(System.Object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = UnityEngine.AddressableAssets.Addressables.LoadSceneAsync(@key, @loadMode, @activateOnLoad, @priority);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* LoadAssetAsync_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Object @key = (System.Object)typeof(System.Object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<UnityEngine.TextAsset>(@key);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* Release_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.TextAsset> @handle = (UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.TextAsset>)typeof(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.TextAsset>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            UnityEngine.AddressableAssets.Addressables.Release<UnityEngine.TextAsset>(@handle);

            return __ret;
        }

        static StackObject* UpdateCatalogs_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @autoReleaseHandle = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.IEnumerable<System.String> @catalogs = (System.Collections.Generic.IEnumerable<System.String>)typeof(System.Collections.Generic.IEnumerable<System.String>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = UnityEngine.AddressableAssets.Addressables.UpdateCatalogs(@catalogs, @autoReleaseHandle);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* get_ResourceLocators_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            var result_of_this_method = UnityEngine.AddressableAssets.Addressables.ResourceLocators;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* LoadResourceLocationsAsync_5(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Type @type = (System.Type)typeof(System.Type).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            UnityEngine.AddressableAssets.Addressables.MergeMode @mode = (UnityEngine.AddressableAssets.Addressables.MergeMode)typeof(UnityEngine.AddressableAssets.Addressables.MergeMode).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Collections.IEnumerable @keys = (System.Collections.IEnumerable)typeof(System.Collections.IEnumerable).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = UnityEngine.AddressableAssets.Addressables.LoadResourceLocationsAsync(@keys, @mode, @type);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* DownloadDependenciesAsync_6(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @autoReleaseHandle = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation> @locations = (System.Collections.Generic.IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation>)typeof(System.Collections.Generic.IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = UnityEngine.AddressableAssets.Addressables.DownloadDependenciesAsync(@locations, @autoReleaseHandle);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* Release_7(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle @handle = (UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle)typeof(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            UnityEngine.AddressableAssets.Addressables.Release(@handle);

            return __ret;
        }



    }
}
