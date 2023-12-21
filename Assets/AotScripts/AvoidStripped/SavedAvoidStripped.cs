
using Cysharp.Threading.Tasks;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using YooAsset;
using static YooAsset.DownloaderOperation;
using Object = UnityEngine.Object;

//HybridCLRData的AOTGenericReferences若有变化，很大可能要更新包
public class SavedAvoidStripped : MonoBehaviour
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
        var tp1 = typeof( Cysharp.Threading.Tasks.ITaskPoolNode<object>);
        var tp2 = typeof( Cysharp.Threading.Tasks.TaskPool<>);
        var tp3 = typeof( System.Func<int>);
        var tp5 = typeof( System.Func<object>);
        var tp7 = typeof(System.Action<int>);
        var tp8 = typeof(System.Action<object>);
        var tp9 = typeof(System.Collections.Generic.Comparer<byte>);
        var tp20 = typeof(System.Collections.Generic.Comparer<object>);
        var tp21 = typeof(System.Collections.Generic.Comparer<ushort>);
        var tp22 = typeof(System.Collections.Generic.EqualityComparer<byte>);
        var tp23 = typeof(System.Collections.Generic.EqualityComparer<object>);
        var tp24 = typeof(System.Collections.Generic.EqualityComparer<ushort>);
        var tp25 = typeof(System.Collections.Generic.IEnumerable<object>);
        var tp26 = typeof(System.Collections.Generic.IEnumerator<object>);
        var tp27 = typeof(System.Collections.Generic.LinkedList<object>.Enumerator);
        var tp28 = typeof(System.Collections.Generic.LinkedList<object>);
        var tp29 = typeof(System.Collections.Generic.LinkedListNode<object>);
        var tp36 = typeof(System.Nullable<double>);
        var tp37 = typeof(System.Nullable<long>);
        var tp38 = typeof(System.ValueTuple<byte, object, ushort, object>);
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
        this.StartCoroutine(nameof(RefMethods));

        var aa = System.Array.Empty<object>();

    }
}
