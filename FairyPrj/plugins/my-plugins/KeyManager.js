"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const UnityEngine = CS.UnityEngine;
const LookText_1 = require("./ToolPlugin/LookText");
class KeyManager {
    static onKeyDown(evt) {
        let input = evt.data;
        if (input.ctrl && input.alt) {
            //#region  小说
            if (input.keyCode == UnityEngine.KeyCode.P) {
                //第一次打开 第二次关闭
                LookText_1.default.open();
            }
            else if (input.keyCode == UnityEngine.KeyCode.Period) {
                LookText_1.default.nextPage();
                //下一页
            }
            else if (input.keyCode == UnityEngine.KeyCode.Comma) {
                LookText_1.default.lastPage();
                //上一页
            }
        }
        //#endregion
    }
}
exports.default = KeyManager;
