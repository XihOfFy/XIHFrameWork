
const FairyGUI = CS.FairyGUI;
const FairyEditor = CS.FairyEditor
import UIWind, { WindData } from "./UIWind"
import ToolMgr from '../ToolMgr';
const App = FairyEditor.App;

type FailureAssetData = {
    url: string,
    failure: string[],
}

export default class FindFailureAssets extends UIWind {

    constructor() {
        super(new WindData("Extend", "FindFailureAssets"))
    }

    static show() {
        let url = ToolMgr.joinUrl("Extend", "FindFailureAssets")
        if (UIWind.FindUrl(url) == undefined) {
            UIWind.add(new this)
        }

        super.show(url)
    }

    private state: CS.FairyGUI.Controller;
    private check: CS.FairyGUI.GButton
    private progress: CS.FairyGUI.GProgressBar
    private resList: CS.FairyGUI.GList;

    private assets = new Map<string, FindFailureAssets>()
    protected onInit(): void {
        super.onInit();
        this.state = this.contentPane.GetController("state");
        this.check = this.contentPane.GetChild("check").asButton;
        this.progress = this.contentPane.GetChild("progress").asProgress;
        this.resList = this.contentPane.GetChild("resList").asList;

        this.resList.itemRenderer = this.onItemRenderer.bind(this);

        this.resList.onClickItem.Add(this.onListClickItem.bind(this))

        this.check.onClick.Add(this.onCheckClick.bind(this))

        let cancel = this.contentPane.GetChild("close").asButton
        cancel.onClick.Add(this.onCancelClick.bind(this))

        this.resList.SetVirtual();
    }

    protected onShown(): void {
        super.onShown();

    }

    protected onHide(): void {
        super.onHide();
        this.resList.data = null;
        this.resList.numItems = 0;
    }

    private async onCheckClick() {
        if (this.state.selectedIndex == 0) {
            this.state.selectedIndex = 1;
            this.assets.clear();
            let pkgs = App.project.allPackages;
            this.progress.value = 0;

            this.progress.max = ToolMgr.getItemsAll();

            for (let index = 0; index < pkgs.Count; index++) {
                let pkg = pkgs.get_Item(index);
                if (this.state.selectedIndex == 0) break
                for (let j = 0; j < pkg.items.Count; j++) {
                    if (this.state.selectedIndex == 0) break
                    let item = pkg.items.get_Item(j);
                    this.progress.value += 1
                    if (item.type != CS.FairyEditor.FObjectType.COMPONENT) {
                        continue
                    }
                    //@ts-ignore
                    ToolMgr.getFailureAssets(item, this.assets, 0)
                    if (this.progress.value % 30 == 0) {
                        await ToolMgr.sleep(0)
                    }
                }
            }
            App.consoleView.Log("查找完毕")
            this.state.selectedIndex = 0;
            let asset = Array.from(this.assets).map(item => item[1]);
            this.resList.data = asset;
            this.resList.numItems = asset.length;

            this.contentPane.GetChild("projectCount").asTextField.templateVars = ToolMgr.getTemplateVars(['value'],[asset.length.toString()]);

        } else {
            this.state.selectedIndex = 0;
        }
    }

    private onItemRenderer(index: number, obj: CS.FairyGUI.GComponent): void {
        let data = this.resList.data[index] as FailureAssetData;
        obj.data = data;
        let packageItem = App.project.GetItemByURL(data.url)
        obj.GetChild("title").text = packageItem.name

        obj.__onDispose = () => {
            obj.GetChild("title").text = ''
            obj.data = null;
        }
    }

    private onListClickItem(item: CS.FairyGUI.EventContext): void {
        let data = item.data.data as FailureAssetData;
        let packageItem = App.project.GetItemByURL(data.url)
        App.consoleView.Log(ToolMgr.getUBBUrl(packageItem.GetURL(),'失效组件'))
        for (let index = 0; index < data.failure.length; index++) {
            App.consoleView.Log('失效URL:'+data.failure[index])
        }
    }

    private onCancelClick() {
        let self = this;
        this.Hide();
        FairyGUI.Timers.inst.Add(0.05, 1, () => {
            UIWind.remove(self)
        })
    }
}

