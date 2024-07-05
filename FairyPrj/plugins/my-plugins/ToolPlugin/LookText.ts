import SG, { Config } from '../SG';
import ToolMgr from '../ToolMgr';
/**
 * 小说阅读
 * ctrl+ait+p 读取小说 再次点击关闭小说
 * ctrl+ait+. 下一页
 * ctrl+ait+, 上一页
 */
export default class LookText {
    //开始索引
    private static page = 0;
    //每行最大字数
    private static fontcount = 50;
    //文本长度
    private static textLength = 0;

    /**最大页数 */
    private static pageMax = 0;

    //内容
    private static msg: string = "";

    private static logItem: CS.FairyGUI.GButton;


    /**是否可以阅读 */
    private static isRed = false;

    private static close() {
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
            return
        }
        if (CS.System.IO.File.Exists(SG.config[Config.LookTextPath]) == false) {
            return
        }

        LookText.msg = CS.System.IO.File.ReadAllText(SG.config[Config.LookTextPath]);
        LookText.textLength = LookText.msg.length;
        LookText.page = SG.config[Config.LookPage];
        LookText.fontcount = SG.config[Config.LookFontCount];

        LookText.pageMax = Math.ceil(LookText.textLength / LookText.fontcount);

        LookText.isRed = true;
    }

    //翻页
    static nextPage() {

        if (LookText.isRed == false) {
            return
        }

        if (LookText.logItem == null) {
            LookText.logItem = CS.FairyEditor.App.mainView.panel.GetChild("logItem") as CS.FairyGUI.GButton
            LookText.logItem.GetChild("title").asRichTextField.singleLine = true;
            LookText.logItem.GetChild("title").asRichTextField.autoSize = CS.FairyGUI.AutoSizeType.None;
        }
        LookText.page = SG.config[Config.LookPage] + 1;
        LookText.logItem.text = LookText.msg.substring(LookText.page * LookText.fontcount, LookText.page * LookText.fontcount + LookText.fontcount).replace(/\n/g, '') + LookText.getPage()
        SG.config[Config.LookPage] = LookText.page;
        //保存
        ToolMgr.saveConfig(SG.config_path)
    }

    static lastPage() {

        if (LookText.isRed == false) {
            return
        }

        if (LookText.logItem == null) {
            LookText.logItem = CS.FairyEditor.App.mainView.panel.GetChild("logItem") as CS.FairyGUI.GButton
            LookText.logItem.GetChild("title").asRichTextField.singleLine = true;
            LookText.logItem.GetChild("title").asRichTextField.autoSize = CS.FairyGUI.AutoSizeType.None;
        }
        LookText.page = SG.config[Config.LookPage] - 1;
        LookText.logItem.text = LookText.msg.substring(LookText.page * LookText.fontcount, LookText.page * LookText.fontcount + LookText.fontcount).replace(/\n/g, '') + LookText.getPage()
        SG.config[Config.LookPage] = LookText.page;
        //保存
        ToolMgr.saveConfig(SG.config_path)
    }

    private static getPage() {
        return `(${LookText.page + 1}/${LookText.pageMax})`
    }
}