﻿using Obfuz.EncryptionVM;
using Obfuz.ObfusPasses;
using Obfuz.ObfusPasses.CallObfus;
using Obfuz.ObfusPasses.ConstEncrypt;
using Obfuz.ObfusPasses.ControlFlowObfus;
using Obfuz.ObfusPasses.EvalStackObfus;
using Obfuz.ObfusPasses.ExprObfus;
using Obfuz.ObfusPasses.FieldEncrypt;
using Obfuz.ObfusPasses.SymbolObfus;
using Obfuz.Settings;
using Obfuz.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Obfuz
{

    public class CoreSettingsFacade
    {
        public BuildTarget buildTarget;

        public byte[] defaultStaticSecretKey;
        public byte[] defaultDynamicSecretKey;
        public List<string> assembliesUsingDynamicSecretKeys;
        public int randomSeed;

        public string encryptionVmGenerationSecretKey;
        public int encryptionVmOpCodeCount;
        public string encryptionVmCodeFile;

        public List<string> assembliesToObfuscate;
        public List<string> nonObfuscatedButReferencingObfuscatedAssemblies;
        public List<string> assemblySearchPaths;
        public string obfuscatedAssemblyOutputPath;
        public string obfuscatedAssemblyTempOutputPath;

        public ObfuscationPassType enabledObfuscationPasses;
        public List<string> obfuscationPassRuleConfigFiles;
        public List<IObfuscationPass> obfuscationPasses;
    }

    public class ObfuscatorBuilder
    {
        private CoreSettingsFacade _coreSettingsFacade;

        public CoreSettingsFacade CoreSettingsFacade => _coreSettingsFacade;

        public void InsertTopPriorityAssemblySearchPaths(List<string> assemblySearchPaths)
        {
            _coreSettingsFacade.assemblySearchPaths.InsertRange(0, assemblySearchPaths);
        }

        public ObfuscatorBuilder AddPass(IObfuscationPass pass)
        {
            _coreSettingsFacade.obfuscationPasses.Add(pass);
            return this;
        }

        public Obfuscator Build()
        {
            return new Obfuscator(this);
        }

        public static List<string> BuildUnityAssemblySearchPaths(bool searchPathIncludeUnityEditorDll = false)
        {
            string applicationContentsPath = EditorApplication.applicationContentsPath;
            var searchPaths = new List<string>
                {
#if UNITY_2021_1_OR_NEWER
#if UNITY_STANDALONE_WIN || (UNITY_EDITOR_WIN && UNITY_SERVER) || UNITY_WSA || UNITY_LUMIN
                "MonoBleedingEdge/lib/mono/unityaot-win32",
                "MonoBleedingEdge/lib/mono/unityaot-win32/Facades",
#elif UNITY_STANDALONE_OSX || (UNITY_EDITOR_OSX && UNITY_SERVER) || UNITY_IOS || UNITY_TVOS
                "MonoBleedingEdge/lib/mono/unityaot-macos",
                "MonoBleedingEdge/lib/mono/unityaot-macos/Facades",
#else
                "MonoBleedingEdge/lib/mono/unityaot-linux",
                "MonoBleedingEdge/lib/mono/unityaot-linux/Facades",
#endif
#else
                "MonoBleedingEdge/lib/mono/unityaot",
                "MonoBleedingEdge/lib/mono/unityaot/Facades",
#endif

#if UNITY_STANDALONE_WIN || (UNITY_EDITOR_WIN && UNITY_SERVER)
                "PlaybackEngines/windowsstandalonesupport/Variations/il2cpp/Managed",
#elif UNITY_STANDALONE_OSX || (UNITY_EDITOR_OSX && UNITY_SERVER)
                "PlaybackEngines/MacStandaloneSupport/Variations/il2cpp/Managed",
#elif UNITY_STANDALONE_LINUX || (UNITY_EDITOR_LINUX && UNITY_SERVER)
                "PlaybackEngines/LinuxStandaloneSupport/Variations/il2cpp/Managed",
#elif UNITY_ANDROID
                "PlaybackEngines/AndroidPlayer/Variations/il2cpp/Managed",
#elif UNITY_IOS
                "PlaybackEngines/iOSSupport/Variations/il2cpp/Managed",
#elif UNITY_MINIGAME || UNITY_WEIXINMINIGAME
#if TUANJIE_1_1_OR_NEWER
                "PlaybackEngines/WeixinMiniGameSupport/Variations/il2cpp/nondevelopment/Data/Managed",
#else
                "PlaybackEngines/WeixinMiniGameSupport/Variations/nondevelopment/Data/Managed",
#endif
#elif UNITY_OPENHARMONY
                "PlaybackEngines/OpenHarmonyPlayer/Variations/il2cpp/Managed",
#elif UNITY_WEBGL
                "PlaybackEngines/WebGLSupport/Variations/nondevelopment/Data/Managed",
#elif UNITY_TVOS
                "PlaybackEngines/AppleTVSupport/Variations/il2cpp/Managed",
#elif UNITY_WSA
                "PlaybackEngines/WSASupport/Variations/il2cpp/Managed",
#elif UNITY_LUMIN
                "PlaybackEngines/LuminSupport/Variations/il2cpp/Managed",
#else
#error "Unsupported platform, please report to us"
#endif
                };

            if (searchPathIncludeUnityEditorDll)
            {
                searchPaths.Add("Managed/UnityEngine");
            }

                var resultPaths = new List<string>();
            foreach (var path in searchPaths)
            {
                string candidatePath1 = Path.Combine(applicationContentsPath, path);
                if (Directory.Exists(candidatePath1))
                {
                    resultPaths.Add(candidatePath1);
                }
                if (path.StartsWith("PlaybackEngines"))
                {
                    string candidatePath2 = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(applicationContentsPath)), path);
                    if (Directory.Exists(candidatePath2))
                    {
                        resultPaths.Add(candidatePath2);
                    }
                }
            }
            return resultPaths;
        }

        public static ObfuscatorBuilder FromObfuzSettings(ObfuzSettings settings, BuildTarget target, bool searchPathIncludeUnityEditorInstallLocation, bool searchPathIncludeUnityEditorDll = false)
        {
            List<string> searchPaths = searchPathIncludeUnityEditorInstallLocation ?
                BuildUnityAssemblySearchPaths(searchPathIncludeUnityEditorDll).Concat(settings.assemblySettings.additionalAssemblySearchPaths).ToList()
                : settings.assemblySettings.additionalAssemblySearchPaths.ToList();
            foreach (var path in searchPaths)
            {
                bool exists = Directory.Exists(path);
                UnityEngine.Debug.Log($"search path:{path} exists:{exists}");
            }
            var builder = new ObfuscatorBuilder
            {
                _coreSettingsFacade = new CoreSettingsFacade()
                {
                    buildTarget = target,
                    defaultStaticSecretKey = KeyGenerator.GenerateKey(settings.secretSettings.defaultStaticSecretKey, VirtualMachine.SecretKeyLength),
                    defaultDynamicSecretKey = KeyGenerator.GenerateKey(settings.secretSettings.defaultDynamicSecretKey, VirtualMachine.SecretKeyLength),
                    assembliesUsingDynamicSecretKeys = settings.secretSettings.assembliesUsingDynamicSecretKeys.ToList(),
                    randomSeed = settings.secretSettings.randomSeed,
                    encryptionVmGenerationSecretKey = settings.encryptionVMSettings.codeGenerationSecretKey,
                    encryptionVmOpCodeCount = settings.encryptionVMSettings.encryptionOpCodeCount,
                    encryptionVmCodeFile = settings.encryptionVMSettings.codeOutputPath,
                    assembliesToObfuscate = settings.assemblySettings.GetAssembliesToObfuscate(),
                    nonObfuscatedButReferencingObfuscatedAssemblies = settings.assemblySettings.nonObfuscatedButReferencingObfuscatedAssemblies.ToList(),
                    assemblySearchPaths = searchPaths,
                    obfuscatedAssemblyOutputPath = settings.GetObfuscatedAssemblyOutputPath(target),
                    obfuscatedAssemblyTempOutputPath = settings.GetObfuscatedAssemblyTempOutputPath(target),
                    enabledObfuscationPasses = settings.obfuscationPassSettings.enabledPasses,
                    obfuscationPassRuleConfigFiles = settings.obfuscationPassSettings.ruleFiles?.ToList() ?? new List<string>(),
                    obfuscationPasses = new List<IObfuscationPass>(),
                },
            };
            ObfuscationPassType obfuscationPasses = settings.obfuscationPassSettings.enabledPasses;
            if (obfuscationPasses.HasFlag(ObfuscationPassType.ConstEncrypt))
            {
                builder.AddPass(new ConstEncryptPass(settings.constEncryptSettings.ToFacade()));
            }
            if (obfuscationPasses.HasFlag(ObfuscationPassType.ExprObfus))
            {
                builder.AddPass(new ExprObfusPass(settings.exprObfusSettings.ToFacade()));
            }
            if (obfuscationPasses.HasFlag(ObfuscationPassType.EvalStackObfus))
            {
                builder.AddPass(new EvalStackObfusPass(settings.evalStackObfusSettings.ToFacade()));
            }
            if (obfuscationPasses.HasFlag(ObfuscationPassType.FieldEncrypt))
            {
                builder.AddPass(new FieldEncryptPass(settings.fieldEncryptSettings.ToFacade()));
            }
            if (obfuscationPasses.HasFlag(ObfuscationPassType.CallObfus))
            {
                builder.AddPass(new CallObfusPass(settings.callObfusSettings.ToFacade()));
            }
            if (obfuscationPasses.HasFlag(ObfuscationPassType.ControlFlowObfus))
            {
                builder.AddPass(new ControlFlowObfusPass(settings.controlFlowObfusSettings.ToFacade()));
            }
            if (obfuscationPasses.HasFlag(ObfuscationPassType.SymbolObfus))
            {
                builder.AddPass(new SymbolObfusPass(settings.symbolObfusSettings.ToFacade()));
            }
            return builder;
        }
    }
}
