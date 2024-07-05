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
const System = CS.System;
const UIWind_1 = require("./UIWind");
const ToolMgr_1 = require("../ToolMgr");
const SG_1 = require("../SG");
const App = FairyEditor.App;
class FindDependencyQuery extends UIWind_1.default {
    constructor() {
        super(new UIWind_1.WindData("Extend", "FindDependencyQuery"));
        //依赖选中的包体集合
        this.yilaiSelectPkgs = new Array();
        //点击锁定查询的pkgs
        this.lockPkgs = new Array();
    }
    static show() {
        let url = ToolMgr_1.default.joinUrl("Extend", "FindDependencyQuery");
        if (UIWind_1.default.FindUrl(url) == undefined) {
            UIWind_1.default.add(new this);
        }
        super.show(url);
    }
    onInit() {
        super.onInit();
        let self = this;
        this.projectCount = this.contentPane.GetChild("projectCount").asTextField;
        this.yilaiCount = this.contentPane.GetChild("yilaiCount").asTextField;
        this.resCount = this.contentPane.GetChild("resCount").asTextField;
        this.projectList = this.contentPane.GetChild("projectList").asList;
        this.yilaiList = this.contentPane.GetChild("yilaiList").asList;
        this.resList = this.contentPane.GetChild("resList").asList;
        this.externalCount = this.contentPane.GetChild("externalCount").asTextField;
        this.externalList = this.contentPane.GetChild("externalList").asList;
        this.state = this.contentPane.GetController("state");
        this.progress = this.contentPane.GetChild("progress").asProgress;
        this.contentPane.GetChild("close").onClick.Add(this.onCloseClick.bind(this));
        this.yilaiCB = this.contentPane.GetChild("yilaiCB").asButton;
        this.yilaiCB.onClick.Add(() => {
            self.onUpdateShown();
        });
        this.projectList.itemRenderer = this.onProjectListRenderer.bind(this);
        this.yilaiList.itemRenderer = this.onYiLaiListItemRenderer.bind(this);
        this.projectList.onClickItem.Add(this.onProjectListClickItem.bind(this));
        this.yilaiList.onClickItem.Add(this.onYiLaiListClickItem.bind(this));
        this.resList.itemRenderer = this.onResListItemRenderer.bind(this);
        this.resList.onClickItem.Add(this.onResListClickItem.bind(this));
        this.externalList.itemRenderer = this.onExternalListItemRenderer.bind(this);
        this.externalList.onClickItem.Add(this.onExternalListClickItem.bind(this));
        this.resList.SetVirtual();
    }
    onShown() {
        super.onShown();
        this.onUpdateShown();
        this.yilaiCB.selected = SG_1.default.config[SG_1.Config.ClearOnPublish];
    }
    onHide() {
        super.onHide();
        this.projectList.numItems = 0;
        this.yilaiList.numItems = 0;
        this.projectList.data = null;
        this.yilaiList.data = null;
        this.resList.numItems = 0;
        this.resList.data = null;
        this.externalList.data = null;
        this.externalList.numItems = 0;
    }
    onUpdateShown() {
        //获取项目包体
        let pkgs = [];
        for (let index = 0; index < App.project.allPackages.Count; index++) {
            pkgs.push(App.project.allPackages.get_Item(index));
        }
        this.projectList.data = pkgs;
        this.projectList.numItems = pkgs.length;
        this.projectCount.templateVars = this.getTemplateVars(["value"], [pkgs.length.toString()]);
    }
    onProjectListRenderer(index, obj) {
        let data = this.projectList.data[index];
        obj.text = data.name;
        obj.data = data;
        obj.__onDispose = () => {
            obj.text = "";
            obj.data = null;
        };
    }
    /**点击项目包体 查找对应的包体总依赖 */
    onProjectListClickItem(item) {
        return __awaiter(this, void 0, void 0, function* () {
            this.state.selectedIndex = 1;
            let querys = new Array();
            let pkg = item.data.data;
            let count = pkg.items.Count;
            this.progress.max = count;
            this.progress.value = 0;
            let list3Array = new Array();
            let map = new Map();
            for (let index = 0; index < count; index++) {
                let item = pkg.items.get_Item(index);
                this.progress.value = index;
                if (index % 50 == 0) {
                    yield ToolMgr_1.default.sleep(0);
                }
                //未导出资源 不查找
                if (!item.exported)
                    continue;
                if (item.type != FairyEditor.FPackageItemType.COMPONENT) {
                    continue;
                }
                if (this.yilaiCB.selected) {
                    ToolMgr_1.default.isClearOnPublish(item, map, 0);
                }
                else {
                    // //获取包体对应的依赖
                    let list = ToolMgr_1.default.createGenericList(FairyEditor.FPackageItem);
                    list.Add(item);
                    let data = new FairyEditor.DependencyQuery();
                    data.QueryDependencies(list, FairyEditor.DependencyQuery.SeekLevel.ALL);
                    for (let index = 0; index < data.references.Count; index++) {
                        let reference = data.references.get_Item(index);
                        let pkgID = reference.pkgId;
                        let yilaipkg = App.project.GetPackage(pkgID);
                        if (yilaipkg == null)
                            continue;
                        if (yilaipkg.GetItem(reference.itemId) && querys.indexOf(yilaipkg) == -1) {
                            querys.push(yilaipkg);
                        }
                    }
                }
                list3Array.push({
                    item: item,
                    querts: null
                });
            }
            this.clearSelectPkgs();
            this.lockPkgs = [];
            if (this.yilaiCB.selected) {
                map.forEach((v, k) => {
                    App.consoleView.Log(v.name);
                }, this);
                querys = Array.from(map).map(v => v[1]);
            }
            this.yilaiList.data = querys;
            this.yilaiList.numItems = querys.length;
            this.yilaiCount.templateVars = this.getTemplateVars(["value"], [querys.length.toString()]);
            this.resList.data = list3Array;
            this.resList.numItems = list3Array.length;
            this.resCount.templateVars = this.getTemplateVars(["value"], [list3Array.length.toString()]);
            this.externalList.numItems = null;
            this.externalList.data = null;
            this.externalCount.templateVars = this.getTemplateVars(["value"], ['0']);
            this.state.selectedIndex = 0;
        });
    }
    /**获取资源点击的依赖包体 */
    getResYiLaiArray(item) {
        let querys = new Array();
        let list = ToolMgr_1.default.createGenericList(FairyEditor.FPackageItem);
        list.Add(item);
        let data = new FairyEditor.DependencyQuery();
        data.QueryDependencies(list, FairyEditor.DependencyQuery.SeekLevel.ALL);
        for (let index = 0; index < data.references.Count; index++) {
            let reference = data.references.get_Item(index);
            let pkgID = reference.pkgId;
            let yilaipkg = App.project.GetPackage(pkgID);
            if (yilaipkg == null)
                continue;
            if (yilaipkg.GetItem(reference.itemId) && querys.indexOf(yilaipkg) == -1) {
                querys.push(yilaipkg);
            }
        }
        return querys;
    }
    onResListItemRenderer(index, obj) {
        let data = this.resList.data[index];
        obj.text = data.item.name;
        obj.data = data;
        if (this.yilaiCB.selected) {
            let map = new Map();
            ToolMgr_1.default.isClearOnPublish(data.item, map, 0);
            data.querts = Array.from(map).map(x => x[1]);
        }
        else {
            data.querts = this.getResYiLaiArray(data.item);
        }
        let state = 0;
        if (this.yilaiSelectPkgs.length != 0) {
            if (this.isResInYiLai(obj, this.yilaiSelectPkgs)) {
                state = 1;
            }
            else {
                state = 2;
            }
        }
        obj.asCom.GetController("state").selectedIndex = state;
        obj.__onDispose = () => {
            obj.text = "";
            obj.data = null;
        };
    }
    onResListClickItem(obj) {
        let data = obj.data.data;
        let item = data.item;
        this.clearSelectPkgs();
        this.yilaiList.data = data.querts;
        this.yilaiList.numItems = data.querts.length;
        this.yilaiCount.templateVars = this.getTemplateVars(["value"], [data.querts.length.toString()]);
        //获取当前显示的包体 是否在锁定查询的信息里面
        let lockPkgs = new Array();
        for (let index = 0; index < data.querts.length; index++) {
            let pkg = data.querts[index];
            if (this.lockPkgs.indexOf(pkg) != -1) {
                lockPkgs.push(pkg);
                this.yilaiSelectPkgs.push(pkg);
            }
        }
        for (let index = 0; index < this.resList.numChildren; index++) {
            let item = this.resList.GetChildAt(index);
            if (this.isResInYiLai(item, lockPkgs))
                this.resList.GetChildAt(index).asCom.GetController("state").selectedIndex = 1;
            else
                this.resList.GetChildAt(index).asCom.GetController("state").selectedIndex = 2;
        }
        App.consoleView.Log(ToolMgr_1.default.getUBBUrl(item.GetURL(), item.name));
        this.updateExternalList(item);
    }
    //刷新外部资源列表
    updateExternalList(item) {
        //刷新当前选中的包体 对应的依赖资源
        let query = this.getDependencyQuery(item);
        //如果勾选的外部依赖包体 那么只查找对应的 否则显示全部
        if (this.yilaiSelectPkgs.length > 0)
            for (let index = query.length - 1; index >= 0; index--) {
                const element = query[index];
                if (this.yilaiSelectPkgs.indexOf(element.owner) == -1) {
                    query.splice(index, 1);
                }
            }
        this.externalList.data = query;
        this.externalList.numItems = this.externalList.data.length;
        this.externalCount.templateVars = this.getTemplateVars(["value"], [this.externalList.data.length + ""]);
    }
    //渲染点击的资源 外部依赖的详细资源
    onExternalListItemRenderer(index, obj) {
        let data = this.externalList.data[index];
        obj.text = data.name;
        obj.data = data;
        obj.GetChild("icon").asLoader.url = data.GetIcon(false, false, true);
        obj.__onDispose = () => {
            obj.text = "";
            obj.data = null;
            obj.GetChild("icon").asLoader.url = "";
        };
    }
    onExternalListClickItem(obj) {
        let data = obj.data.data;
        App.consoleView.Log(ToolMgr_1.default.getUBBUrl(data.GetURL(), `依赖:${data.owner.name}/${data.name}`));
    }
    /**生成模板值 */
    getTemplateVars(keys, nums) {
        let dic = ToolMgr_1.default.createGenericDictionary(System.String, System.String);
        keys.forEach((key, index) => {
            dic.set_Item(key, nums[index]);
        }, this);
        return dic;
    }
    //清空依赖选中
    clearSelectPkgs() {
        this.yilaiList.SelectNone();
        this.yilaiSelectPkgs = [];
    }
    /**获取依赖清单 */
    getDependencyQuery(item) {
        //获取包体对应的依赖
        let yilaiItems = new Array();
        if (this.yilaiCB.selected) {
            let map = new Map();
            ToolMgr_1.default.getClearOnPublishPackageItem(item, map, 0);
            yilaiItems = Array.from(map).map(x => x[1]);
        }
        else {
            let list = ToolMgr_1.default.createGenericList(FairyEditor.FPackageItem);
            list.Add(item);
            let data = new FairyEditor.DependencyQuery();
            data.QueryDependencies(list, FairyEditor.DependencyQuery.SeekLevel.ALL);
            for (let index = 0; index < data.references.Count; index++) {
                let reference = data.references.get_Item(index);
                let pkgID = reference.pkgId;
                let yilaipkg = App.project.GetPackage(pkgID);
                if (yilaipkg == null)
                    continue;
                let yilaiItem = yilaipkg.GetItem(reference.itemId);
                if (yilaiItem && yilaiItems.indexOf(yilaiItem) == -1) {
                    //App.consoleView.Log(ToolMgr.getUBBUrl(yilaiItem.GetURL(), yilaiItem.name));
                    yilaiItems.push(yilaiItem);
                }
            }
        }
        return yilaiItems;
    }
    onYiLaiListClickItem(obj) {
        let btn = obj.data;
        //锁定的包体不允许取消选中
        if (!btn.selected && this.lockPkgs.indexOf(btn.data) != -1) {
            btn.selected = true;
            return;
        }
        this.getSelectYiLaiPkgs();
        //获取选中的物体
        for (let index = 0; index < this.resList.numChildren; index++) {
            let resItem = this.resList.GetChildAt(index);
            if (this.isResInYiLai(resItem, this.yilaiSelectPkgs) || this.isResInYiLai(resItem, this.lockPkgs)) {
                resItem.asCom.GetController("state").selectedIndex = 1;
            }
            else {
                resItem.asCom.GetController("state").selectedIndex = 2;
            }
        }
        //刷新当前外部资源列表
        if (this.resList.selectedIndex < 0) {
            this.externalList.numItems = 0;
            this.externalList.data = null;
            this.externalCount.templateVars = this.getTemplateVars(['value'], ['0']);
        }
        else {
            let res = this.resList.GetChildAt(this.resList.selectedIndex).data;
            this.updateExternalList(res.item);
        }
    }
    //获取依赖列表全部选中的包
    getSelectYiLaiPkgs() {
        //获取全部选中的物体
        let selectIndexs = this.yilaiList.GetSelection();
        let selectPkgs = new Array();
        for (let index = 0; index < selectIndexs.Count; index++) {
            selectPkgs.push(this.yilaiList.GetChildAt(selectIndexs.get_Item(index)).data);
        }
        this.yilaiSelectPkgs = selectPkgs;
    }
    //资源是否在依赖列表中
    isResInYiLai(obj, pkgs) {
        let querts = obj.data.querts;
        if (pkgs.length == 0)
            return false;
        //判断两个数组是否有相同的值
        return pkgs.some(pkg => {
            return querts.some(resItem => {
                return resItem.name == pkg.name;
            });
        });
    }
    onYiLaiListItemRenderer(index, obj) {
        let data = this.yilaiList.data[index];
        obj.text = data.name;
        obj.data = data;
        obj.selected = this.lockPkgs.indexOf(data) != -1;
        let cb = obj.GetChild("cb").asButton;
        cb.selected = obj.selected;
        cb.RemoveEventListeners();
        cb.onClick.Add(this.onCBLock.bind(this, data, obj));
        obj.__onDispose = () => {
            obj.text = "";
            obj.data = null;
        };
    }
    //CB锁定
    onCBLock(data, obj, item) {
        let index = this.lockPkgs.indexOf(data);
        if (index == -1) {
            if (obj.selected)
                item.StopPropagation();
            this.lockPkgs.push(data);
        }
        else {
            this.lockPkgs.splice(index, 1);
            item.StopPropagation();
        }
    }
    onCloseClick() {
        let self = this;
        self.Hide();
        FairyGUI.Timers.inst.Add(0.05, 1, () => {
            UIWind_1.default.remove(this);
        });
    }
}
exports.default = FindDependencyQuery;
