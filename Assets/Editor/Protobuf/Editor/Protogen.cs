#if UNITY_2018_1_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProtoBuf;
using Google.Protobuf.Reflection;
using ProtoBuf.Reflection;
using System.IO;

public class Protogen {
    [MenuItem("XIHUtil/Bundle/Generate Protocs")]
    public static void GenerateProtobufCS(){
		Generate(Application.dataPath + "/../../../Others/Proto/", new string[]{"mmopb.proto"},Application.dataPath + "/../../HotFix/");
    }
    static void Generate(string inpath,string[] inprotos,string outpath){

		var set = new FileDescriptorSet();

		set.AddImportPath(inpath);
		foreach (var inproto in inprotos) {
			set.Add (inproto, true);
		}

		set.Process();
		var errors = set.GetErrors();
		CSharpCodeGenerator.ClearTypeNames ();
		var files = CSharpCodeGenerator.Default.Generate(set);

        int idx = 1;
		foreach (var file in files)
		{
            EditorUtility.DisplayProgressBar("Generate", file.Name, idx / (1.0f * inprotos.Length));
			var path = Path.Combine(outpath, file.Name);
			File.WriteAllText(path, file.Text);

			Debug.Log($"generated: {path}");
		}
        EditorUtility.ClearProgressBar();
    }
}
#endif