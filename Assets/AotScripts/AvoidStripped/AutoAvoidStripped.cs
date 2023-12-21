
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
        var tp17 = typeof( System.Action<int>);
        var tp18 = typeof( System.Action<object>);
        var tp19 = typeof( System.Collections.Generic.Comparer<byte>);
        var tp20 = typeof( System.Collections.Generic.Comparer<object>);
        var tp21 = typeof( System.Collections.Generic.Comparer<ushort>);
        var tp22 = typeof( System.Collections.Generic.EqualityComparer<byte>);
        var tp23 = typeof( System.Collections.Generic.EqualityComparer<object>);
        var tp24 = typeof( System.Collections.Generic.EqualityComparer<ushort>);
        var tp25 = typeof( System.Collections.Generic.IEnumerable<object>);
        var tp26 = typeof( System.Collections.Generic.IEnumerator<object>);
        var tp27 = typeof( System.Collections.Generic.LinkedList<object>.Enumerator);
        var tp28 = typeof( System.Collections.Generic.LinkedList<object>);
        var tp29 = typeof( System.Collections.Generic.LinkedListNode<object>);
        var tp36 = typeof( System.Nullable<double>);
        var tp37 = typeof( System.Nullable<long>);
        var tp38 = typeof( System.ValueTuple<byte,object,ushort,object>);
	}
public void RefMethods()
	{
	}
}
