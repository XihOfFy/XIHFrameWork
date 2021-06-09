using System;
using System.Collections.Generic;
using System.Reflection;

namespace ILRuntime.Runtime.Generated
{
    class CLRBindings
    {

//will auto register in unity
#if UNITY_5_3_OR_NEWER
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        static private void RegisterBindingAction()
        {
            ILRuntime.Runtime.CLRBinding.CLRBindingUtils.RegisterBindingAction(Initialize);
        }

        internal static ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector3> s_UnityEngine_Vector3_Binding_Binder = null;
        internal static ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Quaternion> s_UnityEngine_Quaternion_Binding_Binder = null;
        internal static ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector2> s_UnityEngine_Vector2_Binding_Binder = null;

        /// <summary>
        /// Initialize the CLR binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
        public static void Initialize(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            System_Collections_Generic_LinkedList_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_LinkedListNode_1_ILTypeInstance_Binding.Register(app);
            System_Buffer_Binding.Register(app);
            System_Object_Binding.Register(app);
            System_Diagnostics_Debug_Binding.Register(app);
            System_UInt32_Binding.Register(app);
            System_Array_Binding.Register(app);
            System_Byte_Binding.Register(app);
            System_BitConverter_Binding.Register(app);
            System_Runtime_CompilerServices_AsyncTaskMethodBuilder_1_Boolean_Binding.Register(app);
            System_Runtime_CompilerServices_AsyncVoidMethodBuilder_Binding.Register(app);
            System_Threading_Monitor_Binding.Register(app);
            System_Collections_Generic_Queue_1_Byte_Array_Binding.Register(app);
            System_Action_1_Byte_Array_Binding.Register(app);
            System_Net_Sockets_UdpClient_Binding.Register(app);
            System_Exception_Binding.Register(app);
            XiHNet_Debugger_Binding.Register(app);
            System_Threading_CancellationTokenSource_Binding.Register(app);
            System_Collections_Generic_Queue_1_Exception_Binding.Register(app);
            System_Action_Binding.Register(app);
            System_Net_Sockets_Socket_Binding.Register(app);
            System_Runtime_CompilerServices_AsyncTaskMethodBuilder_Binding.Register(app);
            System_Threading_Tasks_Task_Binding.Register(app);
            System_Threading_Tasks_TaskFactory_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_Binding.Register(app);
            System_Threading_Tasks_Task_1_Int32_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_1_Int32_Binding.Register(app);
            System_Threading_Tasks_Task_1_Task_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_1_Task_Binding.Register(app);
            System_Threading_Tasks_Task_1_UdpReceiveResult_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_1_UdpReceiveResult_Binding.Register(app);
            System_Net_Sockets_UdpReceiveResult_Binding.Register(app);
            System_DateTime_Binding.Register(app);
            System_TimeSpan_Binding.Register(app);
            System_Convert_Binding.Register(app);
            System_IO_MemoryStream_Binding.Register(app);
            System_IO_BinaryReader_Binding.Register(app);
            XiHNet_AesCryptor_Binding.Register(app);
            XiHNet_XorCryptor_Binding.Register(app);
            XiHNet_NoneCryptor_Binding.Register(app);
            System_String_Binding.Register(app);
            System_IDisposable_Binding.Register(app);
            System_Collections_Generic_Queue_1_Action_Binding.Register(app);
            System_Runtime_CompilerServices_AsyncTaskMethodBuilder_1_ILTypeInstance_Binding.Register(app);
            XiHNet_NetConfig_Binding.Register(app);
            System_ValueTuple_4_Boolean_Byte_Array_UInt16_Byte_Array_Binding.Register(app);
            XiHNet_ICryptor_Binding.Register(app);
            System_Threading_Tasks_TaskCompletionSource_1_Boolean_Binding.Register(app);
            System_Threading_Tasks_Task_1_Boolean_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_1_Boolean_Binding.Register(app);
            System_ValueTuple_2_Byte_Array_TaskCompletionSource_1_ILTypeInstance_Binding.Register(app);
            System_Threading_Tasks_TaskCompletionSource_1_ILTypeInstance_Binding.Register(app);
            System_Threading_Tasks_Task_1_ILTypeInstance_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_1_ILTypeInstance_Binding.Register(app);
            System_DateTimeOffset_Binding.Register(app);
            System_Timers_Timer_Binding.Register(app);
            XiHNet_XiHTimer_Binding.Register(app);
            System_Net_Sockets_TcpClient_Binding.Register(app);
            System_Net_IPEndPoint_Binding.Register(app);
            System_Net_EndPoint_Binding.Register(app);
            System_Net_Sockets_LingerOption_Binding.Register(app);
            System_IO_Stream_Binding.Register(app);
            System_Collections_Generic_HashSet_1_UInt16_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_UInt16_Type_Binding.Register(app);
            System_TypeAccessException_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_UInt16_Binding.Register(app);
            System_Type_Binding.Register(app);
            System_Reflection_Assembly_Binding.Register(app);
            System_Reflection_MemberInfo_Binding.Register(app);
            ProtoBuf_Serializer_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_UInt16_TaskCompletionSource_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_UInt16_Action_1_Byte_Array_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_UInt16_TaskCompletionSource_1_ILTypeInstance_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_KeyValuePair_2_UInt16_TaskCompletionSource_1_ILTypeInstance_Binding.Register(app);
            System_Threading_Interlocked_Binding.Register(app);
            System_Action_1_Boolean_Binding.Register(app);
            XIHBasic_HotFixBridge_Binding.Register(app);
            UnityEngine_Debug_Binding.Register(app);
            UnityEngine_AddressableAssets_Addressables_Binding.Register(app);
            UnityEngine_ResourceManagement_AsyncOperations_AsyncOperationHandle_1_SceneInstance_Binding.Register(app);
            System_Threading_Tasks_Task_1_SceneInstance_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_1_SceneInstance_Binding.Register(app);
            UnityEngine_Caching_Binding.Register(app);
            UnityEngine_Object_Binding.Register(app);
            UnityEngine_Rigidbody_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_Vector3_Binding.Register(app);
            UnityEngine_Vector3_Binding.Register(app);
            UnityEngine_Transform_Binding.Register(app);
            UnityEngine_EventSystems_PointerEventData_Binding.Register(app);
            UnityEngine_Vector2_Binding.Register(app);
            UnityEngine_RectTransform_Binding.Register(app);
            System_Math_Binding.Register(app);
            XIHBasic_MonoTouch_Binding.Register(app);
            XIHBasic_MonoDotBase_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_GameObject_Binding.Register(app);
            UnityEngine_GameObject_Binding.Register(app);
            UnityEngine_Time_Binding.Register(app);
            UnityEngine_UI_Button_Binding.Register(app);
            UnityEngine_Events_UnityEvent_Binding.Register(app);
            UnityEngine_TextMesh_Binding.Register(app);
            UnityEngine_Collider_Binding.Register(app);
            UnityEngine_SceneManagement_SceneManager_Binding.Register(app);
            UnityEngine_SceneManagement_Scene_Binding.Register(app);
            UnityEngine_UI_Text_Binding.Register(app);
            XIHBasic_PlatformConfig_Binding.Register(app);
            System_IO_File_Binding.Register(app);
            LitJson_JsonMapper_Binding.Register(app);
            LitJson_JsonData_Binding.Register(app);
            UnityEngine_UI_Scrollbar_Binding.Register(app);
            UnityEngine_Mathf_Binding.Register(app);
            UnityEngine_ResourceManagement_ResourceLocations_IResourceLocation_Binding.Register(app);
            UnityEngine_ResourceManagement_ResourceProviders_AssetBundleRequestOptions_Binding.Register(app);
            UnityEngine_Hash128_Binding.Register(app);
            UnityEngine_CachedAssetBundle_Binding.Register(app);
            UnityEngine_Application_Binding.Register(app);
            UnityEngine_Events_UnityEventBase_Binding.Register(app);
            System_Net_WebRequest_Binding.Register(app);
            System_Threading_Tasks_Task_1_WebResponse_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_1_WebResponse_Binding.Register(app);
            System_Net_WebResponse_Binding.Register(app);
            System_IO_StreamReader_Binding.Register(app);
            System_IO_TextReader_Binding.Register(app);
            System_IO_FileStream_Binding.Register(app);
            UnityEngine_ResourceManagement_AsyncOperations_AsyncOperationHandle_1_TextAsset_Binding.Register(app);
            System_Threading_Tasks_Task_1_TextAsset_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_1_TextAsset_Binding.Register(app);
            UnityEngine_ResourceManagement_AsyncOperations_AsyncOperationHandle_1_List_1_IResourceLocator_Binding.Register(app);
            System_Threading_Tasks_Task_1_List_1_IResourceLocator_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_1_List_1_IResourceLocator_Binding.Register(app);
            System_Collections_Generic_HashSet_1_IResourceLocation_Binding.Register(app);
            System_Collections_Generic_IEnumerable_1_IResourceLocator_Binding.Register(app);
            System_Collections_Generic_IEnumerator_1_IResourceLocator_Binding.Register(app);
            UnityEngine_AddressableAssets_ResourceLocators_IResourceLocator_Binding.Register(app);
            UnityEngine_ResourceManagement_AsyncOperations_AsyncOperationHandle_1_IList_1_IResourceLocation_Binding.Register(app);
            System_Threading_Tasks_Task_1_IList_1_IResourceLocation_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_1_IList_1_IResourceLocation_Binding.Register(app);
            System_Collections_Generic_IEnumerable_1_IResourceLocation_Binding.Register(app);
            System_Collections_Generic_IEnumerator_1_IResourceLocation_Binding.Register(app);
            System_Collections_IEnumerator_Binding.Register(app);
            System_Collections_Generic_List_1_IResourceLocation_Binding.Register(app);
            UnityEngine_ResourceManagement_AsyncOperations_AsyncOperationHandle_Binding.Register(app);
            System_Threading_Tasks_Task_1_Object_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_1_Object_Binding.Register(app);

            ILRuntime.CLR.TypeSystem.CLRType __clrType = null;
            __clrType = (ILRuntime.CLR.TypeSystem.CLRType)app.GetType (typeof(UnityEngine.Vector3));
            s_UnityEngine_Vector3_Binding_Binder = __clrType.ValueTypeBinder as ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector3>;
            __clrType = (ILRuntime.CLR.TypeSystem.CLRType)app.GetType (typeof(UnityEngine.Quaternion));
            s_UnityEngine_Quaternion_Binding_Binder = __clrType.ValueTypeBinder as ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Quaternion>;
            __clrType = (ILRuntime.CLR.TypeSystem.CLRType)app.GetType (typeof(UnityEngine.Vector2));
            s_UnityEngine_Vector2_Binding_Binder = __clrType.ValueTypeBinder as ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector2>;
        }

        /// <summary>
        /// Release the CLR binding, please invoke this BEFORE ILRuntime Appdomain destroy
        /// </summary>
        public static void Shutdown(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            s_UnityEngine_Vector3_Binding_Binder = null;
            s_UnityEngine_Quaternion_Binding_Binder = null;
            s_UnityEngine_Vector2_Binding_Binder = null;
        }
    }
}
