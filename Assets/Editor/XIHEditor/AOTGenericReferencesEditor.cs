using HybridCLR.Editor.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 将 HybridCLR 生成的 AOTGenericReferences 转为可编译的 AvoidStripped 代码。
/// 规则：
/// 1. 内部方法相关类型直接跳过（不写出）
/// 2. 不存在/不可访问的类、内部类替换为 object / StructType / EnumType
/// 3. 生成 AutoAvoidStripped 后，把 GenericType 融合进 SavedAvoidStripped
/// </summary>
public class AOTGenericReferencesEditor
{
    const string AutoRelPath = "Assets/AotScripts/AvoidStripped/AutoAvoidStripped.cs";
    const string SavedRelPath = "Assets/AotScripts/AvoidStripped/SavedAvoidStripped.cs";

    static string AutoFullPath => Path.GetFullPath(AutoRelPath);
    static string SavedFullPath => Path.GetFullPath(SavedRelPath);

    static readonly Dictionary<string, string> reversed = new Dictionary<string, string>()
    {
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

    static readonly HashSet<string> skipContains = new HashSet<string>()
    {
        "Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<",
        "Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<",
        "Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<",
        "Cysharp.Threading.Tasks.Internal.StatePool<",
        "Cysharp.Threading.Tasks.Internal.StateTuple<",
        "Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<",
        "Cysharp.Threading.Tasks.CompilerServices.IStateMachineRunnerPromise<",
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

        // 内部方法 / 编译器生成类型：直接跳过
        "<>c__DisplayClass",
        ".<>c<",
        "ValueTaskSourceAsTask",
        "ContinuationTaskFromResultTask",
    };

    static readonly Regex ObfuscatedTypeRegex = new Regex(
        @"\$[A-Za-z0-9_]+(?:\.\$[A-Za-z0-9_]+)*(?:<(?:[^<>]|\$[A-Za-z0-9_.<>]+)*>)?",
        RegexOptions.Compiled);

    static readonly Regex InternalMethodNestedRegex = new Regex(
        @"\.\s*<[^>]+>\s*[a-zA-Z_]",
        RegexOptions.Compiled);

    static readonly Regex CompilerGenDRegex = new Regex(
        @"<[A-Za-z_][A-Za-z0-9_]*>d__",
        RegexOptions.Compiled);

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

        var uniqueTypes = new HashSet<string>();
        var typeLines = new List<string>();
        var methodLines = new List<string>();

        var lines = File.ReadAllLines(src);
        int len = lines.Length;
        int curLine = -1;

        while (++curLine < len)
        {
            var line = lines[curLine].Trim();
            if (line.Equals("// {{ AOT generic types"))
            {
                while (++curLine < len)
                {
                    line = lines[curLine].Trim();
                    if (line.Equals("// }}")) break;
                    TryAddGenericType(curLine, line, uniqueTypes, typeLines);
                }
            }
        }

        while (++curLine < len)
        {
            var line = lines[curLine].Trim();
            if (line.Equals("{"))
            {
                while (++curLine < len)
                {
                    line = lines[curLine].Trim();
                    if (line.Equals("}")) break;
                    // 内部方法：按规则删除错误行，不写入
                    TryAddReferenceMethod(curLine, line, methodLines);
                }
            }
        }

        WriteAutoAvoidStripped(typeLines, methodLines);
        MergeIntoSavedAvoidStripped(typeLines);

        AssetDatabase.Refresh();
        Debug.LogWarning($"AOTGenericReferences 已处理：生成 {AutoRelPath}，并融合 GenericType 到 {SavedRelPath}（类型 {typeLines.Count} 条）");
    }

    static void TryAddGenericType(int curLine, string line, HashSet<string> uniqueTypes, List<string> typeLines)
    {
        if (string.IsNullOrEmpty(line) || !line.StartsWith("//"))
            return;

        line = line.Substring(2).Trim();
        foreach (var str in skipContains)
        {
            if (line.Contains(str)) return;
        }

        // Nested enumerator 等写法修正：Dictionary.Enumerator<object,int> → Dictionary<object,int>.Enumerator
        foreach (var str in reversed)
        {
            if (line.StartsWith(str.Key))
            {
                var suf = line.Substring(str.Key.Length);
                var pre = line.Substring(0, str.Key.Length - str.Value.Length - 1);
                line = pre + suf + "." + str.Value;
            }
        }

        line = FixTypeExpression(line);
        if (string.IsNullOrEmpty(line))
            return;

        if (!uniqueTypes.Add(line))
            return;

        typeLines.Add($"        var tp{curLine} = typeof({line});");
    }

