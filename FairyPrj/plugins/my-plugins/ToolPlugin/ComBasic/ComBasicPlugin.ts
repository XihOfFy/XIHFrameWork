const FairyGUI = CS.FairyGUI
const FairyEditor = CS.FairyEditor
const System = CS.System
import SG, { Config } from '../../SG';
import ToolMgr from '../../ToolMgr';
const App = FairyEditor.App;

export class BasicCopyData {
    x: string
    y: string
    width: string;
    height: string;
    scaleX: string;
    scaleY: string;
    skewX: string;
    skewY: string;
    pivotX: string;
    pivotY: string;
    alpha: string;
    rotation: string

    //扩展
    minWidth: string
    minHeight: string
    maxWidth: string
    maxHeight: string

    useSourceSize: boolean;
    aspectLocked: boolean;
    anchor: boolean;
    visible: boolean;
    grayed: boolean
    touchable: boolean
}

export default class ComBasicPlugin extends FairyEditor.View.PluginInspector {

    static create() {
        //Register a inspector
        App.inspectorView.AddInspector(() => new ComBasicPlugin(), "ComponentBasic", "");
        //Condition to show it
        App.docFactory.ConnectInspector("ComponentBasic", "", false, false);
    }
    private btnCopy: CS.FairyGUI.GButton;
    private btnPaste: CS.FairyGUI.GButton;

    public constructor() {
        super();

        this.panel = FairyGUI.UIPackage.CreateObject("Extend", "ComponentBasic").asCom;
        this.btnCopy = this.panel.GetChild("btnCopy").asButton;
        this.btnPaste = this.panel.GetChild("btnPaste").asButton;

        this.btnCopy.onClick.Add(this.onBtnCopyClick.bind(this))
        this.btnPaste.onClick.Add(this.onBtnPasteClick.bind(this))

        this.updateAction = () => { return this.updateUI(); };

        if (SG.config[Config.XYWHComputer]) {
            let comBasic = App.inspectorView.GetInspector("basic").panel;
            let items = ["x", "y", "width", "height"]
            for (let index = 0; index < items.length; index++) {
                const element = items[index];
                comBasic.GetChild(element).asLabel.GetChild("title").asTextInput.RemoveEventListeners()
                comBasic.GetChild(element).asLabel.GetChild("title").asTextInput.onFocusOut.Add(this.onXYWHComputer.bind(this, element))
            }
        }
    }

    private onXYWHComputer(name: string) {
        let comBasic = App.inspectorView.GetInspector("basic").panel;
        let label = comBasic.GetChild(name).asLabel
        let text = label.GetTextField().text;
        try {
            let num = eval(text)
            num = Math.floor(num)
            //开启了保持比例
            if (comBasic.GetChild("aspectLocked").asButton.selected) {
                let w = App.activeDoc.inspectingTarget.GetProperty("width")
                let h = App.activeDoc.inspectingTarget.GetProperty("height")
                if (name == 'width') {
                    let height = Math.floor(num / w * h)
                    App.activeDoc.inspectingTarget.docElement.SetProperty('height', height)
                } else {
                    let width = Math.floor(num / h * w)
                    App.activeDoc.inspectingTarget.docElement.SetProperty('width', width)
                }
            }
            App.activeDoc.inspectingTarget.docElement.SetProperty(name, num)
        } catch {
            App.consoleView.Log("请输入数字或者正确的运算符")
        }
    }

    private copyData: BasicCopyData;

    private onBtnCopyClick() {
        // let comBasic = App.inspectorView.GetInspector("comBasic").panel;
        let comBasic = App.inspectorView.GetInspector("basic").panel;
        let data = new BasicCopyData()
        data.x = comBasic.GetChild("x").asLabel.GetTextField().text
        data.y = comBasic.GetChild("y").asLabel.GetTextField().text

        data.width = comBasic.GetChild("width").asLabel.GetTextField().text
        data.height = comBasic.GetChild("height").asLabel.GetTextField().text

        data.scaleX = comBasic.GetChild("scaleX").asLabel.GetTextField().text
        data.scaleY = comBasic.GetChild("scaleY").asLabel.GetTextField().text
        data.skewX = comBasic.GetChild("skewX").asLabel.GetTextField().text
        data.skewY = comBasic.GetChild("skewY").asLabel.GetTextField().text

        data.pivotX = comBasic.GetChild("pivotX").asLabel.GetTextField().text
        data.pivotY = comBasic.GetChild("pivotY").asLabel.GetTextField().text

        data.alpha = comBasic.GetChild("alpha").asLabel.GetTextField().text
        data.rotation = comBasic.GetChild("rotation").asLabel.GetTextField().text

        if (App.activeDoc.content.GetController("showRestrictSize")) {
            data.minWidth = comBasic.GetChild("minWidth").asLabel.GetTextField().text
            data.maxWidth = comBasic.GetChild("maxWidth").asLabel.GetTextField().text
            data.minHeight = comBasic.GetChild("minHeight").asLabel.GetTextField().text
            data.maxHeight = comBasic.GetChild("maxHeight").asLabel.GetTextField().text
        }

        data.useSourceSize = comBasic.GetChild("useSourceSize").asButton.selected
        data.aspectLocked = comBasic.GetChild("aspectLocked").asButton.selected
        data.anchor = comBasic.GetChild("anchor").asButton.selected
        data.visible = !comBasic.GetChild("visible").asButton.selected
        data.touchable = !comBasic.GetChild("touchable").asButton.selected
        data.grayed = comBasic.GetChild("grayed").asButton.selected

        this.copyData = data;

        this.btnPaste.grayed = false;
        this.btnPaste.touchable = true;
    }

