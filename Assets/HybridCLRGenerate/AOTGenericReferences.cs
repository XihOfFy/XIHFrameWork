using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"YooAsset.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// System.Collections.Generic.IEnumerator<object>
	// }}

	public void RefMethods()
	{
		// YooAsset.AllAssetsHandle YooAsset.ResourcePackage.LoadAllAssetsAsync<object>(string,uint)
		// YooAsset.AllAssetsHandle YooAsset.YooAssets.LoadAllAssetsAsync<object>(string,uint)
	}
}