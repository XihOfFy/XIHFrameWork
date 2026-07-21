using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Tmpl;
using UnityEditor;
using UnityEngine;

public class TMPCharacterCollectorUtil
{
    [MenuItem("XIHUtil/TMPCharacterCollector")]
    static void CreateSpriteAtlas4AvatarPng()
    {
        var strSet = new HashSet<string>();
        Debug.Log($"TMPCharacterCollector Start");
        //var cfgs = Directory.GetFiles("Assets/Resources/Assets/Res/Data", "*.csv");
        /*var cfgs = Directory.GetFiles("datatmpl/tmpl", "*.csv",SearchOption.AllDirectories);
        foreach (var file in cfgs) {
            var cont = File.ReadAllText(file);
            strSet.Add(cont);
        }*/
        var tbs = Tables.Instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (var tbsField in tbs)
        {
            if (tbsField.Name.StartsWith("Tb"))
            {
                var tb = tbsField.GetValue(Tables.Instance);
                var datas = tb.GetType().GetField("_dataList", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
                if (datas != null)
                {
                    var vals = (IEnumerable)datas.GetValue(tb);
                    foreach (var ls in vals)
                    {
                        strSet.Add(ls.ToString());
                        //Debug.Log(ls.ToString());
                    }
                }
                else
                {
                    datas = tb.GetType().GetField("_data", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
                    var val = datas.GetValue(tb);
                    strSet.Add(val.ToString());
                }
            }
        }




        var cfgs = Directory.GetFiles("Assets/Resources", "*.xml");
        foreach (var file in cfgs)
        {
            var cont = File.ReadAllText(file);
            strSet.Add(cont);
        }
        cfgs = Directory.GetFiles("Assets/Res", "*.xml");
        foreach (var file in cfgs)
        {
            var cont = File.ReadAllText(file);
            strSet.Add(cont);
        }

        var fguiPath = "fairyprj/assets";
        var fguis = Directory.GetFiles(fguiPath, "*.xml", SearchOption.AllDirectories);
        foreach (var fgui in fguis)
        {
            var cont = File.ReadAllText(fgui);
            strSet.Add(cont);
        }
        ;

        var dirPaths = new List<string>() {
            //"Assets/Aot2HotScripts",
            "Assets/AotScripts",
            "Assets/HotScripts",
        };
        foreach (var path in dirPaths)
        {
            var fs = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
            foreach (var f in fs)
            {
                var conts = File.ReadAllLines(f);
                var inBlock = false;
                foreach (var cont in conts)
                {
                    var line = cont.Trim();
                    var sbLine = new StringBuilder(line.Length);
                    for (var ci = 0; ci < line.Length; ci++)
                    {
                        if (inBlock)
                        {
                            if (ci + 1 < line.Length && line[ci] == '*' && line[ci + 1] == '/')
                            {
                                inBlock = false;
                                ci++;
                            }
                            continue;
                        }
                        if (ci + 1 < line.Length)
                        {
                            if (line[ci] == '/' && line[ci + 1] == '/')
                                break;
                            if (line[ci] == '/' && line[ci + 1] == '*')
                            {
                                inBlock = true;
                                ci++;
                                continue;
                            }
                        }
                        sbLine.Append(line[ci]);
                    }
                    var stripped = sbLine.ToString().Trim();
                    if (string.IsNullOrEmpty(stripped)) continue;
                    if (stripped.Contains("Log")) continue;
                    if (stripped.Contains("[Header")) continue;
                    if (stripped.Contains("[ToolTip")) continue;
                    strSet.Add(stripped);
                }
            }
        }
        /*var fs = Directory.GetFiles("Assets/AotRes", "*.unity", SearchOption.AllDirectories);//不支持，因为yml中文使用\u编码
        foreach (var f in fs)
        {
            var cont = File.ReadAllText(f);
            strSet.Add(cont);
            Debug.Log(cont);
        }*/
        var prefabScenePaths = new List<string>() {
            "Assets/Res/",
        };
        foreach (var prefabScenePath in prefabScenePaths)
        {
            foreach (var pattern in new[] { "*.prefab", "*.unity" })
            {
                var files = Directory.GetFiles(prefabScenePath, pattern, SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var content = File.ReadAllText(file);
                    // Unity YAML 中 \uXXXX 解码为原生 Unicode 字符
                    content = Regex.Replace(content, @"\\u([0-9a-fA-F]{4})",
                        m => ((char)int.Parse(m.Groups[1].Value, NumberStyles.HexNumber)).ToString());
                    strSet.Add(content);
                }
            }
        }


        var sb = new StringBuilder();
        var cSet = new HashSet<char>();
        //已添加。遍历可打印 ASCII 范围 32（空格）到 126（~），确保所有英文字母、数字、标点符号都包含在字符集中，即使代码和配置中没有显式使用到也不会遗漏
        for (char c = (char)32; c < 127; c++)
        {
            cSet.Add(c);
        }
        foreach (var str in strSet)
        {
            foreach (var c in str.ToCharArray())
            {
                cSet.Add(c);
            }
        }
        foreach (var c in cSet)
        {
            sb.Append(c);
        }
        File.WriteAllText("TMPCharacterCollector.txt", sb.ToString());
        Debug.LogWarning($"TMPCharacterCollector End >> TMPCharacterCollector.txt");
        EditorUtility.OpenWithDefaultApp("TMPCharacterCollector.txt");
    }
}
