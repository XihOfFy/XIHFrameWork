const FairyEditor = CS.FairyEditor
const App = FairyEditor.App;
App.pluginManager.LoadUIPackage(App.pluginManager.basePath + "/" + eval("__dirname") + '/Extend')
App.pluginManager.LoadUIPackage(App.pluginManager.basePath + "/" + eval("__dirname") + '/Basic')


import ToolPlugin from "./ToolPlugin/ToolPlugin";
import SG, { Config } from "./SG";
import ToolMgr from "./ToolMgr";
import KeyManager from "./KeyManager";
import PublishQuery from "./ToolPlugin/PublishQuery";

SG.config_path = `${App.pluginManager.basePath}/config.json`
SG.query_path = `${App.pluginManager.basePath}/dependency.json`

//依赖文件生成后需要发布的路径
SG.dependency_copy_to_path = `${App.pluginManager.basePath}`

let json_data = ToolMgr.loadJson(SG.config_path)
if (json_data == null) {
    SG.config[Config.Dependency] = false;
    SG.config[Config.CopyAttribute] = false;
    SG.config[Config.XYWHComputer] = false;
    SG.config[Config.ClearOnPublish] = false;
    SG.config[Config.CUSTOMNAME] = null;
    SG.config[Config.OPENCUSTOMNAME] = false;

    SG.config[Config.LookTextPath] = false;
    SG.config[Config.LookPage] = 0;
    SG.config[Config.LookFontCount] = 75;
} else {
    SG.config[Config.Dependency] = json_data[Config.Dependency] != null ? json_data[Config.Dependency] : false;
    SG.config[Config.CopyAttribute] = json_data[Config.CopyAttribute] != null ? json_data[Config.CopyAttribute] : false;
    SG.config[Config.XYWHComputer] = json_data[Config.XYWHComputer] != null ? json_data[Config.XYWHComputer] : false;
    SG.config[Config.ClearOnPublish] = json_data[Config.ClearOnPublish] != null ? json_data[Config.ClearOnPublish] : false;
    SG.config[Config.CUSTOMNAME] = json_data[Config.CUSTOMNAME] != null ? json_data[Config.CUSTOMNAME] : ['title', 'icon', 'bar', 'bar_v', 'grip', 'arrow1', 'arrow2', 'ani', 'list', 'closeButton', 'dragArea', 'contentArea'];
    SG.config[Config.OPENCUSTOMNAME] = json_data[Config.OPENCUSTOMNAME] != null ? json_data[Config.OPENCUSTOMNAME] : false;

    SG.config[Config.LookTextPath] = json_data[Config.LookTextPath] != null ? json_data[Config.LookTextPath] : "";
    SG.config[Config.LookPage] = json_data[Config.LookPage] != null ? json_data[Config.LookPage] : 0;
    SG.config[Config.LookFontCount] = json_data[Config.LookFontCount] != null ? json_data[Config.LookFontCount] : 75;
}

//添加工具插件
const toolPlugin = new ToolPlugin();
toolPlugin.Register()


function onDestroy() {
    toolPlugin.onDestroy();
    App.groot.onKeyDown.Remove(onKeyDown)
}

App.groot.onKeyDown.Set(onKeyDown)

function onKeyDown(evt: CS.FairyGUI.EventContext) {
    KeyManager.onKeyDown(evt);
}

//生成代码的时候 生成对应的AB配置
function onPublish(handler: CS.FairyEditor.PublishHandler) {
    handler.genCode = false; //prevent default output
    if (!SG.config[Config.Dependency]) return
    new PublishQuery().DependencyQuery(handler.fileName)

    //打开注释会生成json文件到本地 路径可以配置  SG.dependency_copy_to_path
    let dependency = "Dependency_temp.json"
    let copyPath = SG.dependency_copy_to_path
    if (CS.System.IO.File.Exists(copyPath + "/" + dependency)) {
        CS.System.IO.File.Delete(copyPath + "/" + dependency)
    }
    CS.System.IO.File.Copy(SG.query_path, copyPath + "/" + dependency)


    let a = `[url=${copyPath}]`
    let b = handler.fileName + "生成依赖文件成功"
    let c = "[/url]"

    App.consoleView.Log(a + b + c)
}

App.consoleView.Log(ToolMgr.getHttpPath('https://gitee.com/liuhai875311152/fgui-plugin/tree/master', '通用插件仓库地址'))

export { onPublish, onDestroy };

// function readAllFiles(path) {
//     let files = CS.System.IO.Directory.GetFiles(path,'*.*')
//    for (let index = 0; index < files.Length; index++) {
//         const element = files.get_Item(index);
//         App.consoleView.Log("文件:"+element)
//    }

//     let dirs = CS.System.IO.Directory.GetDirectories(path)
//     for (let index = 0; index < dirs.Length; index++) {
//         const element = dirs.get_Item(index);
//         App.consoleView.Log("文件夹:"+element)
//         readAllFiles(element)
//    }
// }
// readAllFiles(App.pluginManager.basePath)
// App.consoleView.Log(App.pluginManager.basePath)