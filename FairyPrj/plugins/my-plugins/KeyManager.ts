const UnityEngine = CS.UnityEngine
import LookText from "./ToolPlugin/LookText"
export default class KeyManager {
    static onKeyDown(evt: CS.FairyGUI.EventContext) {
        let input = evt.data as CS.FairyGUI.InputEvent;
        if (input.ctrl && input.alt) {
            //#region  小说
            if (input.keyCode == UnityEngine.KeyCode.P) {
                //第一次打开 第二次关闭
                LookText.open();
            }
            else if (input.keyCode == UnityEngine.KeyCode.Period) {
                LookText.nextPage();
                //下一页
            }
            else if (input.keyCode == UnityEngine.KeyCode.Comma) {
                LookText.lastPage();
                //上一页
            }
        }

        //#endregion
    }
}