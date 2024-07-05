"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const SG_1 = require("../SG");
const ToolMgr_1 = require("../ToolMgr");
/**
 * 小说阅读
 * ctrl+ait+p 读取小说 再次点击关闭小说
 * ctrl+ait+. 下一页
 * ctrl+ait+, 上一页
 */
class LookText {
    static close() {
        LookText.page = 0;
        LookText.fontcount = 50;
        LookText.textLength = 0;
        LookText.pageMax = 0;
        LookText.msg = "";
        LookText.logItem.text = "";
        //@ts-ignore
        LookText.logItem = null;
        LookText.isRed = false;
    }
    //打开文本
    static open() {
        if (LookText.msg != "") {
            this.close();
            return;
        }
        if (CS.System.IO.File.Exists(SG_1.default.config[SG_1.Config.LookTextPath]) == false) {
            return;
        }
        LookText.msg = CS.System.IO.File.ReadAllText(SG_1.default.config[SG_1.Config.LookTextPath]);
        LookText.textLength = LookText.msg.length;
        LookText.page = SG_1.default.config[SG_1.Config.LookPage];
        LookText.fontcount = SG_1.default.config[SG_1.Config.LookFontCount];
        LookText.pageMax = Math.ceil(LookText.textLength / LookText.fontcount);
        LookText.isRed = true;
    }
    //翻页
    static nextPage() {
        if (LookText.isRed == false) {
            return;
        }
        if (LookText.logItem == null) {
            LookText.logItem = CS.FairyEditor.App.mainView.panel.GetChild("logItem");
            LookText.logItem.GetChild("title").asRichTextField.singleLine = true;
            LookText.logItem.GetChild("title").asRichTextField.autoSize = CS.FairyGUI.AutoSizeType.None;
        }
        LookText.page = SG_1.default.config[SG_1.Config.LookPage] + 1;
        LookText.logItem.text = LookText.msg.substring(LookText.page * LookText.fontcount, LookText.page * LookText.fontcount + LookText.fontcount).replace(/\n/g, '') + LookText.getPage();
        SG_1.default.config[SG_1.Config.LookPage] = LookText.page;
        //保存
        ToolMgr_1.default.saveConfig(SG_1.default.config_path);
    }
    static lastPage() {
        if (LookText.isRed == false) {
            return;
        }
        if (LookText.logItem == null) {
            LookText.logItem = CS.FairyEditor.App.mainView.panel.GetChild("logItem");
            LookText.logItem.GetChild("title").asRichTextField.singleLine = true;
            LookText.logItem.GetChild("title").asRichTextField.autoSize = CS.FairyGUI.AutoSizeType.None;
        }
        LookText.page = SG_1.default.config[SG_1.Config.LookPage] - 1;
        LookText.logItem.text = LookText.msg.substring(LookText.page * LookText.fontcount, LookText.page * LookText.fontcount + LookText.fontcount).replace(/\n/g, '') + LookText.getPage();
        SG_1.default.config[SG_1.Config.LookPage] = LookText.page;
        //保存
        ToolMgr_1.default.saveConfig(SG_1.default.config_path);
    }
    static getPage() {
        return `(${LookText.page + 1}/${LookText.pageMax})`;
    }
}
exports.default = LookText;
//开始索引
LookText.page = 0;
//每行最大字数
LookText.fontcount = 50;
//文本长度
LookText.textLength = 0;
/**最大页数 */
LookText.pageMax = 0;
//内容
LookText.msg = "";
/**是否可以阅读 */
LookText.isRed = false;
