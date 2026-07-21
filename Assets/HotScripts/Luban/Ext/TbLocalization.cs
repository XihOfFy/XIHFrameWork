using System.Collections.Generic;
using System.Linq;
using Aot;
using Aot2Hot;
using FairyGUI;
using UnityEngine;

namespace Tmpl
{
    /// <summary>
    /// 延迟翻译值，args 中的嵌套 Translate 会保留 id 信息，切换语言时可递归重新翻译。
    /// 例如 1001.Translate(1002.Translate(2001.Translate(2002.Translate("noTranslate..."))),1003.Translate(),"noTranslate...")
    /// </summary>
    public readonly struct TrsValue
    {
        readonly LocalizationCfg cfg;
        readonly object[] args;
        TrsValue(int id, object[] args) : this(Tables.Instance.TbLocalization.GetOrDefault(id), args)
        {
#if UNITY_EDITOR || USE_GM
            if (cfg == null)
            {
                Debug.LogError($"LocalizationCfg not found Id {id}");
            }
#endif
        }
        TrsValue(LocalizationCfg cfg, object[] args)
        {
            this.cfg = cfg;
            this.args = args;
        }

        public static TrsValue FromId(int id, params object[] args) => new TrsValue(id, args);

        public string Resolve()
        {
            if (cfg == null)
            {
#if UNITY_EDITOR || USE_GM
                Debug.LogError($"LocalizationCfg not found");
#endif
                return "";
            }
            return cfg.Translate(args);
        }
        //TODO 删除后是否满足自动转换string，没必要做这个转换
        // public static object[] ResolveArgs(object[] rawArgs)
        // {
        //     if (rawArgs == null || rawArgs.Length == 0) return rawArgs;
        //     var resolved = new object[rawArgs.Length];
        //     for (var i = 0; i < rawArgs.Length; i++)
        //     {
        //         resolved[i] = rawArgs[i] is TrsValue tv ? tv.Resolve() : rawArgs[i];
        //     }
        //     return resolved;
        // }
        public static implicit operator string(TrsValue value) => value.Resolve();
        public override string ToString() => Resolve();
    }

