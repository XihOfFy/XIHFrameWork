"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const SG_1 = require("../SG");
const ToolMgr_1 = require("../ToolMgr");
const FairyEditor = CS.FairyEditor;
const App = FairyEditor.App;
class PublishQuery {
    /**包体依赖导出 */
    DependencyQuery(name) {
        let querys = new Array();
        let pkg = App.project.GetPackageByName(name);
        let count = pkg.items.Count;
        let map = new Map();
        for (let index = 0; index < count; index++) {
            let item = pkg.items.get_Item(index);
            if (!item.exported)
                continue;
            if (item.type != FairyEditor.FPackageItemType.COMPONENT) {
                continue;
            }
            //不发布清理依赖资源
            if (SG_1.default.config[SG_1.Config.ClearOnPublish]) {
                ToolMgr_1.default.isClearOnPublish(item, map, 0);
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
                    if (yilaipkg.GetItem(reference.itemId) && querys.indexOf(yilaipkg.name) == -1 && yilaipkg.name != name) {
                        querys.push(yilaipkg.name);
                    }
                }
            }
        }
        if (SG_1.default.config[SG_1.Config.ClearOnPublish]) {
            querys = Array.from(map).map(item => item[1].name);
            let index = querys.indexOf(name);
            if (index != -1) {
                querys.splice(index, 1);
            }
        }
        let json_data = ToolMgr_1.default.loadJson(SG_1.default.query_path);
        if (!json_data) {
            json_data = {};
        }
        if (json_data['pkgName'] == null) {
            json_data['pkgName'] = new Array();
        }
        let pkgNames = json_data['pkgName'];
        querys.forEach((element, index) => {
            if (pkgNames.indexOf(element) == -1) {
                pkgNames.push(element);
            }
            //@ts-ignore  注释则显示包体名称
            querys[index] = pkgNames.indexOf(element);
        });
        if (querys.length > 0)
            json_data[name] = querys;
        else
            delete json_data[name];
        ToolMgr_1.default.writerJson(SG_1.default.query_path, json_data);
    }
}
exports.default = PublishQuery;
