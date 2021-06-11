[TOC]

## 本项目是由`XIHNet`和`XIHHotFix`整合的，包含`Addressable`作为资源热更新，`ILRuntime`作为C#代码热更新功能，基本实现全热更要求

### 本地运行流程

#### 服务端配置

- 打开`XIHServer/XIHServer.sln`
- 找到`XIHServer\XIHServer\Config\ServerCfg.json`,将Addr改为你本地的IP地址

- 在`解决方案`栏目右键菜单栏选择`重新生成解决方案`

- 进入生成的目录`XIHServer\Res`,分为游戏服务器`ServerBin`和资源服务器`WebBin`

> 进入`XIHServer\Res\ServerBin\net5.0`目录，打开cmd窗口，执行如下指令启动游戏服务器:
>
> ```
> dotnet XIHServer.dll
> ```
> **若是Window，也可以直接执行XIHServer.exe**

资源服务器`WebBin`可以替换为你自己的Web服务器，这只是充当资源下载功能

> 进入`XIHServer\Res\WebBin\net5.0`目录，修改`appsettings.json`的`ResRoot`字段为`"../Game"`，打开cmd窗口，执行如下指令启动Web服务器:
>
> ```
> dotnet XIHEmptyWeb.dll
> ```
> **若是Window，也可以直接执行XIHEmptyWeb.exe**

#### 客户端配置

打开Unity项目

> `Addressables` 配置

- `Addressables` 的 **Profile** 文件选择`Pro`配置

- `Addressables Group`的`Play Mode Script`选择`Use Existing Build`
- 首次打包点击`Build>New Build>Default Build Script`

> **UrlConfig**和**XIHHotFix**文件配置

- 在菜单栏打开`XIHUtil>ResCfgWindow`
- `Main Url` 填写对应Web服务器地址
- `Dll Version` 填写当前dll版本，方便热更对比
- `检测AA网络尝试的Key`默认`Assets/Bundles/CheckAANetConn.txt` 即可
- `Key所在bundle的名字` 需要在游戏运行时，点击`查看远程资源Bundle的信息,显示全部远程bundle名字`按钮，然后找到对应bundle名字填写到此
- `LoginIp` 对应游戏服务器的登录IP
- `LoginPort` 对应游戏服务器的登录端口
- `Is Kcp,Otherwise Tcp` 对应游戏服务器的协议类型KCP/TCP

**最后找到`Assets/XIHBasic/XIHBaseEnter.unity`，打开此场景，运行游戏**



### 热更注意点

#### 打包前先记得执行`ILRuntime>通过自动分析热更DLL生成CLR绑定`

#### 热更代码要求

- 尽量不要在热更类调用非热更类的泛型方法
- 热更类避免使用`lock`关键字
- 热更的Protocol协议不要使用List<T>或枚举，要替换为T[]和int表示

- 热更代码无协程，全部使用await\async方法，比较优雅。不习惯的话自行添加ILRuntime对协程的适配

