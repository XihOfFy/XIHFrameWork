[TOC]

# ʹ�á�RSYNC���ϴ���Դ��Centos������

## Centos7����˰�װRSYNC

- ��Centosƽ̨�°�װRsync��

```
sudo yum -y install rsync
```

- ��RSync�������ļ��������ļ������¸�ʽ�޸ģ�Ȼ�󱣴�

```
sudo vim /etc/rsyncd.conf #�������ļ�
```

�޸����£�ȫ������Ȼ���滻Դ�����ļ�����

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

���������ļ��ɰ����޸ģ��Ƽ�ֻ�޸���������

**[Sync]**:  ģ������,���ж��壬֮��ͻ����ϴ���Ҫ�õ�

**path = /home/fy/xihsvr/Sync**: ��������ŵ�·��`����޸�Ϊ���Լ���`���������н�����·�����ļ��д�ţ������RootȨ���µ�Ŀ¼����Ȼ�ܶ�Ȩ�����⣬���������鷳

**secrets file = /home/fy/xihsvr/rsync.pwd**: ���пͻ����˺����뱣��·��,`����޸�Ϊ���Լ���`���ͻ��������˺������������ƥ��ſ�����������

- ��ӿͻ����˺������**rsync.pwd**�ļ�,��Ӧ·��`����޸�Ϊ���Լ���`

```
echo 'fy:fy'>/home/fy/xihsvr/rsync.pwd #fyΪ�˺ź���������룬��ð�ŷָ�������Centos7�ĵ�½�˺ź����룬������ȫ�Լ������ģ��ͻ�����Ҫ�õ�
```

- �޸�**rsync.pwd**�ļ�Ȩ��,��Ӧ·��`����޸�Ϊ���Լ���`

```
chmod 600 /home/fy/xihsvr/rsync.pwd
```

- ����Rsync����Ĭ�϶˿�873

```
sudo setenforce 0 #��ʱ�ر�SElinux
sudo systemctl start rsyncd 
```

- �鿴�Ƿ����������鿴ռ�õĶ˿��Ƿ����873�������˵�������ɹ�

```
netstat -tlunp
```

- Ϊ����������ӵ��˷������˿ڣ���Ҫ��873�˿ڱ�¶

```
firewall-cmd --zone=public --add-port=873/tcp --permanent #���˿ڱ�¶
firewall-cmd --reload #���ط���ǽ
firewall-cmd --zone=public --list-ports #�鿴����ǽ��¶�Ķ˿ڣ������873˵���ܴ�������Ӹ÷�������Rsync����
```

## �ͻ���ʹ��RSYNC

- ����Unity\XiHNet\XIHServer����cmdִ��:

```
D:\WorkSpace\Unity\XiHNet\XIHServer\Tools\cwrsync\rsync.exe -vzrtopg --password-file=./Tools/cwrsync/config/rsync.secrets --exclude-from=./Tools/cwrsync/config/exclude.txt --delete ./ fy@192.168.25.128::Sync/ --chmod=ugo=rwX
```

��������������

**D:\WorkSpace\Unity\XiHNet\XIHServer\Tools\cwrsync\rsync.exe**: rsync.exe���ڵ�Ŀ¼����Ҫ��Ϊ�Լ��ģ�

**--password-file=./Tools/cwrsync/config/rsync.secrets**: ������·��(�������˺���Ϣ)����Ӧ�˺�Ϊ**fy@192.168.25.128::Sync/**�е�**fy**

**--exclude-from=./Tools/cwrsync/config/exclude.txt**: ����ͬ�������������ļ��������������

**--delete ./**: ɾ�����������в�һ�µ��ļ���������ͻ����ļ�ͬ��

**fy@192.168.25.128::Sync/**: ʹ���˺�Ϊ**fy**ȥͬ���ļ�(`ע��fy����Centos���˺����������ڷ����/home/fy/xihsvr/rsync.pwd�ļ��ڰ������˺�`)����Ӧģ��Ϊ**Sync**(�������/etc/rsyncd.conf�ļ������õ�ģ����)

# �������Զ�˷�����

- ֱ���ڷ���˽���RSYNC��ͬ��Ŀ¼�µ�**Sync/Res/ServerBin/net5.0/**Ŀ¼��ִ��

>  ��.Net5����������ȥ���ﰲװ: [�� CentOS �ϰ�װ .NET - .NET | Microsoft Docs](https://docs.microsoft.com/zh-cn/dotnet/core/install/linux-centos)

```
[fy@localhost ~]$ cd xihsvr/Sync/Res/ServerBin/net5.0/
[fy@localhost ~]$ dotnet XIHServer.dll
```

- ���������˿ڱ�¶,���ⲿ�����ӣ������˿�����

> **12345/udp**��**54321/tcp**Ϊ`Unity\XiHNet\XIHServer\XIHServer\Server\Config\SvrConfig.cs`���õĶ˿�

```
sudo setenforce 0 #��ʱ�ر�SElinux
firewall-cmd --zone=public --add-port=12345/udp --permanent #��������TCP�˿ڱ�¶
firewall-cmd --zone=public --add-port=54321/tcp --permanent #��������KCP�˿ڱ�¶
firewall-cmd --reload #���ط���ǽ
firewall-cmd --zone=public --list-ports #�鿴����ǽ��¶�Ķ˿ڣ������˵���ܴ�������Ӹ÷���
```

