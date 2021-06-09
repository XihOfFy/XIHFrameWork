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
    unsafe class System_Collections_Generic_Dictionary_2_UInt16_TaskCompletionSource_1_ILTypeInstance_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(System.Collections.Generic.Dictionary<System.UInt16, System.Threading.Tasks.TaskCompletionSource<ILRuntime.Runtime.Intepreter.ILTypeInstance>>);
            args = new Type[]{typeof(System.UInt16)};
            method = type.GetMethod("ContainsKey", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ContainsKey_0);
            args = new Type[]{typeof(System.UInt16)};
            method = type.GetMethod("get_Item", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Item_1);
            args = new Type[]{typeof(System.UInt16), typeof(System.Threading.Tasks.TaskCompletionSource<ILRuntime.Runtime.Intepreter.ILTypeInstance>)};
            method = type.GetMethod("set_Item", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_Item_2);
            args = new Type[]{typeof(System.UInt16)};
            method = type.GetMethod("Remove", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Remove_3);
            args = new Type[]{};
            method = type.GetMethod("GetEnumerator", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetEnumerator_4);
            args = new Type[]{};
            method = type.GetMethod("Clear", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Clear_5);

            args = new Type[]{typeof(System.Int32)};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }


        static StackObject* ContainsKey_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.UInt16 @key = (ushort)ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.Dictionary<System.UInt16, System.Threading.Tasks.TaskCompletionSource<ILRuntime.Runtime.Intepreter.ILTypeInstance>> instance_of_this_method = (System.Collections.Generic.Dictionary<System.UInt16, System.Threading.Tasks.TaskCompletionSource<ILRuntime.Runtime.Intepreter.ILTypeInstance>>)typeof(System.Collections.Generic.Dictionary<System.UInt16, System.Threading.Tasks.TaskCompletionSource<ILRuntime.Runtime.Intepreter.ILTypeInstance>>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.ContainsKey(@key);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static StackObject* get_Item_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.UInt16 @key = (ushort)ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.Dictionary<System.UInt16, System.Threading.Tasks.TaskCompletionSource<ILRuntime.Runtime.Intepreter.ILTypeInstance>> instance_of_this_method = (System.Collections.Generic.Dictionary<System.UInt16, System.Threading.Tasks.TaskCompletionSource<ILRuntime.Runtime.Intepreter.ILTypeInstance>>)typeof(System.Collections.Generic.Dictionary<System.UInt16, System.Threading.Tasks.TaskCompletionSource<ILRuntime.Runtime.Intepreter.ILTypeInstance>>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method[key];

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* set_Item_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Threading.Tasks.TaskCompletionSource<ILRuntime.Runtime.Intepreter.ILTypeInstance> @value = (System.Threading.Tasks.TaskCompletionSource<ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Threading.Tasks.TaskCompletionSource<ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.UInt16 @key = (ushort)ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Collections.Generic.Dictionary<System.UInt16, System.Threading.Tasks.TaskCompletionSource<ILRuntime.Runtime.Intepreter.ILTypeInstance>> instance_of_this_method = (System.Collections.Generic.Dictionary<System.UInt16, System.Threading.Tasks.TaskCompletionSource<ILRuntime.Runtime.Intepreter.ILTypeInstance>>)typeof(System.Collections.Generic.Dictionary<System.UInt16, System.Threading.Tasks.TaskCompletionSource<ILRuntime.Runtime.Intepreter.ILTypeInstance>>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method[key] = value;

            return __ret;
        }

        static StackObject* Remove_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.UInt16 @key = (ushort)ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.Dictionary<System.UInt16, System.Threading.Tasks.TaskCompletionSource<ILRuntime.Runtime.Intepreter.ILTypeInstance>> instance_of_this_method = (System.Collections.Generic.Dictionary<System.UInt16, System.Threading.Tasks.TaskCompletionSource<ILRuntime.Runtime.Intepreter.ILTypeInstance>>)typeof(System.Collections.Generic.Dictionary<System.UInt16, System.Threading.Tasks.TaskCompletionSource<ILRuntime.Runtime.Intepreter.ILTypeInstance>>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.Remove(@key);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static StackObject* GetEnumerator_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.Generic.Dictionary<System.UInt16, System.Threading.Tasks.TaskCompletionSource<ILRuntime.Runtime.Intepreter.ILTypeInstance>> instance_of_this_method = (System.Collections.Generic.Dictionary<System.UInt16, System.Threading.Tasks.TaskCompletionSource<ILRuntime.Runtime.Intepreter.ILTypeInstance>>)typeof(System.Collections.Generic.Dictionary<System.UInt16, System.Threading.Tasks.TaskCompletionSource<ILRuntime.Runtime.Intepreter.ILTypeInstance>>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GetEnumerator();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* Clear_5(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.Generic.Dictionary<System.UInt16, System.Threading.Tasks.TaskCompletionSource<ILRuntime.Runtime.Intepreter.ILTypeInstance>> instance_of_this_method = (System.Collections.Generic.Dictionary<System.UInt16, System.Threading.Tasks.TaskCompletionSource<ILRuntime.Runtime.Intepreter.ILTypeInstance>>)typeof(System.Collections.Generic.Dictionary<System.UInt16, System.Threading.Tasks.TaskCompletionSource<ILRuntime.Runtime.Intepreter.ILTypeInstance>>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Clear();

            return __ret;
        }


        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 @capacity = ptr_of_this_method->Value;


            var result_of_this_method = new System.Collections.Generic.Dictionary<System.UInt16, System.Threading.Tasks.TaskCompletionSource<ILRuntime.Runtime.Intepreter.ILTypeInstance>>(@capacity);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


    }
}
