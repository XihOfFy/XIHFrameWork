"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
const FairyGUI = CS.FairyGUI;
const FairyEditor = CS.FairyEditor;
const UIWind_1 = require("./UIWind");
const ToolMgr_1 = require("../ToolMgr");
const App = FairyEditor.App;
class FindFailureAssets extends UIWind_1.default {
    constructor() {
        super(new UIWind_1.WindData("Extend", "FindFailureAssets"));
        this.assets = new Map();
    }
    static show() {
        let url = ToolMgr_1.default.joinUrl("Extend", "FindFailureAssets");
        if (UIWind_1.default.FindUrl(url) == undefined) {
            UIWind_1.default.add(new this);
        }
        super.show(url);
    }
    onInit() {
        super.onInit();
        this.state = this.contentPane.GetController("state");
        this.check = this.contentPane.GetChild("check").asButton;
        this.progress = this.contentPane.GetChild("progress").asProgress;
        this.resList = this.contentPane.GetChild("resList").asList;
        this.resList.itemRenderer = this.onItemRenderer.bind(this);
        this.resList.onClickItem.Add(this.onListClickItem.bind(this));
        this.check.onClick.Add(this.onCheckClick.bind(this));
        let cancel = this.contentPane.GetChild("close").asButton;
        cancel.onClick.Add(this.onCancelClick.bind(this));
        this.resList.SetVirtual();
    }
    onShown() {
        super.onShown();
    }
    onHide() {
        super.onHide();
        this.resList.data = null;
        this.resList.numItems = 0;
    }
    onCheckClick() {
        return __awaiter(this, void 0, void 0, function* () {
            if (this.state.selectedIndex == 0) {
                this.state.selectedIndex = 1;
                this.assets.clear();
                let pkgs = App.project.allPackages;
                this.progress.value = 0;
                this.progress.max = ToolMgr_1.default.getItemsAll();
                for (let index = 0; index < pkgs.Count; index++) {
                    let pkg = pkgs.get_Item(index);
                    if (this.state.selectedIndex == 0)
                        break;
                    for (let j = 0; j < pkg.items.Count; j++) {
                        if (this.state.selectedIndex == 0)
                            break;
                        let item = pkg.items.get_Item(j);
                        this.progress.value += 1;
                        if (item.type != CS.FairyEditor.FObjectType.COMPONENT) {
                            continue;
                        }
                        //@ts-ignore
                        ToolMgr_1.default.getFailureAssets(item, this.assets, 0);
                        if (this.progress.value % 30 == 0) {
                            yield ToolMgr_1.default.sleep(0);
                        }
                    }
                }
                App.consoleView.Log("查找完毕");
                this.state.selectedIndex = 0;
                let asset = Array.from(this.assets).map(item => item[1]);
                this.resList.data = asset;
                this.resList.numItems = asset.length;
                this.contentPane.GetChild("projectCount").asTextField.templateVars = ToolMgr_1.default.getTemplateVars(['value'], [asset.length.toString()]);
            }
            else {
                this.state.selectedIndex = 0;
            }
        });
    }
    onItemRenderer(index, obj) {
        let data = this.resList.data[index];
        obj.data = data;
        let packageItem = App.project.GetItemByURL(data.url);
        obj.GetChild("title").text = packageItem.name;
        obj.__onDispose = () => {
            obj.GetChild("title").text = '';
            obj.data = null;
        };
    }
    onListClickItem(item) {
        let data = item.data.data;
        let packageItem = App.project.GetItemByURL(data.url);
        App.consoleView.Log(ToolMgr_1.default.getUBBUrl(packageItem.GetURL(), '失效组件'));
        for (let index = 0; index < data.failure.length; index++) {
            App.consoleView.Log('失效URL:' + data.failure[index]);
        }
    }
    onCancelClick() {
        let self = this;
        this.Hide();
        FairyGUI.Timers.inst.Add(0.05, 1, () => {
            UIWind_1.default.remove(self);
        });
    }
}
exports.default = FindFailureAssets;