    private onBtnPasteClick() {
        if (this.copyData == null) return
        let List = ToolMgr.createGenericList<CS.FairyEditor.FObject>(FairyEditor.FObject)
        List.AddRange(App.activeDoc.inspectingTargets);
        let count = List.Count;

        for (let index = 0; index < count; index++) {
            const obj = List.get_Item(index);
            obj.docElement.SetProperty("x", this.copyData.x.toString())
            obj.docElement.SetProperty("y", this.copyData.y.toString())

            obj.docElement.SetProperty("width", this.copyData.width.toString())
            obj.docElement.SetProperty("height", this.copyData.height.toString())

            obj.docElement.SetProperty("scaleX", this.copyData.scaleX.toString())
            obj.docElement.SetProperty("scaleY", this.copyData.scaleY.toString())
            obj.docElement.SetProperty("skewX", this.copyData.skewX.toString())
            obj.docElement.SetProperty("skewY", this.copyData.skewY.toString())
            obj.docElement.SetProperty("pivotX", this.copyData.pivotX.toString())
            obj.docElement.SetProperty("pivotY", this.copyData.pivotY.toString())

            obj.docElement.SetProperty("alpha", this.copyData.alpha.toString())
            obj.docElement.SetProperty("rotation", this.copyData.rotation.toString())

            if (App.activeDoc.content.GetController("showRestrictSize")) {
                obj.docElement.SetProperty("minWidth", this.copyData.minWidth.toString())
                obj.docElement.SetProperty("maxWidth", this.copyData.maxWidth.toString())
                obj.docElement.SetProperty("minHeight", this.copyData.minHeight.toString())
                obj.docElement.SetProperty("maxHeight", this.copyData.maxHeight.toString())
            }

            //开启全部属性赋值

            if (SG.config[Config.CopyAttribute]) {
                obj.docElement.SetProperty("useSourceSize", this.copyData.useSourceSize)
                obj.docElement.SetProperty("aspectLocked", this.copyData.aspectLocked)
                obj.docElement.SetProperty("anchor", this.copyData.anchor)
                obj.docElement.SetProperty("visible", this.copyData.visible)
                obj.docElement.SetProperty("grayed", this.copyData.grayed)
                obj.docElement.SetProperty("touchable", this.copyData.touchable)
            }

        }
    }

    private updateUI(): boolean {
        //返回false 不渲染插件
        // let self = this;
        // let List = ToolMgr.createGenericList<CS.FairyEditor.FObject>(FairyEditor.FObject)
        // List.AddRange(App.activeDoc.inspectingTargets);
        // let sels = List;
        // for (let index = 0; index < List.Count; index++) {
        //     const element = List.get_Item(index);
        //     if ((element instanceof FairyEditor.) == false) {
        //         return false
        //     }
        // }
        if (this.panel.parent.GetChild("basic") == null) return false
        let basic = this.panel.parent.GetChild('basic').asCom
        if (basic == null) return false

        //自定义名称扩展
        if (SG.config[Config.OPENCUSTOMNAME]) {
            let c_name = basic.GetChild("name").asComboBox;
            let list = ToolMgr.createGenericList<string>(System.String);
            let array = SG.config[Config.CUSTOMNAME] as string[];
            array.forEach(element => {
                list.Add(element)
            }, this)
            c_name.values = list.ToArray()
            c_name.items = list.ToArray()
            c_name.text = App.activeDoc.inspectingTarget.name
        }


        if (App.activeDoc.inspectingTarget instanceof CS.FairyEditor.FTextField) {

            let target = App.activeDoc.inspectingTarget;
            if (target.name == "@lookTextPath") {
                if (target.text == "") {
                    target.text = SG.config[Config.LookTextPath]
                } else {
                    SG.config[Config.LookTextPath] = target.text
                    ToolMgr.saveConfig(SG.config_path)
                }
            }
        }

        this.btnPaste.grayed = this.copyData == null;
        this.btnPaste.touchable = this.copyData != null;

        let basicIndex = this.panel.parent.GetChildIndex(basic);
        let customIndex = this.panel.parent.GetChildIndex(this.panel);

        if (basicIndex == customIndex - 1) return true
        let line = this.panel.parent.GetChildAt(customIndex - 1)
        this.panel.parent.SetChildIndex(this.panel, basicIndex + 1)
        if (line) line.visible = false;
        return true
    }

    /**比较数组是否相同 */
    private isArrayEqual(a: CS.System.Array$1<any>, b: CS.System.Array$1<any>): boolean {
        if (a.Length !== b.Length) {
            return false
        }

        for (let index = 0; index < a.Length; index++) {
            const ai = a.get_Item(index);
            const bi = b.get_Item(index)
            if (ai !== bi) return false
        }

        return true
    }
}