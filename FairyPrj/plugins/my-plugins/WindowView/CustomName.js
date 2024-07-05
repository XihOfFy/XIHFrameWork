"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const FairyGUI = CS.FairyGUI;
const FairyEditor = CS.FairyEditor;
const UIWind_1 = require("./UIWind");
const ToolMgr_1 = require("../ToolMgr");
const SG_1 = require("../SG");
class CustomName extends UIWind_1.default {
    constructor() {
        super(new UIWind_1.WindData("Extend", "CustomName"));
    }
    static show() {
        let url = ToolMgr_1.default.joinUrl("Extend", "CustomName");
        if (UIWind_1.default.FindUrl(url) == undefined) {
            UIWind_1.default.add(new this);
        }
        super.show(url);
    }
    onInit() {
        super.onInit();
        this.list = this.contentPane.GetChild("list").asList;
        this.list.itemRenderer = this.onItemRenerer.bind(this);
        let ok = this.contentPane.GetChild("ok").asButton;
        let cancel = this.contentPane.GetChild("cancel").asButton;
        ok.onClick.Add(this.onOKClick.bind(this));
        cancel.onClick.Add(this.onCancelClick.bind(this));
        this.contentPane.GetChild("reset").asButton.onClick.Add(this.onResetClick.bind(this));
    }
    onShown() {
        super.onShown();
        if (SG_1.default.config[SG_1.Config.CUSTOMNAME] == null) {
            this.initAllTag();
        }
        let tags = SG_1.default.config[SG_1.Config.CUSTOMNAME];
        this.list.data = tags;
        this.list.numItems = 20;
    }
    onHide() {
        super.onHide();
        this.list.data = null;
        this.list.numItems = 0;
    }
    onItemRenerer(index, obj) {
        let data = this.list.data[index];
        obj.GetChild("title").asTextField.text = (index + 1).toString();
        obj.GetChild("input").asLabel.GetChild("title").asTextInput.inputTextField.text = data ? data : "";
    }
    onOKClick() {
        let array = new Array();
        for (let index = 0; index < this.list.numItems; index++) {
            let item = this.list.GetChildAt(index).asCom;
            let text = item.GetChild("input").asLabel.GetChild("title").asTextInput.inputTextField.text;
            if (text && text != "") {
                array.push(text);
            }
        }
        SG_1.default.config[SG_1.Config.CUSTOMNAME] = array;
        ToolMgr_1.default.saveConfig(SG_1.default.config_path);
        this.onCancelClick();
    }
    onResetClick() {
        this.list.numItems = 0;
        SG_1.default.config[SG_1.Config.CUSTOMNAME] = null;
        this.onShown();
    }
    initAllTag() {
        SG_1.default.config[SG_1.Config.CUSTOMNAME] = ['title', 'icon', 'bar', 'bar_v', 'grip', 'arrow1', 'arrow2', 'ani', 'list', 'closeButton', 'dragArea', 'contentArea'];
    }
    onCancelClick() {
        let self = this;
        this.Hide();
        FairyGUI.Timers.inst.Add(0.05, 1, () => {
            UIWind_1.default.remove(self);
        });
    }
}
exports.default = CustomName;
