# XIHFrameWork

> 整合一些快捷开发的插件: HybridCLR、YooAsset、UniTask、DOTween、Unity-Logs-Viewer和RuntimeInspector
> 
> 添加一些功能插件: FileEncode、XIHWebServer

## 目录结构

- Aot2HotScripts: 热更首个执行的代码，AOT跳转HOT前，需要通过这个来更新全部资源，也可以热更（重启生效）

- AotScripts: AOT代码，启动后只更新Aot2Hot的部分资源，快速跳到热更更新场景

- Editor：编辑器代码，包含一些菜单栏功能

- HotScripts：热更代码主体，后期开发都在这里

- Res：资源放置路径，包含代码资源和场景和unity其他资源

- Resources：随包资源

- StreamingAssets：不压缩的资源，也是随包

- TextMesh Pro：TextMesh 资源

- WebGLTemplates：微信小游戏插件

- WX-WASM-SDK-V2：微信小游戏插件

## 首次运行

- 调整`AotConfig`代码中的web地址路径，若存在`XIHWebServerRes\Front`文件夹，先全部删除，第一次运行会自动生成
  
  Windows下菜单栏 XIHUtil/Server/WebSvr 即可开启本地web服务；
  
  Mac用户请自行搭建web服务，且设置web根路径为 XIHWebServerRes (与Assets同层级)

- 安装HybridCLR，点击菜单 `HybridCLR/Installer...`

- 若是webgl开发，根据这个来设置Hyclr的路径: [发布WebGL平台 | HybridCLR (code-philosophy.com)](https://hybridclr.doc.code-philosophy.com/docs/basic/buildwebgl)

> 在HybridCLRSettings中，开启`Use Global Il2cpp` 选项。因为webgl平台只支持全局安装。
> 
> 建立 Editor目录的libil2cpp到本地libil2cpp目录的软（硬）引用。升级hybridclr等情形需要重新install时，先恢复Editor安装目录的原始libil2cpp目录

# 推荐版本

- 微信小游戏： [下载与安装 | Unity小游戏](https://unity.cn/instantgame/docs/WechatMinigame/InstallUnity/)

## 小游戏优化和设置

- [案例： Endless Runner | Unity小游戏](https://unity.cn/instantgame/docs/WechatMinigame/Demo/)

## 资源部署

- Yooasset打包的AB全部拷贝到下载目录`XIHWebServerRes\WebGL`中

- StreamingAssets也是全部拷贝到`XIHWebServerRes\WebGL\StreamingAssets`中

# 

## 解决

- 原生webgl遇到最多是这样的报错：Uncaught (in promise) TypeError: Module.addRunDependency is not a function
      at unityFileSystemInit (74a3274d-022f-4ba2-99ba-26e65fb031ea:3:1756)
      at 74a3274d-022f-4ba2-99ba-26e65fb031ea:3:1994
      at callRuntimeCallbacks (74a3274d-022f-4ba2-99ba-26e65fb031ea:3:18987)
      at preRun (74a3274d-022f-4ba2-99ba-26e65fb031ea:3:14367)
      at run (74a3274d-022f-4ba2-99ba-26e65fb031ea:3:522847)
      at runCaller (74a3274d-022f-4ba2-99ba-26e65fb031ea:3:522184)
      at removeRunDependency (74a3274d-022f-4ba2-99ba-26e65fb031ea:3:15690)
      at receiveInstance (74a3274d-022f-4ba2-99ba-26e65fb031ea:3:17420)
      at receiveInstantiationResult (74a3274d-022f-4ba2-99ba-26e65fb031ea:3:17541)
  unityFileSystemInit @ 74a3274d-022f-4ba2-99ba-26e65fb031ea:3

- 其次Yooasset的错误：Exception: The manifest file format is invalid !

- 必须转为小游戏才能正常运行
