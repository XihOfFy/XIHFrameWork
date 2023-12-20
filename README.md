# XIHFrameWork

> 整合一些快捷开发的插件: HybridCLR、YooAsset、UniTask、DOTween、Unity-Logs-Viewer和RuntimeInspector
> 
> 添加一些功能插件: FileEncode、XIHWebServer

## 目录结构

- Aot2HotScripts: 热更首个执行的代码，AOT跳转HOT前，需要通过这个来更新全部资源，也可以热更（重启生效）

- AotRes：AOT场景和资源，启动场景

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

- 调整`Assets/Resources/{nameof(XIHFrontSetting)}.asset`中的web地址路径（或者删除，运行后重新生成本地Web路径），若存在`XIHWebServerRes\Front`文件夹，先全部删除，第一次运行会自动生成
  
  Windows下菜单栏 XIHUtil/Server/WebSvr 即可开启本地web服务；
  
  Mac用户请自行搭建web服务，且设置web根路径为 XIHWebServerRes (与Assets同层级)

- 安装HybridCLR，点击菜单 `HybridCLR/Installer...`

- 若是webgl开发，根据这个来设置Hyclr的路径: [发布WebGL平台 | HybridCLR (code-philosophy.com)](https://hybridclr.doc.code-philosophy.com/docs/basic/buildwebgl)

> 在HybridCLRSettings中，开启`Use Global Il2cpp` 选项。因为webgl平台只支持全局安装。
> 
> 建立 Editor目录的libil2cpp到本地libil2cpp目录的软（硬）引用。升级hybridclr等情形需要重新install时，先恢复Editor安装目录的原始libil2cpp目录

- 执行菜单栏`XIHUtil/Jenkins/HotBuild`生成热更包，可在编辑器看到运行效果

- 若要打整包测试，执行菜单栏`XIHUtil/Jenkins/FullBuild`

- 远程资源部署方案参考下面的**资源部署**

# 推荐版本

- 微信小游戏： [下载与安装 | Unity小游戏](https://unity.cn/instantgame/docs/WechatMinigame/InstallUnity/)

## 小游戏优化和设置

- [案例： Endless Runner | Unity小游戏](https://unity.cn/instantgame/docs/WechatMinigame/Demo/)

## 资源部署

- 执行菜单栏`XIHUtil/Jenkins/HotBuild`生成热更包后，在`XIHWebServerRes`文件夹内存在2个文件夹`Front`和`WebGL`，将其原封不动放到远程CDN根目录下面即可
- `Front`下的json记得修改为远程的url和cdn

# 

## 解决

- 对于打包报错，记得检查link.xml是否正确裁剪，有些程序集不能全部保留，必须裁剪掉一部分才能在webgl运行，例如`UnityEngine.CoreModule`不能全保留
- 微信小游戏缓存设置

```

```
