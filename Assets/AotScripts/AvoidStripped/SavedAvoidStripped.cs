
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
        var tp40 = typeof(Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>);
        var tp48 = typeof(Cysharp.Threading.Tasks.ITaskPoolNode<object>);
        var tp49 = typeof(Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>>>>>);
        var tp50 = typeof(Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>>>>);
        var tp51 = typeof(Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>>>);
        var tp52 = typeof(Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>>);
        var tp53 = typeof(Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>);
        var tp54 = typeof(Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>);
        var tp55 = typeof(Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>);
        var tp56 = typeof(Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte, System.ValueTuple<byte, object>>>);
        var tp57 = typeof(Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte, object>>);
        var tp58 = typeof(Cysharp.Threading.Tasks.IUniTaskSource<object>);
        var tp59 = typeof(Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>>>>>.Awaiter);
        var tp60 = typeof(Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>>>>.Awaiter);
        var tp61 = typeof(Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>>>.Awaiter);
        var tp62 = typeof(Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>>.Awaiter);
        var tp63 = typeof(Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>.Awaiter);
        var tp64 = typeof(Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>.Awaiter);
        var tp65 = typeof(Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>.Awaiter);
        var tp66 = typeof(Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte, System.ValueTuple<byte, object>>>.Awaiter);
        var tp67 = typeof(Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte, object>>.Awaiter);
        var tp68 = typeof(Cysharp.Threading.Tasks.UniTask<object>.Awaiter);
        var tp89 = typeof(Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>>>>>>);
        var tp90 = typeof(Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>>>>>);
        var tp91 = typeof(Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>>>>);
        var tp92 = typeof(Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>>>);
        var tp93 = typeof(Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>>);
        var tp94 = typeof(Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>);
        var tp95 = typeof(Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>);
        var tp96 = typeof(Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>);
        var tp97 = typeof(Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte, System.ValueTuple<byte, object>>>);
        var tp98 = typeof(Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte, object>>);
        var tp99 = typeof(Cysharp.Threading.Tasks.UniTask<object>);
        var tp100 = typeof(Cysharp.Threading.Tasks.UniTaskCompletionSourceCore<Cysharp.Threading.Tasks.AsyncUnit>);
        var tp101 = typeof(Cysharp.Threading.Tasks.UniTaskCompletionSourceCore<object>);
        var tp102 = typeof(System.Action<Cysharp.Threading.Tasks.UniTask>);
        var tp104 = typeof(System.Action<byte>);
        var tp105 = typeof(System.Action<int, object>);
        var tp106 = typeof(System.Action<int>);
        var tp107 = typeof(System.Action<object>);
        var tp111 = typeof(System.Collections.Generic.Comparer<Cysharp.Threading.Tasks.UniTask>);
        var tp112 = typeof(System.Collections.Generic.Comparer<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>>>>);
        var tp113 = typeof(System.Collections.Generic.Comparer<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>>>);
        var tp114 = typeof(System.Collections.Generic.Comparer<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>>);
        var tp115 = typeof(System.Collections.Generic.Comparer<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>);
        var tp116 = typeof(System.Collections.Generic.Comparer<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>);
        var tp117 = typeof(System.Collections.Generic.Comparer<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>);
        var tp118 = typeof(System.Collections.Generic.Comparer<System.ValueTuple<byte, System.ValueTuple<byte, object>>>);
        var tp119 = typeof(System.Collections.Generic.Comparer<System.ValueTuple<byte, object>>);
        var tp121 = typeof(System.Collections.Generic.Comparer<byte>);
        var tp122 = typeof(System.Collections.Generic.Comparer<object>);
        var tp123 = typeof(System.Collections.Generic.Comparer<ushort>);
        var tp124 = typeof(System.Collections.Generic.Dictionary<int, object>.Enumerator);
        var tp125 = typeof(System.Collections.Generic.Dictionary<object, object>.Enumerator);
        var tp126 = typeof(System.Collections.Generic.Dictionary<int, object>.KeyCollection.Enumerator);
        var tp127 = typeof(System.Collections.Generic.Dictionary<object, object>.KeyCollection.Enumerator);
        var tp128 = typeof(System.Collections.Generic.Dictionary<int, object>.KeyCollection);
        var tp129 = typeof(System.Collections.Generic.Dictionary<object, object>.KeyCollection);
        var tp130 = typeof(System.Collections.Generic.Dictionary<int, object>.ValueCollection.Enumerator);
        var tp131 = typeof(System.Collections.Generic.Dictionary<object, object>.ValueCollection.Enumerator);
        var tp132 = typeof(System.Collections.Generic.Dictionary<int, object>.ValueCollection);
        var tp133 = typeof(System.Collections.Generic.Dictionary<object, object>.ValueCollection);
        var tp134 = typeof(System.Collections.Generic.Dictionary<int, object>);
        var tp135 = typeof(System.Collections.Generic.Dictionary<object, object>);
        var tp136 = typeof(System.Collections.Generic.EqualityComparer<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>>>>);
        var tp137 = typeof(System.Collections.Generic.EqualityComparer<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>>>);
        var tp138 = typeof(System.Collections.Generic.EqualityComparer<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>>);
        var tp139 = typeof(System.Collections.Generic.EqualityComparer<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>);
        var tp140 = typeof(System.Collections.Generic.EqualityComparer<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>);
        var tp141 = typeof(System.Collections.Generic.EqualityComparer<System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>);
        var tp142 = typeof(System.Collections.Generic.EqualityComparer<System.ValueTuple<byte, System.ValueTuple<byte, object>>>);
        var tp143 = typeof(System.Collections.Generic.EqualityComparer<System.ValueTuple<byte, object>>);
        var tp144 = typeof(System.Collections.Generic.EqualityComparer<byte>);
        var tp145 = typeof(System.Collections.Generic.EqualityComparer<int>);
        var tp146 = typeof(System.Collections.Generic.EqualityComparer<object>);
        var tp147 = typeof(System.Collections.Generic.EqualityComparer<ushort>);
        var tp148 = typeof(System.Collections.Generic.HashSet<object>.Enumerator);
        var tp149 = typeof(System.Collections.Generic.HashSet<object>);
        var tp151 = typeof(System.Collections.Generic.ICollection<Cysharp.Threading.Tasks.UniTask>);
        var tp152 = typeof(System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int, object>>);
        var tp153 = typeof(System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object, object>>);
        var tp155 = typeof(System.Collections.Generic.ICollection<object>);
        var tp156 = typeof(System.Collections.Generic.IComparer<Cysharp.Threading.Tasks.UniTask>);
        var tp158 = typeof(System.Collections.Generic.IComparer<object>);
        var tp159 = typeof(System.Collections.Generic.IEnumerable<Cysharp.Threading.Tasks.UniTask>);
        var tp160 = typeof(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int, object>>);
        var tp161 = typeof(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object, object>>);
        var tp163 = typeof(System.Collections.Generic.IEnumerable<object>);
        var tp164 = typeof(System.Collections.Generic.IEnumerator<Cysharp.Threading.Tasks.UniTask>);
        var tp165 = typeof(System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int, object>>);
        var tp166 = typeof(System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object, object>>);
        var tp168 = typeof(System.Collections.Generic.IEnumerator<object>);
        var tp169 = typeof(System.Collections.Generic.IEqualityComparer<int>);
        var tp170 = typeof(System.Collections.Generic.IEqualityComparer<object>);
        var tp171 = typeof(System.Collections.Generic.IList<Cysharp.Threading.Tasks.UniTask>);
        var tp173 = typeof(System.Collections.Generic.IList<object>);
        var tp174 = typeof(System.Collections.Generic.KeyValuePair<int, object>);
        var tp175 = typeof(System.Collections.Generic.KeyValuePair<object, object>);
        var tp176 = typeof(System.Collections.Generic.LinkedList<object>.Enumerator);
        var tp177 = typeof(System.Collections.Generic.LinkedList<object>);
        var tp178 = typeof(System.Collections.Generic.LinkedListNode<object>);
        var tp179 = typeof(System.Collections.Generic.List<Cysharp.Threading.Tasks.UniTask>.Enumerator);
        var tp181 = typeof(System.Collections.Generic.List<object>.Enumerator);
        var tp182 = typeof(System.Collections.Generic.List<Cysharp.Threading.Tasks.UniTask>);
        var tp184 = typeof(System.Collections.Generic.List<object>);
        var tp208 = typeof(System.Collections.Generic.Queue<object>.Enumerator);
        var tp209 = typeof(System.Collections.Generic.Queue<object>);
        var tp210 = typeof(System.Collections.ObjectModel.ReadOnlyCollection<Cysharp.Threading.Tasks.UniTask>);
        var tp212 = typeof(System.Collections.ObjectModel.ReadOnlyCollection<object>);
        var tp213 = typeof(System.Comparison<Cysharp.Threading.Tasks.UniTask>);
        var tp215 = typeof(System.Comparison<object>);
        var tp216 = typeof(System.Func<byte>);
        var tp217 = typeof(System.Func<int>);
        var tp218 = typeof(System.Func<object, byte>);
        var tp219 = typeof(System.Func<object, object>);
        var tp225 = typeof(System.Nullable<double>);
        var tp226 = typeof(System.Nullable<long>);
        var tp227 = typeof(System.Predicate<Cysharp.Threading.Tasks.UniTask>);
        var tp229 = typeof(System.Predicate<object>);
        var tp230 = typeof(System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>>>>>);
        var tp231 = typeof(System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>>>>);
        var tp232 = typeof(System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>>>);
        var tp233 = typeof(System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>>);
        var tp234 = typeof(System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>>);
        var tp235 = typeof(System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>>);
        var tp236 = typeof(System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>>);
        var tp237 = typeof(System.ValueTuple<byte, System.ValueTuple<byte, System.ValueTuple<byte, object>>>);
        var tp238 = typeof(System.ValueTuple<byte, System.ValueTuple<byte, object>>);
        var tp239 = typeof(System.ValueTuple<byte, object, ushort, object>);
        var tp240 = typeof(System.ValueTuple<byte, object>);
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
