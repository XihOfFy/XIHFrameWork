const FairyEditor = CS.FairyEditor
const System = CS.System
import { $generic } from 'puerts';
import SG from "./SG";
const App = FairyEditor.App;

export default class ToolMgr {

    /**获取选中目标数量 */
    static getSelectedTargetCount(): number {
        let items = App.activeDoc.inspectingTargets;
        let index = 0;
        try {
            while ((items.get_Item(index) != null)) {
                index++;
            }
        } catch (e) {
            return index
        }
        return index
    }

    /**获取UBB Url文本 */
    static getUBBUrl(url: string, titile: string) {
        return `[url=` + `${url}]${titile}[/url]`
    }

    /**UBB file文本 */
    static getUBBFile(data: CS.FairyEditor.FPackageItem) {

        let a = `[url=${ToolMgr.getFilePath(data)}]`
        let b = this.getAbsPackageItemPath(data)
        let c = "[/url]"
        return a + b + c
    }

    /**获取packageItem的File路径 不带后缀 */
    static getFilePath(data: CS.FairyEditor.FPackageItem) {
        return `file://${ToolMgr.getAbsPackageItemPath(data)}${ToolMgr.getAssetType(data)}`
    }

    static getHttpPath(http: string, title: string) {
        let a = `[url=${http}]`
        let b = title
        let c = "[/url]"
        return a + b + c
    }

    /**获取packageItem的绝对路径 不带后缀 */
    static getAbsPackageItemPath(data: CS.FairyEditor.FPackageItem) {
        return `${App.project.basePath}\\assets\\${data.owner.name}${data.path}${data.name}`
    }

    /**获取资源类型的后缀名 */
    static getAssetType(asset: CS.FairyEditor.FPackageItem) {
        switch (asset.type) {
            case FairyEditor.FPackageItemType.IMAGE:
                return ".png";
            case FairyEditor.FPackageItemType.ATLAS:
                return ".jpg";
            case FairyEditor.FPackageItemType.SPINE:
                return ".json"
        }
    }

    /**拼接fgui url */
    static joinUrl(pkg: string, res: string) {
        return `ui://${pkg}/${res}`;
    }

    /**创建泛型List */
    static createGenericList<T>(t: any) {
        let List = $generic(System.Collections.Generic.List$1, t);
        let list = new List<T>();
        return list
    }

    /**创建泛型字典 */
    static createGenericDictionary<T, K>(t: any, k: any) {
        let Dictionary = $generic(System.Collections.Generic.Dictionary$2, t, k);
        let dictionary = new Dictionary<T, K>();
        return dictionary;
    }


    //#region IO
    /**加载Json文件 */
    static loadJson(path: string) {
        if (System.IO.File.Exists(path) == false) {
            return null;
        }
        let f = System.IO.File.ReadAllText(path)
        return JSON.parse(f) as {}
    }

    /**保存Json文件 */
    static writerJson(path: string, msg: {} | null) {
        let config = JSON.stringify(msg)
        System.IO.File.WriteAllText(path, config)
    }

    /**写入配置 */
    static saveConfig(path: string) {
        let config = JSON.stringify(SG.config)
        System.IO.File.WriteAllText(path, config)
    }

    /**是否清理了依赖 */
    static isClearOnPublish(item: CS.FairyEditor.FPackageItem, map: Map<string, CS.FairyEditor.FPackage>, quertIndex) {
        let xml = CS.FairyEditor.XMLExtension.Load(item.file)
        let rootElements = xml.Elements()
        for (let index = 0; index < rootElements.Count; index++) {
            let child = rootElements.get_Item(index)
            if (child.name != 'displayList') continue
            let childElements = child.Elements();
            for (let index = 0; index < childElements.Count; index++) {
                const element = childElements.get_Item(index);
                if (element.name == 'loader' || element.name == 'loader3D') {
                    if (element.GetAttribute('clearOnPublish')) continue
                    let url = element.GetAttribute('url')
                    if (!url) continue
                    //没清理依赖
                    let packageItem = App.project.GetItemByURL(url)
                    if (!packageItem) {
                        App.consoleView.Log(`${ToolMgr.getUBBUrl(item.GetURL(), item.name)} 引用了失效资源 ${url}`)
                    }
                    else if (!map.has(packageItem.owner.name)) {
                        if (packageItem.type == FairyEditor.FPackageItemType.COMPONENT) {
                            quertIndex = this.isClearOnPublish(packageItem, map, quertIndex)
                        }
                        map.set(packageItem.owner.name, packageItem.owner)
                        quertIndex += 1;
                    }
                } else {
                    let src = element.GetAttribute("src")
                    if (!src) {
                        //系统资源 不需要依赖
                        continue
                    }
                    let pkg = element.GetAttribute("pkg")
                    if (!pkg) {
                        //自己依赖包的资源 所以不需要pkg
                        pkg = item.owner.id;
                    }
                    let url = `ui://${pkg}${src}`
                    if (src == null) App.consoleView.Log(element.name)
                    let packageItem = App.project.GetItemByURL(url)
                    if (!packageItem) {
                        App.consoleView.Log(`${ToolMgr.getUBBUrl(item.GetURL(), item.name)} 引用了失效资源 ${url}`)
                    }
                    else if (!map.has(packageItem.owner.name)) {
                        if (packageItem.type == FairyEditor.FPackageItemType.COMPONENT) {
                            quertIndex = this.isClearOnPublish(packageItem, map, quertIndex)
                        }
                        map.set(packageItem.owner.name, packageItem.owner)
                        quertIndex += 1;
                    }
                }
            }
        }
        return quertIndex
    }

