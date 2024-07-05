"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const FairyGUI = CS.FairyGUI;
const FairyEditor = CS.FairyEditor;
const App = FairyEditor.App;
class WindData {
    constructor(pkg, res) {
        this.packName = pkg;
        this.resName = res;
        this.url = "ui://" + pkg + "/" + res;
    }
}
exports.WindData = WindData;
class UIWind extends CS.FairyGUI.Window {
    constructor(data) {
        super();
        if (data == null)
            return;
        this._windData = data;
        this.__onInit = () => { this.onInit(); };
        this.__onShown = () => { this.onShown(); };
        this.onHide = (() => { this.onHide(); });
    }
    get windData() {
        return this._windData;
    }
    get url() {
        return this._windData.url;
    }
    static add(wind) {
        this.mWinds.set(wind.url, wind);
    }
    /**查看路径资源是否创建 */
    static FindUrl(url) {
        if (url == null)
            url = "";
        return this.mWinds.has(url) ? this.mWinds.get(url) : undefined;
    }
    /**界面是否正在被展示 */
    static isShow(url) {
        let wind = this.FindUrl(url);
        if (wind == undefined)
            return false;
        return wind ? wind.isShowing && wind["_inited"] == true : false;
    }
    /**
* 实例化窗口资源 第一次创建时才会调用
* @returns
*/
    onInit() {
        let self = this;
        App.consoleView.Log("创建组件:" + self.url);
        let windObj = FairyGUI.UIPackage.CreateObject(self.windData.packName, self.windData.resName);
        if (windObj == null) {
            App.consoleView.Log("创建窗口失败 url:" + self.url);
            return;
        }
        self.contentPane = windObj;
        this.Center();
    }
    onShown() {
    }
    onHide() {
    }
    //#region  界面显示
    /**
     * 打开面板
     * @param id 面板路径
     * @param param 需求参数
     */
    static show(id, param) {
        if (this.mWinds.has(id)) {
            let wind = this.mWinds.get(id);
            wind.data = param;
            if (wind.isShowing && wind.contentPane) {
                wind.onShown();
            }
            else {
                wind.Show();
            }
        }
        else {
            App.consoleView.Log("显示窗口失败没有注册窗口 id:" + id);
        }
    }
    //#endregion
    static hide(url, param) {
        if (this.mWinds.has(url)) {
            let wind = this.mWinds.get(url);
            if (wind.isShowing) {
                wind.data = param;
                wind.Hide();
            }
        }
        else {
            App.consoleView.Log("隐藏窗口失败没有注册窗口 id:" + url);
        }
    }
    static delAll(filter) {
        let needDel = new Array();
        this.mWinds.forEach((v, k) => {
            if (filter == null || filter.findIndex(a => a == v.url) == -1) {
                needDel.push(v.url);
            }
        });
        for (let i = 0; i < needDel.length; i++) {
            this.remove(this.mWinds.get(needDel[i]));
        }
    }
    static remove(wind) {
        if (this.mWinds.has(wind.url)) {
            this.mWinds.delete(wind.url);
            App.consoleView.Log("卸载窗口:" + wind.url);
            wind._windData = null;
            wind.RemoveFromParent();
            wind.contentPane.Dispose();
            wind.contentPane = null;
            wind.Dispose();
            wind = null;
        }
    }
}
exports.default = UIWind;
UIWind.mWinds = new Map();
