"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
class Config {
}
exports.Config = Config;
Config.Dependency = "Dependency";
Config.ClearOnPublish = "ClearOnPublish";
Config.CopyAttribute = "CopyAttribute";
/**位置宽高进行 数学运算 */
Config.XYWHComputer = "XYWHComputer";
Config.CUSTOMNAME = "CUSTOMNAME";
Config.OPENCUSTOMNAME = 'OPENCUSTOMNAME';
Config.LookTextPath = "LookTextPath";
Config.LookPage = "LookPage";
Config.LookFontCount = "LookFontCount";
class SG {
}
exports.default = SG;
/**基础文件配置 */
SG.config = {};
SG.config_path = "";
//依赖清单本来路径
SG.query_path = "";
//依赖清单保存的游戏项目路径
SG.dependency_copy_to_path = "";
