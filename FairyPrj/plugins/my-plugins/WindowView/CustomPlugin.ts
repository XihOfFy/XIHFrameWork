
const FairyGUI = CS.FairyGUI;
const FairyEditor = CS.FairyEditor
import UIWind, { WindData } from "./UIWind"
import ToolMgr from '../ToolMgr';
import SG, { Config } from '../SG';
const App = FairyEditor.App;

export default class CustomPlugin extends UIWind {

    constructor() {
        super(new WindData("Extend", "CustomPlugin"))
    }

    static show() {
        let url = ToolMgr.joinUrl("Extend", "CustomPlugin")
        if (UIWind.FindUrl(url) == undefined) {
            UIWind.add(new this)
        }

        super.show(url)
    }


    private dependency: CS.FairyGUI.GButton

    private copyAttribute: CS.FairyGUI.GButton;

    private openXYWHCompoter: CS.FairyGUI.GButton;

    private clearOnPublish: CS.FairyGUI.GButton;

    private openCustomName: CS.FairyGUI.GButton;

    protected onInit(): void {
        super.onInit();
        let basic = this.contentPane.GetChild("basic").asCom
        this.dependency = basic.GetChild("dependency").asButton
        this.copyAttribute = basic.GetChild("copyAttribute").asButton
        this.openXYWHCompoter = basic.GetChild("openXYWHCompoter").asButton
        this.clearOnPublish = basic.GetChild("clearOnPublish").asButton
        this.openCustomName = basic.GetChild("openCustomName").asButton

        let ok = this.contentPane.GetChild("ok").asButton;
        let cancel = this.contentPane.GetChild("cancel").asButton
        ok.onClick.Add(this.onOKClick.bind(this))
        cancel.onClick.Add(this.onCancelClick.bind(this))
    }

    protected onShown(): void {
        super.onShown();
        this.dependency.selected = SG.config[Config.Dependency]

        this.copyAttribute.selected = SG.config[Config.CopyAttribute]

        this.openXYWHCompoter.selected = SG.config[Config.XYWHComputer]

        this.clearOnPublish.selected = SG.config[Config.ClearOnPublish]
        this.openCustomName.selected = SG.config[Config.OPENCUSTOMNAME]
    }

    protected onHide(): void {
        super.onHide();
    }

    private onOKClick() {

        SG.config[Config.Dependency] = this.dependency.selected;
        SG.config[Config.CopyAttribute] = this.copyAttribute.selected;
        SG.config[Config.XYWHComputer] = this.openXYWHCompoter.selected;
        SG.config[Config.ClearOnPublish] = this.clearOnPublish.selected;
        SG.config[Config.OPENCUSTOMNAME] = this.openCustomName.selected;

        ToolMgr.saveConfig(SG.config_path)

        this.onCancelClick();
    }

    private onCancelClick() {
        let self = this;
        this.Hide();
        FairyGUI.Timers.inst.Add(0.05, 1, () => {
            UIWind.remove(self)
        })
    }
}

