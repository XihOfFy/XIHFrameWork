mergeInto(LibraryManager.library, {
    StarkAbfsCheckReady: function() {
        if (!window.StarkSDK || !window.StarkSDK.abfs)
            return false;
        return window.StarkSDK.abfs.checkReady();
    },
    StarkAbfsRegisterAssetBundleUrl: function(ptr) {
        if (!window.StarkSDK) return;
        var url = UTF8ToString(ptr);
        window.StarkSDK.abfs.registerAssetBundleUrl(url);
    },
    StarkAbfsUnregisterAssetBundleUrl: function(ptr) {
        if (!window.StarkSDK) return;
        var url = UTF8ToString(ptr);
        window.StarkSDK.abfs.unregisterAssetBundleUrl(url);
    },
    StarkAbfsFetchBundleFromXHR: function(url, id, callback, needRetry) {
        if (!window.StarkSDK) return false;
        // [V2 兼容性修复] 检测到 TikTok 海外 & 抖音国内当前 unity-plugin (ttmg-fe) 都未实现
        // window.StarkSDK.abfs.fetchBundleFromXHR，直接调用会 throw "is not a function" → 整个 wasm crash。
        // 这里检测不存在时通过 callback 返回错误，让 C# 侧 (TTAssetBundleRequest.Callback) 走失败分支，
        // 不污染主流程。等未来容器实现了此 API 自动恢复工作，无需改动。
        if (!window.StarkSDK.abfs || typeof window.StarkSDK.abfs.fetchBundleFromXHR !== 'function') {
            console.warn('[TTAssetBundle] window.StarkSDK.abfs.fetchBundleFromXHR is not implemented in this container. ' +
                         'Falling back to error callback. Addressables ABFS Provider will not work here.');
            return false;
        }
        var _url = UTF8ToString(url);
        var _id = UTF8ToString(id);
        var _callback = function(code, message) {
            // [V2 B.9] 修复 _malloc 泄漏：dynCall 是同步调用，C# 侧 Callback 在 PtrToStringAuto
            // 之后即不再持有 idPtr。在 try/finally 里立即 _free，避免每次 AB XHR 完成都泄漏
            // 一段 WASM 堆 (len = id 字符串 UTF-8 长度 + 1，多 AB 场景累积 MB 级)。
            var idPtr = 0;
            try {
                var len = lengthBytesUTF8(_id) + 1;
                idPtr = _malloc(len);
                stringToUTF8(_id, idPtr, len);
                dynCall("viii", callback, [idPtr, code, 0]);
            } catch (e) {
                console.error("[TTAssetBundle] fetch callback error:", e.message || e);
            } finally {
                if (idPtr) _free(idPtr);
            }
        }
        window.StarkSDK.abfs.fetchBundleFromXHR(_url, _id, _callback, needRetry);
    }
});
