
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using YooAsset;
using static YooAsset.DownloaderOperation;
using Object = UnityEngine.Object;

//HybridCLRData的AOTGenericReferences若有变化，很大可能要更新包
public class SavedAvoidStripped< T> : MonoBehaviour where T : class, ITaskPoolNode<T>
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
        var tp22 = typeof(Cysharp.Threading.Tasks.ITaskPoolNode<object>);
        var tp23 = typeof(Cysharp.Threading.Tasks.TaskPool<T>);
        var tp24 = typeof(System.Action<StructType>);
        var tp25 = typeof(System.Action<byte>);
        var tp26 = typeof(System.Action<int, object>);
        var tp27 = typeof(System.Action<int>);
        var tp28 = typeof(System.Action<object>);
        var tp31 = typeof(System.Collections.Generic.Comparer<StructType>);
        var tp32 = typeof(System.Collections.Generic.Comparer<byte>);
        var tp33 = typeof(System.Collections.Generic.Comparer<object>);
        var tp34 = typeof(System.Collections.Generic.Comparer<ushort>);
        var tp35 = typeof(System.Collections.Generic.Dictionary<int, object>.Enumerator);
        var tp36 = typeof(System.Collections.Generic.Dictionary<object, object>.Enumerator);
        var tp37 = typeof(System.Collections.Generic.Dictionary<int, object>.KeyCollection.Enumerator);
        var tp38 = typeof(System.Collections.Generic.Dictionary<object, object>.KeyCollection.Enumerator);
        var tp39 = typeof(System.Collections.Generic.Dictionary<int, object>.KeyCollection);
        var tp40 = typeof(System.Collections.Generic.Dictionary<object, object>.KeyCollection);
        var tp41 = typeof(System.Collections.Generic.Dictionary<int, object>.ValueCollection.Enumerator);
        var tp42 = typeof(System.Collections.Generic.Dictionary<object, object>.ValueCollection.Enumerator);
        var tp43 = typeof(System.Collections.Generic.Dictionary<int, object>.ValueCollection);
        var tp44 = typeof(System.Collections.Generic.Dictionary<object, object>.ValueCollection);
        var tp45 = typeof(System.Collections.Generic.Dictionary<int, object>);
        var tp46 = typeof(System.Collections.Generic.Dictionary<object, object>);
        var tp47 = typeof(System.Collections.Generic.EqualityComparer<byte>);
        var tp48 = typeof(System.Collections.Generic.EqualityComparer<int>);
        var tp49 = typeof(System.Collections.Generic.EqualityComparer<object>);
        var tp50 = typeof(System.Collections.Generic.EqualityComparer<ushort>);
        var tp51 = typeof(System.Collections.Generic.HashSet<object>.Enumerator);
        var tp52 = typeof(System.Collections.Generic.HashSet<object>);
        var tp54 = typeof(System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int, object>>);
        var tp55 = typeof(System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object, object>>);
        var tp56 = typeof(System.Collections.Generic.ICollection<StructType>);
        var tp57 = typeof(System.Collections.Generic.ICollection<object>);
        var tp58 = typeof(System.Collections.Generic.IComparer<StructType>);
        var tp59 = typeof(System.Collections.Generic.IComparer<object>);
        var tp60 = typeof(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int, object>>);
        var tp61 = typeof(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object, object>>);
        var tp62 = typeof(System.Collections.Generic.IEnumerable<StructType>);
        var tp63 = typeof(System.Collections.Generic.IEnumerable<object>);
        var tp64 = typeof(System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int, object>>);
        var tp65 = typeof(System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object, object>>);
        var tp66 = typeof(System.Collections.Generic.IEnumerator<StructType>);
        var tp67 = typeof(System.Collections.Generic.IEnumerator<object>);
        var tp68 = typeof(System.Collections.Generic.IEqualityComparer<int>);
        var tp69 = typeof(System.Collections.Generic.IEqualityComparer<object>);
        var tp70 = typeof(System.Collections.Generic.IList<StructType>);
        var tp71 = typeof(System.Collections.Generic.IList<object>);
        var tp72 = typeof(System.Collections.Generic.KeyValuePair<int, object>);
        var tp73 = typeof(System.Collections.Generic.KeyValuePair<object, object>);
        var tp74 = typeof(System.Collections.Generic.LinkedList<object>.Enumerator);
        var tp75 = typeof(System.Collections.Generic.LinkedList<object>);
        var tp76 = typeof(System.Collections.Generic.LinkedListNode<object>);
        var tp77 = typeof(System.Collections.Generic.List<StructType>.Enumerator);
        var tp78 = typeof(System.Collections.Generic.List<object>.Enumerator);
        var tp79 = typeof(System.Collections.Generic.List<StructType>);
        var tp80 = typeof(System.Collections.Generic.List<object>);
        var tp89 = typeof(System.Collections.ObjectModel.ReadOnlyCollection<StructType>);
        var tp90 = typeof(System.Collections.ObjectModel.ReadOnlyCollection<object>);
        var tp91 = typeof(System.Comparison<StructType>);
        var tp92 = typeof(System.Comparison<object>);
        var tp93 = typeof(System.Func<int>);
        var tp94 = typeof(System.Func<object, byte>);
        var tp95 = typeof(System.Func<object, object>);
        var tp101 = typeof(System.Nullable<double>);
        var tp102 = typeof(System.Nullable<long>);
        var tp103 = typeof(System.Predicate<StructType>);
        var tp104 = typeof(System.Predicate<object>);
        var tp105 = typeof(System.Runtime.CompilerServices.ConditionalWeakTable<object, object>.CreateValueCallback);
        var tp107 = typeof(System.Runtime.CompilerServices.ConditionalWeakTable<object, object>);
        var tp108 = typeof(System.ValueTuple<byte, object, ushort, object>);
    }
    public void RefMethods<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : ICriticalNotifyCompletion
                where TStateMachine : IAsyncStateMachine
    {
        awaiter = default;
        stateMachine = default;
        Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder op = default;
        op.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
        op.Start(ref stateMachine);
        YooAsset.AllAssetsHandle a = YooAssets.LoadAllAssetsAsync<Object>("", 0);
        this.StartCoroutine(nameof(RefMethods));

        var aa = System.Array.Empty<object>();

        aa.Select(a => a.ToString());
        var ls = new List<string>();
        ls.SelectMany(l => l).Where(a => true);
        var obj = new GameObject().AddComponent<Camera>();
        UnityEngine.Object.FindObjectOfType<Camera>();
        YooAssets.GetPackage("").LoadAllAssetsAsync<Object>("", 1);
        YooAssets.GetPackage("").LoadAssetSync<Object>("");

    }
}
