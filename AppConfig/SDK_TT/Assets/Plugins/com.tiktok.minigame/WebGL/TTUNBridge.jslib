mergeInto(LibraryManager.library, {
  // ============================================================
  // StarkSafeProxy: Wraps window.StarkSDK in a recursive Proxy
  // so ALL method calls are automatically error-safe.
  // No per-function try-catch needed in any jslib file.
  // ============================================================
  $StarkSafeProxy__postset: "StarkSafeProxy();",
  $StarkSafeProxy: function () {
    if (typeof Proxy === "undefined") return;

    function makeSafe(target, path) {
      return new Proxy(target, {
        get: function (t, prop) {
          if (typeof prop === "symbol") return t[prop];
          var val;
          try {
            val = t[prop];
          } catch (e) {
            return undefined;
          }
          if (typeof val === "function") {
            return function () {
              try {
                return val.apply(t, arguments);
              } catch (e) {
                console.error(
                  "[" + path + "." + prop + "] error:",
                  e.message || e
                );
              }
            };
          }
          if (val != null && typeof val === "object") {
            return makeSafe(val, path + "." + prop);
          }
          return val;
        },
        set: function (t, prop, v) {
          t[prop] = v;
          return true;
        },
      });
    }

    var _raw = window.StarkSDK || null;
    var _safe = _raw ? makeSafe(_raw, "StarkSDK") : null;
    Object.defineProperty(window, "StarkSDK", {
      get: function () {
        return _safe;
      },
      set: function (v) {
        _raw = v;
        _safe = v ? makeSafe(v, "StarkSDK") : v;
      },
      configurable: true,
      enumerable: true,
    });
  },

  // ============================================================
  // Bridge utilities & functions
  // ============================================================
  StarkPointerStringify__deps: ["$StarkSafeProxy"],
  StarkPointerStringify: function (str) {
    if (typeof UTF8ToString !== "undefined") {
      return UTF8ToString(str);
    }
    return Pointer_stringify(str);
  },
  unityCallJs: function (msg) {
    if (typeof UNBridgeCore === "undefined") return;
    try {
      UNBridgeCore.handleMsgFromUnity(_StarkPointerStringify(msg));
    } catch (e) {
      console.error("[TTUNBridge] unityCallJs error:", e.message || e);
    }
  },
  unityCallJsSync: function (msg) {
    if (typeof UNBridgeCore === "undefined") return 0;
    try {
      var result = UNBridgeCore.handleMsgFromUnitySync(
        _StarkPointerStringify(msg)
      );
      if (!result) return 0;
      var bufferSize = lengthBytesUTF8(result) + 1;
      var buffer = _malloc(bufferSize);
      stringToUTF8(result, buffer, bufferSize);
      return buffer;
    } catch (e) {
      console.error("[TTUNBridge] unityCallJsSync error:", e.message || e);
      return 0;
    }
  },
  h5HasAPI: function (apiName) {
    try {
      if (typeof UNBridgeCore === "undefined") return false;
      return UNBridge.h5HasAPI(_StarkPointerStringify(apiName));
    } catch (e) {
      console.error("[TTUNBridge] h5HasAPI error:", e.message || e);
      return false;
    }
  },
  unityMixCallJs: function (msg) {
    if (typeof UNBridgeCore === "undefined") return 0;
    try {
      var result = UNBridgeCore.onUnityMixCall(_StarkPointerStringify(msg));
      if (!result) return 0;
      var bufferSize = lengthBytesUTF8(result) + 1;
      var buffer = _malloc(bufferSize);
      stringToUTF8(result, buffer, bufferSize);
      return buffer;
    } catch (e) {
      console.error("[TTUNBridge] unityMixCallJs error:", e.message || e);
      return 0;
    }
  },
});
