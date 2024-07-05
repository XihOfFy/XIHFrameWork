"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const FairyEditor = CS.FairyEditor;
const FindDependencyQuery_1 = require("../WindowView/FindDependencyQuery");
const ComBasicPlugin_1 = require("./ComBasic/ComBasicPlugin");
const UIWind_1 = require("../WindowView/UIWind");
const FindFailureAssets_1 = require("../WindowView/FindFailureAssets");
const CustomName_1 = require("../WindowView/CustomName");
const CustomPlugin_1 = require("../WindowView/CustomPlugin");
const SG_1 = require("../SG");
const App = FairyEditor.App;
class ToolPlugin {
    Register() {
        this.registerMenu();
        this.registerInspector();
    }
    registerMenu() {
        let toolmenu = App.menu.GetSubMenu("tool");
        toolmenu.AddSeperator();
        toolmenu.AddItem("自定义插件", "openCustomPlugin", this.openCustomPlugin.bind(this));
        toolmenu.AddItem("查找组件依赖关系", "findDependencyQuery", this.findDependencyQuery.bind(this));
        toolmenu.AddItem("查找失效资源", "FindFailureAssets", this.FindFailureAssets.bind(this));
        if (SG_1.default.config[SG_1.Config.OPENCUSTOMNAME])
            toolmenu.AddItem("扩展名称下拉框", "customName", this.customName.bind(this));
    }
    findDependencyQuery() {
        FindDependencyQuery_1.default.show();
    }
    openCustomPlugin() {
        CustomPlugin_1.default.show();
    }
    FindFailureAssets() {
        FindFailureAssets_1.default.show();
    }
    registerInspector() {
        ComBasicPlugin_1.default.create();
    }
    customName() {
        CustomName_1.default.show();
    }
    onDestroy() {
        let toolmenu = App.menu.GetSubMenu("tool");
        toolmenu.RemoveItem("openCustomPlugin");
        toolmenu.RemoveItem("findDependencyQuery");
        toolmenu.RemoveItem('FindFailureAssets');
        toolmenu.RemoveItem('customName');
        UIWind_1.default.delAll([]);
        App.consoleView.Log("插件卸载");
    }
}
exports.default = ToolPlugin;
