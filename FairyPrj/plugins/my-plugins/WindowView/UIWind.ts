const FairyGUI = CS.FairyGUI
const FairyEditor = CS.FairyEditor
const App = FairyEditor.App;

export class WindData {
    packName: string
    resName: string
    url: string;
    constructor(pkg, res) {
        this.packName = pkg;
        this.resName = res;
        this.url = "ui://" + pkg + "/" + res;
    }
}

export default class UIWind extends CS.FairyGUI.Window {
    private static readonly mWinds = new Map<string, UIWind>();
    private _windData: WindData;

    constructor(data: WindData) {
        super();
        if (data == null) return
        this._windData = data;

        this.__onInit = () => { this.onInit(); }
        this.__onShown = () => { this.onShown(); }
        this.onHide = (() => { this.onHide(); })
    }

    get windData() {
        return this._windData
    }

    private get url() {
        return this._windData.url
    }

    public static add(wind: UIWind) {
        this.mWinds.set(wind.url, wind);
    }


    /**查看路径资源是否创建 */
    public static FindUrl(url: string): UIWind | undefined {
        if (url == null) url = "";
        return this.mWinds.has(url) ? this.mWinds.get(url) : undefined;
    }

    /**界面是否正在被展示 */
    public static isShow(url: string) {
        let wind = this.FindUrl(url);
        if (wind == undefined) return false;
        return wind ? wind.isShowing && wind["_inited"] == true : false;
    }

    /**
* 实例化窗口资源 第一次创建时才会调用
* @returns 
*/
    protected onInit() {
        let self = this;
        App.consoleView.Log("创建组件:" + self.url);

        let windObj = FairyGUI.UIPackage.CreateObject(self.windData.packName, self.windData.resName) as CS.FairyGUI.GComponent;
        if (windObj == null) {
            App.consoleView.Log("创建窗口失败 url:" + self.url);
            return;
        }
        self.contentPane = windObj;
        this.Center();
    }

    protected onShown() {

    }

    protected onHide() {

    }

    //#region  界面显示

    /**
     * 打开面板
     * @param id 面板路径
     * @param param 需求参数
     */
    static show(id: any, param?: any) {
        if (this.mWinds.has(id)) {
            let wind: UIWind = this.mWinds.get(id) as UIWind;
            wind.data = param;
            if (wind.isShowing && wind.contentPane) {
                wind.onShown();
            } else {
                wind.Show();
            }
        } else {
            App.consoleView.Log("显示窗口失败没有注册窗口 id:" + id);
        }
    }


    //#endregion

    public static hide(url: string, param?: any) {
        if (this.mWinds.has(url)) {
            let wind = this.mWinds.get(url) as UIWind;
            if (wind.isShowing) {
                wind.data = param;
                wind.Hide();
            }
        } else {
            App.consoleView.Log("隐藏窗口失败没有注册窗口 id:" + url);
        }
    }

    public static delAll(filter: Array<string>) {
        let needDel = new Array<string>();
        this.mWinds.forEach((v, k) => {
            if (filter == null || filter.findIndex(a => a == v.url) == -1) {
                needDel.push(v.url);
            }
        });

        for (let i = 0; i < needDel.length; i++) {
            this.remove(this.mWinds.get(needDel[i]) as UIWind);
        }
    }

    public static remove(wind: UIWind) {
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