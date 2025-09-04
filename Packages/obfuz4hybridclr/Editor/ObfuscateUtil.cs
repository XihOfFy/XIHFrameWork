﻿using dnlib.DotNet;
using dnlib.DotNet.PolymorphicWriter;
using HybridCLR.Editor;
using Obfuz;
using Obfuz.Settings;
using Obfuz.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Obfuz4HybridCLR
{
    public static class ObfuscateUtil
    {
        public static string PackageName { get; } = "com.code-philosophy.obfuz4hybridclr";

        public static string TemplatePathInPackage => $"Packages/{PackageName}/Templates~";

        public static bool AreSameDirectory(string path1, string path2)
        {
            try
            {
                var dir1 = new DirectoryInfo(path1);
                var dir2 = new DirectoryInfo(path2);

                return dir1.FullName.TrimEnd('\\') == dir2.FullName.TrimEnd('\\');
            }
            catch
            {
                return false;
            }
        }

        public static void ObfuscateHotUpdateAssemblies(BuildTarget target, string outputDir)
        {
            string hotUpdateDllPath = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);

            AssemblySettings assemblySettings = ObfuzSettings.Instance.assemblySettings;
            ObfuscationProcess.ValidateReferences(hotUpdateDllPath, new HashSet<string>(assemblySettings.GetAssembliesToObfuscate()), new HashSet<string>(assemblySettings.GetObfuscationRelativeAssemblyNames()));
            var assemblySearchPaths = new List<string>
            {
                hotUpdateDllPath,
            };
            if (AreSameDirectory(hotUpdateDllPath, outputDir))
            {
                throw new Exception($"hotUpdateDllPath:{hotUpdateDllPath} can't be same to outputDir:{outputDir}");
            }
            Obfuscate(target, assemblySearchPaths, outputDir);
            foreach (string hotUpdateAssemblyName in SettingsUtil.HotUpdateAssemblyNamesExcludePreserved)
            {
                string srcFile = $"{hotUpdateDllPath}/{hotUpdateAssemblyName}.dll";
                string dstFile = $"{outputDir}/{hotUpdateAssemblyName}.dll";
                // only copy non obfuscated assemblies
                if (File.Exists(srcFile) && !File.Exists(dstFile))
                {
                    File.Copy(srcFile, dstFile, true);
                    Debug.Log($"[CompileAndObfuscateDll] Copy nonObfuscated assembly {srcFile} to {dstFile}");
                }
            }
        }

        public static void Obfuscate(BuildTarget target, List<string> assemblySearchPaths, string obfuscatedAssemblyOutputPath)
        {
            var obfuzSettings = ObfuzSettings.Instance;

            var assemblySearchDirs = assemblySearchPaths;
            ObfuscatorBuilder builder = ObfuscatorBuilder.FromObfuzSettings(obfuzSettings, target, true);
            builder.InsertTopPriorityAssemblySearchPaths(assemblySearchDirs);
            builder.CoreSettingsFacade.obfuscatedAssemblyOutputPath = obfuscatedAssemblyOutputPath;

            foreach (var assemblySearchDir in builder.CoreSettingsFacade.assemblySearchPaths)
            {
                if (AreSameDirectory(assemblySearchDir, obfuscatedAssemblyOutputPath))
                {
                    throw new Exception($"assemblySearchDir:{assemblySearchDir} can't be same to ObfuscatedAssemblyOutputPath:{obfuscatedAssemblyOutputPath}");
                }
            }

            Obfuscator obfuz = builder.Build();
            obfuz.Run();
        }

        public static void GeneratePolymorphicDll(string originalDllPath, string outputDllPath)
        {
            ModuleDef oldMod = ModuleDefMD.Load(originalDllPath);
            var obfuzSettings = ObfuzSettings.Instance;

            var opt = new NewDllModuleWriterOptions(oldMod)
            {
                MetadataWriter = new PolymorphicMetadataWriter(obfuzSettings.polymorphicDllSettings.codeGenerationSecretKey),
            };
            PolymorphicModuleWriter writer = new PolymorphicModuleWriter(oldMod, opt);
            writer.Write(outputDllPath);
            Debug.Log($"GeneratePolymorphicDll {originalDllPath} => {outputDllPath}");
        }

        public static void GeneratePolymorphicCodes(string libil2cppDir)
        {
            PolymorphicDllSettings settings = ObfuzSettings.Instance.polymorphicDllSettings;
            if (!settings.enable)
            {
                UnityEngine.Debug.LogWarning("Polymorphic code generation is disabled in Obfuz settings.");
                return;
            }
            var options = new PolymorphicCodeGenerator.Options
            {
                GenerationSecretKey = settings.codeGenerationSecretKey,
                Libil2cppDir = libil2cppDir,
                TemplateDir = ObfuscateUtil.TemplatePathInPackage,
                DisableLoadStandardDll = settings.disableLoadStandardDll,
            };
            var generator = new PolymorphicCodeGenerator(options);
            generator.Generate();
        }
    }
}
