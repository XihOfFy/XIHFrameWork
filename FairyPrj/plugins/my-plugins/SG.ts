export class Config {
    static Dependency = "Dependency"
    static ClearOnPublish = "ClearOnPublish"
    static CopyAttribute = "CopyAttribute"
    /**位置宽高进行 数学运算 */
    static XYWHComputer = "XYWHComputer"

    static CUSTOMNAME = "CUSTOMNAME"

    static OPENCUSTOMNAME = 'OPENCUSTOMNAME'

    static LookTextPath = "LookTextPath"

    static LookPage = "LookPage"

    static LookFontCount = "LookFontCount"
}
export default class SG {
    /**基础文件配置 */
    static config = {};
    
    static config_path = ""
    //依赖清单本来路径
    static query_path = ""
    //依赖清单保存的游戏项目路径
    static dependency_copy_to_path = ""
}