    /// <summary>
    /// 修复不可编译的类型表达式；返回 null/空 表示删除该行。
    /// </summary>
    static string FixTypeExpression(string expr)
    {
        if (string.IsNullOrEmpty(expr))
            return null;

        // 内部方法状态机：.<GetEnumerator>d__35
        if (InternalMethodNestedRegex.IsMatch(expr))
            return null;
        if (expr.Contains("<>c__DisplayClass") || expr.Contains(".<>c<") || expr.Contains(".<>c>"))
            return null;
        if (expr.Contains("ValueTaskSourceAsTask") || expr.Contains("ContinuationTaskFromResultTask"))
            return null;

        // ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<T> → ConfiguredTaskAwaitable<T>.ConfiguredTaskAwaiter
        expr = RewriteConfiguredAwaitable(expr, "ConfiguredTaskAwaitable", "ConfiguredTaskAwaiter");
        expr = RewriteConfiguredAwaitable(expr, "ConfiguredValueTaskAwaitable", "ConfiguredValueTaskAwaiter");

        // 内部结构 VoidTaskResult → StructType
        expr = expr.Replace("System.Threading.Tasks.VoidTaskResult", "StructType");

        // 混淆类型 $B.$d / $J.$Bc → object
        while (true)
        {
            var next = ObfuscatedTypeRegex.Replace(expr, "object");
            if (next == expr) break;
            expr = next;
        }

        // 仍含编译器生成符号 → 删除
        if (expr.Contains("<>") || CompilerGenDRegex.IsMatch(expr))
            return null;

        // ConcurrentDictionary 内部嵌套类型 → object
        if (Regex.IsMatch(expr, @"ConcurrentDictionary\.(DictionaryEnumerator|Node|Tables)<"))
            return "object";

        return expr;
    }

    static string RewriteConfiguredAwaitable(string expr, string kind, string awaiter)
    {
        var prefix = $"System.Runtime.CompilerServices.{kind}.{awaiter}<";
        while (true)
        {
            var idx = expr.IndexOf(prefix, StringComparison.Ordinal);
            if (idx < 0) break;

            var start = idx + prefix.Length;
            var depth = 1;
            var i = start;
            while (i < expr.Length && depth > 0)
            {
                var c = expr[i];
                if (c == '<') depth++;
                else if (c == '>') depth--;
                i++;
            }
            if (depth != 0) break;

            var targs = expr.Substring(start, i - start - 1);
            var repl = $"System.Runtime.CompilerServices.{kind}<{targs}>.{awaiter}";
            expr = expr.Substring(0, idx) + repl + expr.Substring(i);
        }
        return expr;
    }

    static void TryAddReferenceMethod(int curLine, string line, List<string> methodLines)
    {
        // 内部方法行一律跳过（无法在源码中直接引用）
        if (string.IsNullOrEmpty(line)) return;
        if (line.Contains("<") && line.Contains(">")) return;
    }

