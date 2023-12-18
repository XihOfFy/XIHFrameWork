
using System;
using UnityEngine;

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
        var tp16 = typeof( System.Collections.Generic.IEnumerator<object>);
	}
public void RefMethods()
	{
	}
}
