using HybridCLR.Editor.Settings;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class AOTGenericReferencesEditor
{
    static readonly Dictionary<string, string> reversed = new Dictionary<string, string>() {
        { "Cysharp.Threading.Tasks.UniTask.Awaiter","Awaiter"},
        { "Spine.ExposedList.Enumerator","Enumerator"},
        { "System.ArraySegment.Enumerator","Enumerator"},
        { "System.Collections.Generic.Dictionary.Enumerator","Enumerator"},
        { "System.Collections.Generic.Dictionary.KeyCollection.Enumerator","KeyCollection.Enumerator"},
        { "System.Collections.Generic.Dictionary.KeyCollection","KeyCollection"},
        { "System.Collections.Generic.Dictionary.ValueCollection.Enumerator","ValueCollection.Enumerator"},
        { "System.Collections.Generic.Dictionary.ValueCollection","ValueCollection"},
        { "System.Collections.Generic.HashSet.Enumerator","Enumerator"},
        { "System.Collections.Generic.LinkedList.Enumerator","Enumerator"},
        { "System.Collections.Generic.List.Enumerator","Enumerator"},
        { "System.Collections.Generic.Queue.Enumerator","Enumerator"},
        { "System.Collections.Generic.SortedDictionary.KeyCollection","KeyCollection"},
        { "System.Collections.Generic.SortedDictionary.KeyCollection.Enumerator","KeyCollection.Enumerator"},
        { "System.Collections.Generic.SortedDictionary.ValueCollection","ValueCollection"},
        { "System.Collections.Generic.SortedDictionary.ValueCollection.Enumerator","ValueCollection.Enumerator"},
        { "System.Collections.Generic.SortedSet.Enumerator","Enumerator"},
        { "System.Span.Enumerator","Enumerator"},
        { "System.Collections.Generic.Stack.Enumerator","Enumerator"},
        { "System.Collections.Generic.SortedDictionary.Enumerator","Enumerator"},
    };
    static readonly HashSet<string> skipContains = new HashSet<string>() {
        "Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<",
        "Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<",
        "Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<",
        "Cysharp.Threading.Tasks.Internal.StatePool<",
        "Cysharp.Threading.Tasks.Internal.StateTuple<",
        "Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<",
        "Cysharp.Threading.Tasks.CompilerServices.IStateMachineRunnerPromise<",
        "Cysharp.Threading.Tasks.Internal.StatePool<",
        "Cysharp.Threading.Tasks.Internal.StateTuple<",
        "Cysharp.Threading.Tasks.UniTask.IsCanceledSource<",
        "Cysharp.Threading.Tasks.UniTask.MemoizeSource<",
        "Cysharp.Threading.Tasks.UniTaskExtensions.<",

        "System.Buffers.TlsOverPerCoreLockedStacksArrayPool.",
        "System.Buffers.TlsOverPerCoreLockedStacksArrayPool<",
        "System.ByReference<",
        "System.Collections.Concurrent.ConcurrentQueue.",
        "System.Collections.Generic.HashSetEqualityComparer<",
        "System.Collections.Generic.SortedSet.<",
        "System.Collections.Generic.SortedDictionary.<",
        "System.Collections.Generic.SortedDictionary.KeyCollection.<",
        "System.Collections.Generic.SortedDictionary.ValueCollection.<",
        "System.Collections.Generic.SortedDictionary.KeyValuePairComparer<",

        "System.Collections.Generic.ArraySortHelper<",
        "System.Collections.Generic.ObjectComparer<",
        "System.Collections.Generic.ObjectEqualityComparer<",
        "System.Collections.Generic.SortedSet.Node<",
        "System.Collections.Generic.TreeSet<",
        "System.Collections.Generic.TreeWalkPredicate<",
        "System.Collections.Generic.ValueListBuilder<",
        "System.Linq.Buffer<",
        "System.Linq.EnumerableSorter<",
        "System.Linq.OrderedEnumerable<",

        "XiHUI.UIParam",
        "System.Linq.Enumerable.",
        "System.Linq.OrderedEnumerable.",
    };

    [MenuItem("XIHUtil/Hclr/GenerStrippedCS")]
    static void GenerStrippedCS()
    {
        var path = HybridCLRSettings.Instance.outputAOTGenericReferenceFile;
        var src = $"{Application.dataPath}/{path}";
        if (!File.Exists(src))
        {
            Debug.LogError($"请确保文件存在: {src}");
            return;
        }
        var dst = "Assets/AotScripts/AvoidStripped/AutoAvoidStripped.cs";
        var sb = new StringBuilder();
        sb.AppendLine(@"
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
    }");
        var lines = File.ReadAllLines(src);
        int len = lines.Length;
        int curLine = -1;
        sb.AppendLine(@"	public void GenericType()
    {");
        while (++curLine < len)
        {
            var line = lines[curLine].Trim();
            if (line.Equals("// {{ AOT generic types"))
            {
                while (++curLine < len)
                {
                    line = lines[curLine].Trim();
                    if (line.Equals("// }}")) break;
                    GenericType(curLine, line, sb);
                }
            }
        }
        sb.AppendLine(@"	}
public void RefMethods()
    {");
        while (++curLine < len)
        {
            var line = lines[curLine].Trim();
            if (line.Equals("{"))
            {
                while (++curLine < len)
                {
                    line = lines[curLine].Trim();
                    if (line.Equals("}")) break;
                    ReferenceMethod(curLine, line, sb);
                }
            }
        }
        sb.AppendLine(@"	}
}");
        File.WriteAllText(dst, sb.ToString());
        AssetDatabase.Refresh();
        Debug.LogWarning($" AOTGenericReferences 将{src}后处理为生成{dst}");
    }

    static void GenericType(int curLine, string line, StringBuilder sb)
    {

        line = line.Trim().Substring(2).Trim();
        foreach (var str in skipContains)
        {
            if (line.Contains(str)) return;
        }
        foreach (var str in reversed)
        {
            if (line.StartsWith(str.Key))
            {
                var suf = line.Substring(str.Key.Length);
                var pre = line.Substring(0, str.Key.Length - str.Value.Length - 1);
                line = pre + suf + "." + str.Value;
            }
        }

        sb.AppendLine($"        var tp{curLine} = typeof({line});");
    }
    static void ReferenceMethod(int curLine, string line, StringBuilder sb)
    {

    }
}
