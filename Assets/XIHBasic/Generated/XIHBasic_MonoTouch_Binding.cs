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
    unsafe class XIHBasic_MonoTouch_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(XIHBasic.MonoTouch);

            field = type.GetField("onBeginDrag", flag);
            app.RegisterCLRFieldGetter(field, get_onBeginDrag_0);
            app.RegisterCLRFieldSetter(field, set_onBeginDrag_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_onBeginDrag_0, AssignFromStack_onBeginDrag_0);
            field = type.GetField("onDrag", flag);
            app.RegisterCLRFieldGetter(field, get_onDrag_1);
            app.RegisterCLRFieldSetter(field, set_onDrag_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_onDrag_1, AssignFromStack_onDrag_1);
            field = type.GetField("onEndDrag", flag);
            app.RegisterCLRFieldGetter(field, get_onEndDrag_2);
            app.RegisterCLRFieldSetter(field, set_onEndDrag_2);
            app.RegisterCLRFieldBinding(field, CopyToStack_onEndDrag_2, AssignFromStack_onEndDrag_2);


        }



        static object get_onBeginDrag_0(ref object o)
        {
            return ((XIHBasic.MonoTouch)o).onBeginDrag;
        }

        static StackObject* CopyToStack_onBeginDrag_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((XIHBasic.MonoTouch)o).onBeginDrag;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_onBeginDrag_0(ref object o, object v)
        {
            ((XIHBasic.MonoTouch)o).onBeginDrag = (System.Action<UnityEngine.EventSystems.PointerEventData>)v;
        }

        static StackObject* AssignFromStack_onBeginDrag_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<UnityEngine.EventSystems.PointerEventData> @onBeginDrag = (System.Action<UnityEngine.EventSystems.PointerEventData>)typeof(System.Action<UnityEngine.EventSystems.PointerEventData>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((XIHBasic.MonoTouch)o).onBeginDrag = @onBeginDrag;
            return ptr_of_this_method;
        }

        static object get_onDrag_1(ref object o)
        {
            return ((XIHBasic.MonoTouch)o).onDrag;
        }

        static StackObject* CopyToStack_onDrag_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((XIHBasic.MonoTouch)o).onDrag;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_onDrag_1(ref object o, object v)
        {
            ((XIHBasic.MonoTouch)o).onDrag = (System.Action<UnityEngine.EventSystems.PointerEventData>)v;
        }

        static StackObject* AssignFromStack_onDrag_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<UnityEngine.EventSystems.PointerEventData> @onDrag = (System.Action<UnityEngine.EventSystems.PointerEventData>)typeof(System.Action<UnityEngine.EventSystems.PointerEventData>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((XIHBasic.MonoTouch)o).onDrag = @onDrag;
            return ptr_of_this_method;
        }

        static object get_onEndDrag_2(ref object o)
        {
            return ((XIHBasic.MonoTouch)o).onEndDrag;
        }

        static StackObject* CopyToStack_onEndDrag_2(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((XIHBasic.MonoTouch)o).onEndDrag;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_onEndDrag_2(ref object o, object v)
        {
            ((XIHBasic.MonoTouch)o).onEndDrag = (System.Action<UnityEngine.EventSystems.PointerEventData>)v;
        }

        static StackObject* AssignFromStack_onEndDrag_2(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<UnityEngine.EventSystems.PointerEventData> @onEndDrag = (System.Action<UnityEngine.EventSystems.PointerEventData>)typeof(System.Action<UnityEngine.EventSystems.PointerEventData>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((XIHBasic.MonoTouch)o).onEndDrag = @onEndDrag;
            return ptr_of_this_method;
        }



    }
}