    /// <summary>
    /// 绑定 UI 控件的翻译信息，切换语言时会重新翻译。
    /// </summary>
    struct TrsInfo
    {
        public TMPro.TMP_Text tmpTrs;
        public GTextField fguiTrs;
        object[] args;
        LocalizationCfg cfg;
        public TrsInfo(LocalizationCfg cfg, object[] args)
        {
            this.cfg = cfg;
            this.args = args;
            this.tmpTrs = null;
            this.fguiTrs = null;
        }
        public TrsInfo(LocalizationCfg cfg, TMPro.TMP_Text tmpTrs, object[] args) : this(cfg, args)
        {
            this.tmpTrs = tmpTrs;
            Translate();
        }
        public TrsInfo(LocalizationCfg cfg, GTextField fguiTrs, object[] args) : this(cfg, args)
        {
            this.fguiTrs = fguiTrs;
            Translate();
        }
        public TrsInfo(int trsId, object[] args) : this(Tables.Instance.TbLocalization.GetOrDefault(trsId), args)
        {
#if UNITY_EDITOR || USE_GM
            if (cfg == null)
            {
                Debug.LogError($"LocalizationCfg not found Id {trsId}");
            }
#endif
        }
        public TrsInfo(int trsId, TMPro.TMP_Text tmpTrs, object[] args) : this(trsId, args)
        {
            this.tmpTrs = tmpTrs;
            Translate();
        }
        public TrsInfo(int trsId, GTextField fguiTrs, object[] args) : this(trsId, args)
        {
            this.fguiTrs = fguiTrs;
            Translate();
        }
        public readonly void Translate()
        {
            if (tmpTrs) tmpTrs.text = cfg.Translate(args);
            if (fguiTrs != null && !fguiTrs.isDisposed) fguiTrs.SetText(cfg.Translate(args));
        }
        public readonly bool Validate()
        {
            return tmpTrs || (fguiTrs != null && !fguiTrs.isDisposed);
        }
    }
    /// <summary>
    /// 对于args 中的嵌套翻译，使用方式如下：
    ///TextField.Translate(mainId，"noTranslate..."，subId0_0.Translate(subId0_1.Translate("noTranslate...")),subId1_0.Translate("noTranslate..."));
    /// </summary>
    public static class LocalizationExt
    {
        static readonly Dictionary<object, TrsInfo> objCache = new Dictionary<object, TrsInfo>(512);
        public static void Translate(this TMPro.TMP_Text trs, int id, params object[] args)
        {
            if (!trs)
            {
#if UNITY_EDITOR || USE_GM
                Debug.LogError($"TMP_Text is disposed {trs?.name} {id}");
#endif
                return;
            }
            var trsInfo = new TrsInfo(id, trs, args);
            objCache[trs] = trsInfo;
        }
        public static void Translate(this TMPro.TMP_Text trs, LocalizationCfg cfg, params object[] args)
        {
            if (!trs)
            {
#if UNITY_EDITOR || USE_GM
                Debug.LogError($"TMP_Text is disposed {trs?.name} {cfg?.ID}");
#endif
                return;
            }
            var trsInfo = new TrsInfo(cfg, trs, args);
            objCache[trs] = trsInfo;
        }
        public static void Translate(this GTextField trs, int id, params object[] args)
        {
            if (trs == null || trs.isDisposed)
            {
#if UNITY_EDITOR || USE_GM
                Debug.LogError($"GTextField is disposed {trs?.name} {id}");
#endif
                return;
            }
            var trsInfo = new TrsInfo(id, trs, args);
            objCache[trs] = trsInfo;
        }
        public static void Translate(this GTextField trs, LocalizationCfg cfg, params object[] args)
        {
            if (trs == null || trs.isDisposed)
            {
#if UNITY_EDITOR || USE_GM
                Debug.LogError($"GTextField is disposed {trs?.name} {cfg?.ID}");
#endif
                return;
            }
            var trsInfo = new TrsInfo(cfg, trs, args);
            objCache[trs] = trsInfo;
        }
        // 直接翻译 id；返回 TrsValue 以支持嵌套 args，可隐式转为 string
        public static TrsValue Translate(this int id, params object[] args) => TrsValue.FromId(id, args);
        public static string Translate(this LocalizationCfg cfg, params object[] args)
        {
            if (cfg == null)
            {
#if UNITY_EDITOR || USE_GM
                Debug.LogError($"LocalizationCfg not found");
#endif
                return "";
            }
            //var resolvedArgs = TrsValue.ResolveArgs(args);
            var resolvedArgs = args;
            switch (AotConfig.Language)
            {
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional:
                    return string.Format(cfg.Zh, resolvedArgs);
                // case SystemLanguage.Japanese:
                //     return string.Format(cfg.Ja, resolvedArgs);
                default:
                    return string.Format(cfg.En, resolvedArgs);
            }
        }
        public static void ChangeLanguageBySystem()
        {
            var language = AotConfig.Language;
#if (UNITY_DY || UNITY_TT)
            try
            {
                //https://bytedance.larkoffice.com/wiki/Q86awaAM1iJUoskbYsIceXIMnld?from=from_parent_docx
                var systemInfo = TTSDK.TT.GetSystemInfo();
                if (systemInfo.language == "ja-JP")
                {
                    language = SystemLanguage.Japanese;
                }
                else if (systemInfo.language == "zh-Hant-TW" || systemInfo.language == "zh-Hans")
                {
                    language = SystemLanguage.Chinese;
                }
                else{
                    language = SystemLanguage.English;
                }
                Debug.LogWarning($"SetLanguageId success:{language} {systemInfo.language}");
            }
            catch
            {
                    language = SystemLanguage.English;
            }
#else
            language = Application.systemLanguage;
#endif
            ChangeLanguage(language);
        }
        public static void ChangeLanguage(SystemLanguage language)
        {
            if (AotConfig.Language == language) return;
            AotConfig.Language = language;
            var keys = objCache.Keys.ToList();
            foreach (var key in keys)
            {
                var value = objCache[key];
                if (value.Validate())
                {
                    value.Translate();
                }
                else
                {
                    objCache.Remove(key);
                }
            }
        }
        //可以主动调用，例如切换场景，其中切换语言会自动处理
        //为避免长时间累积，可以每次关闭界面或间隔半分钟调用一次，类似AB延迟清理
        public static void ClearTrsInfoCache()
        {
            var keys = objCache.Keys.ToList();
            foreach (var key in keys)
            {
                var value = objCache[key];
                if (!value.Validate())
                {
                    objCache.Remove(key);
                }
            }
        }
    }
}
