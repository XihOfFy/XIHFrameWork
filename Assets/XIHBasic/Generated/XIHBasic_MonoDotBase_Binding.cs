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
    unsafe class XIHBasic_MonoDotBase_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(XIHBasic.MonoDotBase);
            args = new Type[]{};
            method = type.GetMethod("get_GameObjsDic", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_GameObjsDic_0);

            field = type.GetField("onEnable", flag);
            app.RegisterCLRFieldGetter(field, get_onEnable_0);
            app.RegisterCLRFieldSetter(field, set_onEnable_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_onEnable_0, AssignFromStack_onEnable_0);
            field = type.GetField("onDisable", flag);
            app.RegisterCLRFieldGetter(field, get_onDisable_1);
            app.RegisterCLRFieldSetter(field, set_onDisable_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_onDisable_1, AssignFromStack_onDisable_1);
            field = type.GetField("onDestory", flag);
            app.RegisterCLRFieldGetter(field, get_onDestory_2);
            app.RegisterCLRFieldSetter(field, set_onDestory_2);
            app.RegisterCLRFieldBinding(field, CopyToStack_onDestory_2, AssignFromStack_onDestory_2);


        }


        static StackObject* get_GameObjsDic_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            XIHBasic.MonoDotBase instance_of_this_method = (XIHBasic.MonoDotBase)typeof(XIHBasic.MonoDotBase).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GameObjsDic;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


        static object get_onEnable_0(ref object o)
        {
            return ((XIHBasic.MonoDotBase)o).onEnable;
        }

        static StackObject* CopyToStack_onEnable_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((XIHBasic.MonoDotBase)o).onEnable;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_onEnable_0(ref object o, object v)
        {
            ((XIHBasic.MonoDotBase)o).onEnable = (System.Action)v;
        }

        static StackObject* AssignFromStack_onEnable_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action @onEnable = (System.Action)typeof(System.Action).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((XIHBasic.MonoDotBase)o).onEnable = @onEnable;
            return ptr_of_this_method;
        }

        static object get_onDisable_1(ref object o)
        {
            return ((XIHBasic.MonoDotBase)o).onDisable;
        }

        static StackObject* CopyToStack_onDisable_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((XIHBasic.MonoDotBase)o).onDisable;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_onDisable_1(ref object o, object v)
        {
            ((XIHBasic.MonoDotBase)o).onDisable = (System.Action)v;
        }

        static StackObject* AssignFromStack_onDisable_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action @onDisable = (System.Action)typeof(System.Action).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((XIHBasic.MonoDotBase)o).onDisable = @onDisable;
            return ptr_of_this_method;
        }

        static object get_onDestory_2(ref object o)
        {
            return ((XIHBasic.MonoDotBase)o).onDestory;
        }

        static StackObject* CopyToStack_onDestory_2(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((XIHBasic.MonoDotBase)o).onDestory;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_onDestory_2(ref object o, object v)
        {
            ((XIHBasic.MonoDotBase)o).onDestory = (System.Action)v;
        }

        static StackObject* AssignFromStack_onDestory_2(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action @onDestory = (System.Action)typeof(System.Action).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((XIHBasic.MonoDotBase)o).onDestory = @onDestory;
            return ptr_of_this_method;
        }



    }
}
