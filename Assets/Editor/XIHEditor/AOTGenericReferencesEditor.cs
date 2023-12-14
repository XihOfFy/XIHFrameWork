using HybridCLR.Editor.Settings;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class AOTGenericReferencesEditor
{
    [MenuItem("XIHUtil/Hclr/GenerStrippedCS")]
    static void GenerStrippedCS() {
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
        var lines= File.ReadAllLines(src);
        int len=lines.Length;
        int curLine = -1;
        sb.AppendLine(@"	public void GenericType()
	{");
        while (++curLine < len) { 
            var line = lines[curLine].Trim();
            if (line.Equals("// {{ AOT generic types")) {
                while (++curLine < len)
                {
                    line = lines[curLine].Trim();
                    if (line.Equals("// }}")) break;
                    GenericType(curLine,line, sb);
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
                    ReferenceMethod(curLine,line, sb);
                }
            }
        }
        sb.AppendLine(@"	}
}");
        File.WriteAllText(dst, sb.ToString());
        AssetDatabase.Refresh();
        Debug.LogWarning($" AOTGenericReferences 将{src}后处理为生成{dst}");
    }

    static void GenericType(int curLine, string line,StringBuilder sb) {
        sb.AppendLine($"        var tp{curLine} = typeof({line.Trim().Substring(2)});");
    }
    static void ReferenceMethod(int curLine, string line,StringBuilder sb) { 
        
    }
}
