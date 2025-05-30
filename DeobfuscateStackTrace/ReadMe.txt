https://www.obfuz.com/docs/manual/deobfuscate-stacktrace

DeobfuscateStackTrace工具
DeobfuscateStackTrace是一个基于.net 8开发的命令行工具，可以运行在Win、MacOS、Linux等所有.net支持的平台。

源码在仓库根目录下的DeobfuscateStackTrace。 可以自己编译，也可以直接从github release链接DeobfuscateStackTrace.7z中下载。

使用方式如下：

DeobfuscateStackTrace --help 查看帮助。
DeobfuscateStackTrace -m {symbol mapping file} -i {obfuscated log} -o {deobfuscate log} 命令将混淆后的堆栈日志还原为原始日志。 -m的参数是 ObfuzSettings.SymbolObfusSettings.SymbolMappingFile指向的的symbol mapping文件， -i 的参数是混淆后的日志文件， -o是输出的还原堆栈后的日志文件。
示例:

Windows 下

DeobfuscateStackTrace -m path/of/symbol-mapping.xml -i obfuscated.log -o deobfuscated.log

DeobfuscateStackTrace -m ../Assets/Obfuz/SymbolObfus/symbol-mapping.xml  -i obfuscated.log -o deobfuscated.log


MacOS或Linux下
dotnet DeobfuscateStackTrace.dll -m path/of/symbol-mapping.xml -i obfuscated.log -o deobfuscated.log
