
using Cysharp.Threading.Tasks;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;

//HybridCLRData的AOTGenericReferences若有变化，很大可能要更新包
public class AvoidStripped : MonoBehaviour
{
    enum EnumType
    {
        None
    }
    struct StructType
    {
    }
	public void GenericType()
	{
        var tp21 = typeof( Cysharp.Threading.Tasks.ITaskPoolNode<object>);
        var tp22 = typeof( Cysharp.Threading.Tasks.TaskPool<>);
        var tp23 = typeof( System.Func<int>);
        var tp25 = typeof( System.Func<object>);
	}
public void RefMethods<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
    {
        awaiter = default;
        stateMachine = default;
        Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder op = default;
        op.AwaitUnsafeOnCompleted(ref awaiter,ref stateMachine);
        op.Start(ref stateMachine);
        YooAsset.AllAssetsHandle a = YooAssets.LoadAllAssetsAsync<Object>("", 0);

    }
}
