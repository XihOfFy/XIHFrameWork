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
    unsafe class System_Net_Sockets_TcpClient_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(System.Net.Sockets.TcpClient);
            args = new Type[]{};
            method = type.GetMethod("Close", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Close_0);
            args = new Type[]{};
            method = type.GetMethod("Dispose", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Dispose_1);
            args = new Type[]{typeof(System.Net.IPAddress), typeof(System.Int32)};
            method = type.GetMethod("ConnectAsync", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ConnectAsync_2);
            args = new Type[]{typeof(System.Net.Sockets.LingerOption)};
            method = type.GetMethod("set_LingerState", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_LingerState_3);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("set_NoDelay", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_NoDelay_4);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("set_SendBufferSize", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_SendBufferSize_5);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("set_ReceiveBufferSize", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_ReceiveBufferSize_6);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("set_SendTimeout", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_SendTimeout_7);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("set_ReceiveTimeout", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_ReceiveTimeout_8);
            args = new Type[]{};
            method = type.GetMethod("GetStream", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetStream_9);

            args = new Type[]{typeof(System.Net.Sockets.AddressFamily)};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }


        static StackObject* Close_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Net.Sockets.TcpClient instance_of_this_method = (System.Net.Sockets.TcpClient)typeof(System.Net.Sockets.TcpClient).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Close();

            return __ret;
        }

        static StackObject* Dispose_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Net.Sockets.TcpClient instance_of_this_method = (System.Net.Sockets.TcpClient)typeof(System.Net.Sockets.TcpClient).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Dispose();

            return __ret;
        }

        static StackObject* ConnectAsync_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 @port = ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Net.IPAddress @address = (System.Net.IPAddress)typeof(System.Net.IPAddress).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Net.Sockets.TcpClient instance_of_this_method = (System.Net.Sockets.TcpClient)typeof(System.Net.Sockets.TcpClient).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.ConnectAsync(@address, @port);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* set_LingerState_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Net.Sockets.LingerOption @value = (System.Net.Sockets.LingerOption)typeof(System.Net.Sockets.LingerOption).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Net.Sockets.TcpClient instance_of_this_method = (System.Net.Sockets.TcpClient)typeof(System.Net.Sockets.TcpClient).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.LingerState = value;

            return __ret;
        }

        static StackObject* set_NoDelay_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @value = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Net.Sockets.TcpClient instance_of_this_method = (System.Net.Sockets.TcpClient)typeof(System.Net.Sockets.TcpClient).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.NoDelay = value;

            return __ret;
        }

        static StackObject* set_SendBufferSize_5(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 @value = ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Net.Sockets.TcpClient instance_of_this_method = (System.Net.Sockets.TcpClient)typeof(System.Net.Sockets.TcpClient).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.SendBufferSize = value;

            return __ret;
        }

        static StackObject* set_ReceiveBufferSize_6(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 @value = ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Net.Sockets.TcpClient instance_of_this_method = (System.Net.Sockets.TcpClient)typeof(System.Net.Sockets.TcpClient).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.ReceiveBufferSize = value;

            return __ret;
        }

        static StackObject* set_SendTimeout_7(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 @value = ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Net.Sockets.TcpClient instance_of_this_method = (System.Net.Sockets.TcpClient)typeof(System.Net.Sockets.TcpClient).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.SendTimeout = value;

            return __ret;
        }

        static StackObject* set_ReceiveTimeout_8(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 @value = ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Net.Sockets.TcpClient instance_of_this_method = (System.Net.Sockets.TcpClient)typeof(System.Net.Sockets.TcpClient).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.ReceiveTimeout = value;

            return __ret;
        }

        static StackObject* GetStream_9(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Net.Sockets.TcpClient instance_of_this_method = (System.Net.Sockets.TcpClient)typeof(System.Net.Sockets.TcpClient).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GetStream();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Net.Sockets.AddressFamily @family = (System.Net.Sockets.AddressFamily)typeof(System.Net.Sockets.AddressFamily).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = new System.Net.Sockets.TcpClient(@family);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


    }
}
