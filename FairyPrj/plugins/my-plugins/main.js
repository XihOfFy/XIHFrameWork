"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const FairyEditor = CS.FairyEditor;
const App = FairyEditor.App;
App.pluginManager.LoadUIPackage(App.pluginManager.basePath + "/" + eval("__dirname") + '/Extend');
App.pluginManager.LoadUIPackage(App.pluginManager.basePath + "/" + eval("__dirname") + '/Basic');
const ToolPlugin_1 = require("./ToolPlugin/ToolPlugin");
const SG_1 = require("./SG");
const ToolMgr_1 = require("./ToolMgr");
const KeyManager_1 = require("./KeyManager");
const PublishQuery_1 = require("./ToolPlugin/PublishQuery");
SG_1.default.config_path = `${App.pluginManager.basePath}/config.json`;
SG_1.default.query_path = `${App.pluginManager.basePath}/dependency.json`;
//依赖文件生成后需要发布的路径
SG_1.default.dependency_copy_to_path = `${App.pluginManager.basePath}`;
let json_data = ToolMgr_1.default.loadJson(SG_1.default.config_path);
if (json_data == null) {
    SG_1.default.config[SG_1.Config.Dependency] = false;
    SG_1.default.config[SG_1.Config.CopyAttribute] = false;
    SG_1.default.config[SG_1.Config.XYWHComputer] = false;
    SG_1.default.config[SG_1.Config.ClearOnPublish] = false;
    SG_1.default.config[SG_1.Config.CUSTOMNAME] = null;
    SG_1.default.config[SG_1.Config.OPENCUSTOMNAME] = false;
    SG_1.default.config[SG_1.Config.LookTextPath] = false;
    SG_1.default.config[SG_1.Config.LookPage] = 0;
    SG_1.default.config[SG_1.Config.LookFontCount] = 75;
}
else {
    SG_1.default.config[SG_1.Config.Dependency] = json_data[SG_1.Config.Dependency] != null ? json_data[SG_1.Config.Dependency] : false;
    SG_1.default.config[SG_1.Config.CopyAttribute] = json_data[SG_1.Config.CopyAttribute] != null ? json_data[SG_1.Config.CopyAttribute] : false;
    SG_1.default.config[SG_1.Config.XYWHComputer] = json_data[SG_1.Config.XYWHComputer] != null ? json_data[SG_1.Config.XYWHComputer] : false;
    SG_1.default.config[SG_1.Config.ClearOnPublish] = json_data[SG_1.Config.ClearOnPublish] != null ? json_data[SG_1.Config.ClearOnPublish] : false;
    SG_1.default.config[SG_1.Config.CUSTOMNAME] = json_data[SG_1.Config.CUSTOMNAME] != null ? json_data[SG_1.Config.CUSTOMNAME] : ['title', 'icon', 'bar', 'bar_v', 'grip', 'arrow1', 'arrow2', 'ani', 'list', 'closeButton', 'dragArea', 'contentArea'];
    SG_1.default.config[SG_1.Config.OPENCUSTOMNAME] = json_data[SG_1.Config.OPENCUSTOMNAME] != null ? json_data[SG_1.Config.OPENCUSTOMNAME] : false;
    SG_1.default.config[SG_1.Config.LookTextPath] = json_data[SG_1.Config.LookTextPath] != null ? json_data[SG_1.Config.LookTextPath] : "";
    SG_1.default.config[SG_1.Config.LookPage] = json_data[SG_1.Config.LookPage] != null ? json_data[SG_1.Config.LookPage] : 0;
    SG_1.default.config[SG_1.Config.LookFontCount] = json_data[SG_1.Config.LookFontCount] != null ? json_data[SG_1.Config.LookFontCount] : 75;
}
//添加工具插件
const toolPlugin = new ToolPlugin_1.default();
toolPlugin.Register();
function onDestroy() {
    toolPlugin.onDestroy();
    App.groot.onKeyDown.Remove(onKeyDown);
}
exports.onDestroy = onDestroy;
App.groot.onKeyDown.Set(onKeyDown);
function onKeyDown(evt) {
    KeyManager_1.default.onKeyDown(evt);
}
//生成代码的时候 生成对应的AB配置
function onPublish(handler) {
    handler.genCode = false; //prevent default output
    if (!SG_1.default.config[SG_1.Config.Dependency])
        return;
    new PublishQuery_1.default().DependencyQuery(handler.fileName);
    //打开注释会生成json文件到本地 路径可以配置  SG.dependency_copy_to_path
    let dependency = "Dependency_temp.json";
    let copyPath = SG_1.default.dependency_copy_to_path;
    if (CS.System.IO.File.Exists(copyPath + "/" + dependency)) {
        CS.System.IO.File.Delete(copyPath + "/" + dependency);
    }
    CS.System.IO.File.Copy(SG_1.default.query_path, copyPath + "/" + dependency);
    let a = `[url=${copyPath}]`;
    let b = handler.fileName + "生成依赖文件成功";
    let c = "[/url]";
    App.consoleView.Log(a + b + c);
}
exports.onPublish = onPublish;
App.consoleView.Log(ToolMgr_1.default.getHttpPath('https://gitee.com/liuhai875311152/fgui-plugin/tree/master', '通用插件仓库地址'));
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
