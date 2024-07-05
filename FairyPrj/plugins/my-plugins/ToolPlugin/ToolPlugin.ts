const FairyEditor = CS.FairyEditor
import FindDependencyQuery from "../WindowView/FindDependencyQuery"
import ComBasicPlugin from "./ComBasic/ComBasicPlugin"
import UIWind from "../WindowView/UIWind";
import FindFailureAssets from "../WindowView/FindFailureAssets";
import CustomName from "../WindowView/CustomName";
import CustomPlugin from "../WindowView/CustomPlugin";
import SG, { Config } from "../SG";
const App = FairyEditor.App

export default class ToolPlugin {
    Register() {
        this.registerMenu();
        this.registerInspector()
    }

    private registerMenu() {
        let toolmenu = App.menu.GetSubMenu("tool")
        toolmenu.AddSeperator()
        toolmenu.AddItem("自定义插件", "openCustomPlugin", this.openCustomPlugin.bind(this))
        toolmenu.AddItem("查找组件依赖关系", "findDependencyQuery", this.findDependencyQuery.bind(this))
        toolmenu.AddItem("查找失效资源", "FindFailureAssets", this.FindFailureAssets.bind(this))
        if (SG.config[Config.OPENCUSTOMNAME])
            toolmenu.AddItem("扩展名称下拉框", "customName", this.customName.bind(this))
    }

    private findDependencyQuery() {
        FindDependencyQuery.show()
    }

    private openCustomPlugin() {
        CustomPlugin.show()
    }

    private FindFailureAssets() {
        FindFailureAssets.show()
    }

    private registerInspector() {
        ComBasicPlugin.create();
    }

    private customName() {
        CustomName.show()
    }

    onDestroy() {
        let toolmenu = App.menu.GetSubMenu("tool")
        toolmenu.RemoveItem("openCustomPlugin")
        toolmenu.RemoveItem("findDependencyQuery")
        toolmenu.RemoveItem('FindFailureAssets')
        toolmenu.RemoveItem('customName')
        UIWind.delAll([]);
        App.consoleView.Log("插件卸载")
    }
}

