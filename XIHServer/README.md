[TOC]

# 使用【RSYNC】上传资源到Centos服务器

## Centos7服务端安装RSYNC

- 在Centos平台下安装Rsync：

```
sudo yum -y install rsync
```

- 打开RSync的配置文件，并将文件按如下格式修改，然后保存

```
sudo vim /etc/rsyncd.conf #打开配置文件
```

修改如下，全部复制然后替换源配置文件即可

```
fake super = yes
log file = /var/log/rsyncd.log
uid = root
gid = root
use chroot = no
port = 873
read only = no
write only = no
[Sync]
path = /home/fy/xihsvr/Sync
comment = Sync
secrets file = /home/fy/xihsvr/rsync.pwd
list = yes
```

以上配置文件可按需修改，推荐只修改如下配置

**[Sync]**:  模块名字,自行定义，之后客户端上传需要用到

**path = /home/fy/xihsvr/Sync**: 服务器存放的路径`务必修改为你自己的`，可以自行建立新路径和文件夹存放，别放在Root权限下的目录，不然很多权限问题，处理起来麻烦

**secrets file = /home/fy/xihsvr/rsync.pwd**: 所有客户端账号密码保存路径,`务必修改为你自己的`，客户端连接账号密码与该配置匹配才可以正常连接

- 添加客户端账号密码进**rsync.pwd**文件,对应路径`务必修改为你自己的`

```
echo 'fy:fy'>/home/fy/xihsvr/rsync.pwd #fy为账号后面的是密码，以冒号分割，这个不是Centos7的登陆账号和密码，这是完全自己决定的，客户端需要用到
```

- 修改**rsync.pwd**文件权限,对应路径`务必修改为你自己的`

```
chmod 600 /home/fy/xihsvr/rsync.pwd
```

- 启动Rsync服务，默认端口873

```
sudo setenforce 0 #临时关闭SElinux
sudo systemctl start rsyncd 
```

- 查看是否启动，并查看占用的端口是否包含873，如果有说明启动成功

```
netstat -tlunp
```

- 为了外界能连接到此服务器端口，需要将873端口暴露

```
firewall-cmd --zone=public --add-port=873/tcp --permanent #将端口暴露
firewall-cmd --reload #重载防火墙
firewall-cmd --zone=public --list-ports #查看防火墙暴露的端口，如果有873说明能从外界连接该服务器的Rsync服务
```

## 客户端使用RSYNC

- 进入Unity\XiHNet\XIHServer，打开cmd执行:

```
D:\WorkSpace\Unity\XiHNet\XIHServer\Tools\cwrsync\rsync.exe -vzrtopg --password-file=./Tools/cwrsync/config/rsync.secrets --exclude-from=./Tools/cwrsync/config/exclude.txt --delete ./ fy@192.168.25.128::Sync/ --chmod=ugo=rwX
```

解释下上面的命令：

**D:\WorkSpace\Unity\XiHNet\XIHServer\Tools\cwrsync\rsync.exe**: rsync.exe所在的目录（需要改为自己的）

**--password-file=./Tools/cwrsync/config/rsync.secrets**: 密码存放路径(不包含账号信息)，对应账号为**fy@192.168.25.128::Sync/**中的**fy**

**--exclude-from=./Tools/cwrsync/config/exclude.txt**: 无需同步到服务器的文件，可以自行添加

**--delete ./**: 删除服务器所有不一致的文件，保持与客户端文件同步

**fy@192.168.25.128::Sync/**: 使用账号为**fy**去同步文件(`注意fy不是Centos的账号名，而是在服务端/home/fy/xihsvr/rsync.pwd文件内包含的账号`)，对应模块为**Sync**(即服务端/etc/rsyncd.conf文件内配置的模块名)

# 如何运行远端服务器

- 直接在服务端进入RSYNC的同步目录下的**Sync/Res/ServerBin/net5.0/**目录，执行

>  无.Net5环境可以先去这里安装: [在 CentOS 上安装 .NET - .NET | Microsoft Docs](https://docs.microsoft.com/zh-cn/dotnet/core/install/linux-centos)

```
[fy@localhost ~]$ cd xihsvr/Sync/Res/ServerBin/net5.0/
[fy@localhost ~]$ dotnet XIHServer.dll
```

- 将服务器端口暴露,让外部能连接，其他端口类似

> **12345/udp**、**54321/tcp**为`Unity\XiHNet\XIHServer\XIHServer\Server\Config\SvrConfig.cs`配置的端口

```
sudo setenforce 0 #临时关闭SElinux
firewall-cmd --zone=public --add-port=12345/udp --permanent #将服务器TCP端口暴露
firewall-cmd --zone=public --add-port=54321/tcp --permanent #将服务器KCP端口暴露
firewall-cmd --reload #重载防火墙
firewall-cmd --zone=public --list-ports #查看防火墙暴露的端口，如果有说明能从外界连接该服务
```