    /**获取清理依赖后的资源 */
    static getClearOnPublishPackageItem(item: CS.FairyEditor.FPackageItem, map: Map<string, CS.FairyEditor.FPackageItem>, quertIndex) {
        let xml = CS.FairyEditor.XMLExtension.Load(item.file)
        let rootElements = xml.Elements()
        for (let index = 0; index < rootElements.Count; index++) {
            let child = rootElements.get_Item(index)
            if (child.name != 'displayList') continue
            let childElements = child.Elements();
            for (let index = 0; index < childElements.Count; index++) {
                const element = childElements.get_Item(index);
                if (element.name == 'loader' || element.name == 'loader3D') {
                    if (element.GetAttribute('clearOnPublish')) continue
                    let url = element.GetAttribute('url')
                    if (!url) continue
                    //没清理依赖
                    let packageItem = App.project.GetItemByURL(url)
                    if (!packageItem) {
                        App.consoleView.Log(`${ToolMgr.getUBBUrl(item.GetURL(), item.name)} 引用了失效资源 ${url}`)
                    }
                    else if (!map.has(packageItem.name)) {
                        if (packageItem.type == FairyEditor.FPackageItemType.COMPONENT) {
                            quertIndex = this.getClearOnPublishPackageItem(packageItem, map, quertIndex)
                        }
                        map.set(packageItem.owner.name, packageItem)
                        quertIndex += 1;
                    }
                } else {
                    let src = element.GetAttribute("src")
                    if (!src) {
                        //系统资源 不需要依赖
                        continue
                    }
                    let pkg = element.GetAttribute("pkg")
                    if (!pkg) {
                        //自己依赖包的资源 所以不需要pkg
                        pkg = item.owner.id;
                    }
                    let url = `ui://${pkg}${src}`
                    if (src == null) App.consoleView.Log(element.name)
                    let packageItem = App.project.GetItemByURL(url)
                    if (!packageItem) {
                        App.consoleView.Log(`${ToolMgr.getUBBUrl(item.GetURL(), item.name)} 引用了失效资源 ${url}`)
                    }
                    else if (!map.has(packageItem.name)) {
                        if (packageItem.type == FairyEditor.FPackageItemType.COMPONENT) {
                            quertIndex = this.getClearOnPublishPackageItem(packageItem, map, quertIndex)
                        }
                        map.set(packageItem.name, packageItem)
                        quertIndex += 1;
                    }
                }
            }
        }
        return quertIndex
    }

    /**查找失效资源 */
    static getFailureAssets(item: CS.FairyEditor.FPackageItem, map: Map<string, { url: string, failure: string[] }>, quertIndex) {
        let xml = CS.FairyEditor.XMLExtension.Load(item.file)
        let rootElements = xml.Elements()
        for (let index = 0; index < rootElements.Count; index++) {
            let child = rootElements.get_Item(index)
            if (child.name != 'displayList') continue
            let childElements = child.Elements();
            for (let index = 0; index < childElements.Count; index++) {
                const element = childElements.get_Item(index);
                if (element.name == 'loader' || element.name == 'loader3D') {
                    if (element.GetAttribute('clearOnPublish')) continue
                    let url = element.GetAttribute('url')
                    if (!url) continue
                    //没清理依赖
                    let packageItem = App.project.GetItemByURL(url)
                    if (!packageItem) {
                        if (!map.has(item.GetURL())) {
                            map.set(item.GetURL(), {
                                url: item.GetURL(),
                                failure: []
                            })
                        }
                        map.get(item.GetURL()).failure.push(url)
                    }
                    else if (!map.has(packageItem.GetURL())) {
                        if (packageItem.type == FairyEditor.FPackageItemType.COMPONENT) {
                            quertIndex = this.getFailureAssets(packageItem, map, quertIndex)
                        }
                        quertIndex += 1;
                    }
                } else {
                    let src = element.GetAttribute("src")
                    if (!src) {
                        //系统资源 不需要依赖
                        continue
                    }
                    let pkg = element.GetAttribute("pkg")
                    if (!pkg) {
                        //自己依赖包的资源 所以不需要pkg
                        pkg = item.owner.id;
                    }
                    let url = `ui://${pkg}${src}`
                    if (src == null) App.consoleView.Log(element.name)
                    let packageItem = App.project.GetItemByURL(url)
                    if (!packageItem) {
                        if (!map.has(item.GetURL())) {
                            map.set(item.GetURL(), {
                                url: item.GetURL(),
                                failure: []
                            })
                        }
                        map.get(item.GetURL()).failure.push(url)
                    }
                    else if (!map.has(packageItem.GetURL())) {
                        if (packageItem.type == FairyEditor.FPackageItemType.COMPONENT) {
                            quertIndex = this.getFailureAssets(packageItem, map, quertIndex)
                        }
                        quertIndex += 1;
                    }
                }
            }
        }
        return quertIndex
    }

    /**生成模板值 */
   static getTemplateVars(keys: Array<string>, nums: Array<string>) {
        let dic = ToolMgr.createGenericDictionary<string, string>(System.String, System.String);
        keys.forEach((key, index) => {
            dic.set_Item(key, nums[index])
        }, this)
        return dic;
    }

    static getItemsAll() {
        let count = 0;
        let pkgs = App.project.allPackages;
        pkgs.ForEach(element => {
            count += element.items.Count
        })
        return count
    }

    static sleep(delay: number) {
        return new Promise((resolve) => {
            setTimeout(() => { resolve(null) }, delay);
        });
    }

    //#endregion

}