    static void WriteAutoAvoidStripped(List<string> typeLines, List<string> methodLines)
    {
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
    }
    public void GenericType()
    {");
        foreach (var l in typeLines)
            sb.AppendLine(l);

        sb.AppendLine(@"    }
    public void RefMethods()
    {");
        foreach (var l in methodLines)
            sb.AppendLine(l);

        sb.AppendLine(@"    }
}");
        File.WriteAllText(AutoFullPath, sb.ToString(), new UTF8Encoding(false));
    }

    /// <summary>
    /// 将 Auto 的 GenericType 融合进 SavedAvoidStripped，保留 Saved 的 RefMethods。
    /// 用括号配对定位方法体，避免正则在 \r\n / BOM 下匹配失败。
    /// </summary>
    static void MergeIntoSavedAvoidStripped(List<string> typeLines)
    {
        if (!File.Exists(SavedFullPath))
        {
            Debug.LogError($"融合失败，找不到 {SavedFullPath}");
            return;
        }

        var saved = File.ReadAllText(SavedFullPath);
        if (!TryReplaceMethodBody(saved, "GenericType", BuildGenericTypeBody(typeLines), out var merged, out var err))
        {
            Debug.LogError($"融合失败：{err}（文件: {SavedFullPath}）");
            return;
        }

        File.WriteAllText(SavedFullPath, merged, new UTF8Encoding(true)); // 保留 UTF-8 BOM，与 Unity 习惯一致
        Debug.Log($"已融合 GenericType → {SavedRelPath}");
    }

    static string BuildGenericTypeBody(List<string> typeLines)
    {
        var sb = new StringBuilder();
        sb.AppendLine("    public void GenericType()");
        sb.AppendLine("    {");
        foreach (var l in typeLines)
            sb.AppendLine(l);
        sb.Append("    }");
        return sb.ToString();
    }

    /// <summary>
    /// 定位 `public void methodName(...)` 到配对的方法体结束 `}`，整段替换为 newMethodText。
    /// 注意：参数列表用 [^)]*，不能用 [^;]*（否则会吞掉 typeof(...) 的括号）。
    /// </summary>
    static bool TryReplaceMethodBody(string source, string methodName, string newMethodText, out string result, out string error)
    {
        result = source;
        error = null;

        // 只匹配到方法名，再手动找参数括号与方法体，避免正则误吞 typeof(...)
        var namePattern = new Regex(
            @"^[ \t]*public\s+void\s+" + Regex.Escape(methodName) + @"\b",
            RegexOptions.Multiline);

        var match = namePattern.Match(source);
        if (!match.Success)
        {
            error = $"未找到方法签名 public void {methodName}";
            return false;
        }

        var sigStart = match.Index;
        var i = match.Index + match.Length;

        // 跳过可选泛型参数 <...>
        i = SkipWhite(source, i);
        if (i < source.Length && source[i] == '<')
        {
            i = SkipBalanced(source, i, '<', '>');
            if (i < 0)
            {
                error = $"{methodName} 泛型参数括号未闭合";
                return false;
            }
        }

        // 参数列表 (...)
        i = SkipWhite(source, i);
        if (i >= source.Length || source[i] != '(')
        {
            error = $"{methodName} 未找到参数列表 '('";
            return false;
        }
        i = SkipBalanced(source, i, '(', ')');
        if (i < 0)
        {
            error = $"{methodName} 参数列表括号未闭合";
            return false;
        }

        // 可选 where 约束（可多行）
        while (true)
        {
            var j = SkipWhite(source, i);
            if (j + 5 <= source.Length &&
                string.Compare(source, j, "where", 0, 5, StringComparison.Ordinal) == 0 &&
                (j + 5 >= source.Length || !char.IsLetterOrDigit(source[j + 5])))
            {
                // 读到下一行 where 或 {
                i = j + 5;
                while (i < source.Length && source[i] != '{' && source[i] != '\n')
                    i++;
                if (i < source.Length && source[i] == '\n')
                    i++;
                continue;
            }
            i = j;
            break;
        }

        i = SkipWhite(source, i);
        if (i >= source.Length || source[i] != '{')
        {
            error = $"找到 {methodName} 签名但未找到方法体 '{{'";
            return false;
        }

        var braceClose = SkipBalanced(source, i, '{', '}');
        if (braceClose < 0)
        {
            error = $"找到 {methodName} 起始 '{{' 但未能配对结束 '}}'";
            return false;
        }

        // SkipBalanced 返回闭括号之后的位置
        result = source.Substring(0, sigStart) + newMethodText + source.Substring(braceClose);
        return true;
    }

    static int SkipWhite(string s, int i)
    {
        while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
        return i;
    }

    /// <summary>
    /// 从 openPos（指向开括号）开始配对，返回闭括号后一位；失败返回 -1。
    /// </summary>
    static int SkipBalanced(string s, int openPos, char open, char close)
    {
        if (openPos < 0 || openPos >= s.Length || s[openPos] != open)
            return -1;
        var depth = 0;
        for (var i = openPos; i < s.Length; i++)
        {
            var c = s[i];
            if (c == open) depth++;
            else if (c == close)
            {
                depth--;
                if (depth == 0)
                    return i + 1;
            }
        }
        return -1;
    }
}
