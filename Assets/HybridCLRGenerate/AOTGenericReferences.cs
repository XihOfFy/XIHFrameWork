using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"UniTask.dll",
		"YooAsset.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<Aot2Hot.Aot2HotMgr.<DownloadHotRes>d__8>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<Aot2Hot.Aot2HotMgr.<GotoHotScene>d__6>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<Aot2Hot.Aot2HotMgr.<DownloadHotRes>d__8>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<Aot2Hot.Aot2HotMgr.<GotoHotScene>d__6>
	// Cysharp.Threading.Tasks.ITaskPoolNode<object>
	// Cysharp.Threading.Tasks.TaskPool<object>
	// System.Collections.Generic.IEnumerator<object>
	// System.Func<int>
	// }}

	public void RefMethods()
	{
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,Aot2Hot.Aot2HotMgr.<DownloadHotRes>d__8>(Cysharp.Threading.Tasks.UniTask.Awaiter&,Aot2Hot.Aot2HotMgr.<DownloadHotRes>d__8&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,Aot2Hot.Aot2HotMgr.<GotoHotScene>d__6>(Cysharp.Threading.Tasks.UniTask.Awaiter&,Aot2Hot.Aot2HotMgr.<GotoHotScene>d__6&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<Aot2Hot.Aot2HotMgr.<DownloadHotRes>d__8>(Aot2Hot.Aot2HotMgr.<DownloadHotRes>d__8&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<Aot2Hot.Aot2HotMgr.<GotoHotScene>d__6>(Aot2Hot.Aot2HotMgr.<GotoHotScene>d__6&)
		// YooAsset.AllAssetsHandle YooAsset.ResourcePackage.LoadAllAssetsAsync<object>(string,uint)
		// YooAsset.AllAssetsHandle YooAsset.YooAssets.LoadAllAssetsAsync<object>(string,uint)
	}
}