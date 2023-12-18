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
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<Aot2Hot.Aot2HotMgr.<DownloadAot2HotRes>d__7>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<Aot2Hot.Aot2HotMgr.<GotoHotScene>d__5>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<Aot2Hot.Aot2HotMgr.<DownloadAot2HotRes>d__7>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<Aot2Hot.Aot2HotMgr.<GotoHotScene>d__5>
	// Cysharp.Threading.Tasks.ITaskPoolNode<object>
	// Cysharp.Threading.Tasks.TaskPool<object>
	// System.Func<int>
	// }}

	public void RefMethods()
	{
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,Aot2Hot.Aot2HotMgr.<DownloadAot2HotRes>d__7>(Cysharp.Threading.Tasks.UniTask.Awaiter&,Aot2Hot.Aot2HotMgr.<DownloadAot2HotRes>d__7&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,Aot2Hot.Aot2HotMgr.<GotoHotScene>d__5>(Cysharp.Threading.Tasks.UniTask.Awaiter&,Aot2Hot.Aot2HotMgr.<GotoHotScene>d__5&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<Aot2Hot.Aot2HotMgr.<DownloadAot2HotRes>d__7>(Aot2Hot.Aot2HotMgr.<DownloadAot2HotRes>d__7&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<Aot2Hot.Aot2HotMgr.<GotoHotScene>d__5>(Aot2Hot.Aot2HotMgr.<GotoHotScene>d__5&)
		// YooAsset.AllAssetsHandle YooAsset.ResourcePackage.LoadAllAssetsAsync<object>(string,uint)
		// YooAsset.AllAssetsHandle YooAsset.YooAssets.LoadAllAssetsAsync<object>(string,uint)
	}
}