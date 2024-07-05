"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const FairyGUI = CS.FairyGUI;
const FairyEditor = CS.FairyEditor;
const UIWind_1 = require("./UIWind");
const ToolMgr_1 = require("../ToolMgr");
const SG_1 = require("../SG");
const App = FairyEditor.App;
class CustomPlugin extends UIWind_1.default {
    constructor() {
        super(new UIWind_1.WindData("Extend", "CustomPlugin"));
    }
    static show() {
        let url = ToolMgr_1.default.joinUrl("Extend", "CustomPlugin");
        if (UIWind_1.default.FindUrl(url) == undefined) {
            UIWind_1.default.add(new this);
        }
        super.show(url);
    }
    onInit() {
        super.onInit();
        let basic = this.contentPane.GetChild("basic").asCom;
        this.dependency = basic.GetChild("dependency").asButton;
        this.copyAttribute = basic.GetChild("copyAttribute").asButton;
        this.openXYWHCompoter = basic.GetChild("openXYWHCompoter").asButton;
        this.clearOnPublish = basic.GetChild("clearOnPublish").asButton;
        this.openCustomName = basic.GetChild("openCustomName").asButton;
        let ok = this.contentPane.GetChild("ok").asButton;
        let cancel = this.contentPane.GetChild("cancel").asButton;
        ok.onClick.Add(this.onOKClick.bind(this));
        cancel.onClick.Add(this.onCancelClick.bind(this));
    }
    onShown() {
        super.onShown();
        this.dependency.selected = SG_1.default.config[SG_1.Config.Dependency];
        this.copyAttribute.selected = SG_1.default.config[SG_1.Config.CopyAttribute];
        this.openXYWHCompoter.selected = SG_1.default.config[SG_1.Config.XYWHComputer];
        this.clearOnPublish.selected = SG_1.default.config[SG_1.Config.ClearOnPublish];
        this.openCustomName.selected = SG_1.default.config[SG_1.Config.OPENCUSTOMNAME];
    }
    onHide() {
        super.onHide();
    }
    onOKClick() {
        SG_1.default.config[SG_1.Config.Dependency] = this.dependency.selected;
        SG_1.default.config[SG_1.Config.CopyAttribute] = this.copyAttribute.selected;
        SG_1.default.config[SG_1.Config.XYWHComputer] = this.openXYWHCompoter.selected;
        SG_1.default.config[SG_1.Config.ClearOnPublish] = this.clearOnPublish.selected;
        SG_1.default.config[SG_1.Config.OPENCUSTOMNAME] = this.openCustomName.selected;
        ToolMgr_1.default.saveConfig(SG_1.default.config_path);
        this.onCancelClick();
    }
    onCancelClick() {
        let self = this;
        this.Hide();
        FairyGUI.Timers.inst.Add(0.05, 1, () => {
            UIWind_1.default.remove(self);
        });
    }
}
exports.default = CustomPlugin;
