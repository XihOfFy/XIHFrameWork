#if UNITY_WEBGL && !UNITY_EDITOR
  #define TT_UDP_ENABLED
#endif

using System;
using UnityEngine;

#if TT_UDP_ENABLED
using System.Runtime.InteropServices;
using AOT;
#endif

namespace TTSDK.Network
{
    public class TTUDPSocketHandler
    {

#if UNITY_EDITOR
            public const string NotImplMessage = "TT UDP Socket not supported on Unity Editor. Test it with Douyin App.";
#elif UNITY_ANDROID
            public const string NotImplMessage = "TT UDP Socket not supported on Android Native.";
#else
            public const string NotImplMessage = "TT UDP Socket not supported on current platform.";
#endif
            
        public delegate void StarkUDPSocketOnMessageCallback(string instanceId, IntPtr ptrMsg, int lenMsg,
          IntPtr ptrLocalInfo, IntPtr ptrRemoteInfo);
        public static string StarkCreateUDPSocket()
        {
            throw new NotSupportedException(NotImplMessage);
        }
        
        public static void StarkUDPSocketClose(string id)
        {
            throw new NotSupportedException(NotImplMessage);
        }
        
        public static void StarkUDPSocketConnect(string id, string option)
        {
            throw new NotSupportedException(NotImplMessage);
        }

        public static void StarkUDPSocketOnClose(string id)
        {
            throw new NotSupportedException(NotImplMessage);
        }

        public static void StarkUDPSocketOffClose(string id)
        {
            throw new NotSupportedException(NotImplMessage);
        }
        
        public static void StarkUDPSocketOnError(string id)
        {
            throw new NotSupportedException(NotImplMessage);
        }

        public static void StarkUDPSocketOffError(string id)
        {
            throw new NotSupportedException(NotImplMessage);
        }

        public static void StarkUDPSocketOnListening(string id)
        {
            throw new NotSupportedException(NotImplMessage);
        }

        public static void StarkUDPSocketOffListening(string id)
        {
            throw new NotSupportedException(NotImplMessage);
        }

        public static void StarkUDPSocketOnMessage(string id, bool needInfo)
        {
            throw new NotSupportedException(NotImplMessage);
        }
        
        public static void StarkUDPSocketOffMessage(string id)
        {
            throw new NotSupportedException(NotImplMessage);
        }

        public static void StarkUDPSocketSend(string id, string data, string param)
        {
            throw new NotSupportedException(NotImplMessage);
        }
        
        public static void StarkUDPSocketSend(string id, byte[] data, int dataLength, string param)
        {
            throw new NotSupportedException(NotImplMessage);
        }
        
        public static void StarkUDPSocketSetTTL(string id, double ttl)
        {
            throw new NotSupportedException(NotImplMessage);
        }

        public static int StarkUDPSocketBind(string id, string param)
        {
            throw new NotSupportedException(NotImplMessage);
        }
        
        public static void StarkUDPSocketDestroy(string id)
        {
            throw new NotSupportedException(NotImplMessage);
        }

        public static void StarkRegisterUDPSocketOnMessageCallback(StarkUDPSocketOnMessageCallback callback)
        {
            throw new NotSupportedException(NotImplMessage);
        }

#if TT_UDP_ENABLED
        [MonoPInvokeCallback(typeof(StarkUDPSocketOnMessageCallback))]
#endif
        public static void _UDPSocketOnMessageCallback(string instanceId, IntPtr ptrMsg, int lenMsg,
            IntPtr ptrLocalInfo, IntPtr ptrRemoteInfo)
        {
          _messageCallback.Invoke(instanceId, ptrMsg, lenMsg, ptrLocalInfo, ptrRemoteInfo);
        }

        private static Action<string, IntPtr, int, IntPtr, IntPtr> _messageCallback;
        public static void SetTTUDPSocketMessageCallback(Action<string, IntPtr, int, IntPtr, IntPtr> callback)
        {
            _messageCallback = callback;
            StarkRegisterUDPSocketOnMessageCallback(_UDPSocketOnMessageCallback);
        }
// #endif
    }
}