[TOC]

# 如何本地运行

服务器: Unity\XiHNet\XIHServer\XIHServer\Server\Config\SvrConfig.cs设置服务器IP为 '127.0.0.1'

Unity: 

- Demo: 运行**Assets/Simple/Login.unity**场景

- 压测(old): 运行**Assets/Test/TestScene.unity**场景,使用按钮QWEASD试试~,具体功能看**Assets/XiHNet/Tests/TestNet.cs**代码,需自行改造

# XiHNet

Unity网络框架，包含Kcp和Tcp,<s>结合GooleProtoc3/GooleProtoc2</s>和简单加密接口，实现简单快速网络开发，内含本地测试用例

已废弃GooleProtoc3/GooleProtoc2，使用[protobuf-net](https://www.nuget.org/packages/protobuf-net)替代,若要更新`protobuf-net`请自行替换`Plugins`下的文件夹

```
Plugins
├─protobuf-net.3.0.101
├─protobuf-net.Core.3.0.101
├─System.Buffers.4.5.1
├─System.Collections.Immutable.1.7.1
├─System.Memory.4.5.4
├─System.Numerics.Vectors.4.5.0
└─System.Runtime.CompilerServices.Unsafe.4.5.3
```

#### 注意点

- 一切协议类都实现`IMessage`接口，默认该接口有`1个属性字段`，客户端无需关注，服务端务必使用**TaskId**属性

- 针对Response信息，需要添加在类属性**[RspMask]**

- 本服务器针对Request和Response属于**一一对应**的（TaskId字段一致），所以服务器务必返回相同的TaskId才能得到`非空`响应 ((await NetAdapter.SendAsync(Request)) as Response rsp)

- 若想剔除该**一一对应**规则,需将`TaskId`对应修改为`类型`对应,则TaskId属性可以删除

1. 需要修改`PBProxy.cs`中`tcsDics`字段为

```
private readonly Dictionary<MsgType, TaskCompletionSource<IMessage>> tcsDics = new Dictionary<MsgType, TaskCompletionSource<IMessage>>(64);
```

2. 修改 public Action DecodeRsp(byte[] data, MsgType rspPkt)方法

```
public Action DecodeRsp(byte[] data, MsgType rspPkt)
{
if (data == null && rspPkt == MsgType.Placement) return null;
            if (tcsDics.TryGetValue(rspPkt, out TaskCompletionSource<IMessage> tcs))
            {
                if (ProtoBuf.Serializer.Deserialize(IMessageExt.GetClassType(rspPkt), new MemoryStream(data)) is IMessage msg)
                {
                    return () => tcs.SetResult(msg);
                }
            }
            else
            {
                if (ntfDefActs.TryGetValue(rspPkt, out Action<byte[]> act))
                {
                    return () => act(data);
                }
            }
            Debugger.Log($"<color=red>未知: 无 {rspPkt} 对应的处理方法</color>");
            return null;
}
```

3. 修改 public (byte[], TaskCompletionSource<IMessage>) SendReq(IMessage req)方法

```
public (byte[], TaskCompletionSource<IMessage>) SendReq(IMessage req)
        {
 MsgType mt = req.GetMsgType();
            MemoryStream stream = new MemoryStream();
            ProtoBuf.Serializer.Serialize(stream, req);
            if (tcsDics.ContainsKey(mt))
            {
                tcsDics[mt].SetResult(null);
                Debugger.Log($"<color=green>已有新的请求{req}，将使之前任务直接置空 </color>");
            }
            tcsDics[mt] = new TaskCompletionSource<IMessage>();
            Debugger.Log($"<color=green>发送Req: </color>{req}");
            //Debugger.Log($"SendReq :len={len},stream.Position={stream.Position}, arr={BitConverter.ToString(arr)}");
            return (stream.ToArray(), tcsDics[mt]);
        }
```

# XIHServer

运行指南:

- Microsoft Visual Studio Community 2019,版本**16.8**以上，不然无法支持.Net5
- 菜单栏=》工具=》NuGet包管理器=》管理解决方案的NuGet程序包，搜索`protobuf-net`和`log4net`并安装，或使用PM安装:

```
PM> Install-Package protobuf-net -Version 3.0.101
PM> Install-Package log4net -Version 2.0.12
```

- 项目右键=》属性=》应用程序=》目标框架：.Net5.0；启动对象：XIHServer.Program
- 右键项目**XIHServer**=》设为启动项目
- 启动

> 若服务器使用AesCryptor加密方式，则需自行测试加密与客户端加密是否一致，**因为客户端UNITY和服务器框架不同**，目前encryptor.Mode = CipherMode.CBC;测试可以通用，若CipherMode.CFB则不行。

