
const FairyGUI = CS.FairyGUI;
const FairyEditor = CS.FairyEditor
import UIWind, { WindData } from "./UIWind"
import ToolMgr from '../ToolMgr';
import SG, { Config } from '../SG';

export default class CustomName extends UIWind {

    constructor() {
        super(new WindData("Extend", "CustomName"))
    }

    static show() {
        let url = ToolMgr.joinUrl("Extend", "CustomName")
        if (UIWind.FindUrl(url) == undefined) {
            UIWind.add(new this)
        }

        super.show(url)
    }


    private list: CS.FairyGUI.GList;

    protected onInit(): void {
        super.onInit();

        this.list = this.contentPane.GetChild("list").asList;
        this.list.itemRenderer = this.onItemRenerer.bind(this);

        let ok = this.contentPane.GetChild("ok").asButton;
        let cancel = this.contentPane.GetChild("cancel").asButton
        ok.onClick.Add(this.onOKClick.bind(this))
        cancel.onClick.Add(this.onCancelClick.bind(this))

        this.contentPane.GetChild("reset").asButton.onClick.Add(this.onResetClick.bind(this))
    }

    protected onShown(): void {
        super.onShown();

        if (SG.config[Config.CUSTOMNAME] == null) {
            this.initAllTag()
        }

        let tags = SG.config[Config.CUSTOMNAME] as string[]
        this.list.data = tags
        this.list.numItems = 20;
    }

    protected onHide(): void {
        super.onHide();
        this.list.data = null;
        this.list.numItems = 0;
    }


    private onItemRenerer(index: number, obj: CS.FairyGUI.GComponent) {
        let data = this.list.data[index]
        obj.GetChild("title").asTextField.text = (index + 1).toString();
        obj.GetChild("input").asLabel.GetChild("title").asTextInput.inputTextField.text = data ? data : "";
    }

    private onOKClick() {
        let array = new Array<string>()
        for (let index = 0; index < this.list.numItems; index++) {
            let item = this.list.GetChildAt(index).asCom;
            let text =  item.GetChild("input").asLabel.GetChild("title").asTextInput.inputTextField.text;
            if (text && text != "") {
                array.push(text)
            }
        }
        SG.config[Config.CUSTOMNAME] = array
        ToolMgr.saveConfig(SG.config_path)

        this.onCancelClick();
    }

    private onResetClick() {
        this.list.numItems = 0;
        SG.config[Config.CUSTOMNAME] = null;
        this.onShown();
    }

    private initAllTag() {
        SG.config[Config.CUSTOMNAME] = ['title', 'icon', 'bar', 'bar_v', 'grip', 'arrow1', 'arrow2', 'ani', 'list', 'closeButton', 'dragArea', 'contentArea']
    }

    private onCancelClick() {
        let self = this;
        this.Hide();
        FairyGUI.Timers.inst.Add(0.05, 1, () => {
            UIWind.remove(self)
        })
    }
}

