### 支持ILRuntime 的 protobuf-net

把 src 里面的 protobuf-net 编译或直接放在 unity Assets 中 删掉.csproj
**已知问题 proto2 optional和枚举 不能使用，由于ILRuntime不认为ILRT里面跑的数据类型是枚举，导致无法把默认值转换为枚举。**


Unity中使用 需要注册一下 

```cs
static bool InitedILRuntime = false;
static IMethod s_HFInitialize;
static IMethod s_HFUpdate;
static ILRuntime.Runtime.Enviorment.AppDomain HFDomain;

static void InitializeILRuntimeCLR()
{
    ProtoBuf.PType.RegisterFunctionCreateInstance(PType_CreateInstance);
    ProtoBuf.PType.RegisterFunctionGetRealType(PType_GetRealType);
}

static void Initialize()
{
    var hfMain = HFDomain.GetType("HotFix.Main");
    s_HFInitialize = hfMain.GetMethod("Initialize", 0);
    s_HFUpdate = hfMain.GetMethod("Update", 0);
    HFDomain.Invoke(s_HFInitialize, null, null);
}

public static void Update()
{
    if (InitedILRuntime)
    {
        HFDomain.Invoke(s_HFUpdate, null, null);
    }
}

static object PType_CreateInstance(string typeName)
{
    return HFDomain.Instantiate(typeName);
}

static Type PType_GetRealType(object o)
{
    var type = o.GetType();
    if (type.FullName == "ILRuntime.Runtime.Intepreter.ILTypeInstance")
    {
        var ilo = o as ILRuntime.Runtime.Intepreter.ILTypeInstance;
        type = ProtoBuf.PType.FindType(ilo.Type.FullName);
    }
    return type;
}
```


Dll 中使用 参考 hotfix目录下main.cs

```cs
public static void Initialize()
{
    ILRuntime_mmopb.Initlize();
    Debug.Log("Initialize");
}

public static void Update()
{
    if (!s_Initialized) return;
    var c = new mmopb.m_login_c();
    c.account = new mmopb.p_account_c();
    c.account.account = "abc";
    c.account.snapshots.Add(new mmopb.p_avatar_snapshot());
    c.account.snapshots.Add(new mmopb.p_avatar_snapshot());
    var s = new mmopb.p_avatar_snapshot();
    s.avatar = new mmopb.p_entity_basis();
    s.avatar.account = "defxxx";
    c.account.snapshots.Add(s);
    c.account.snapshots.Add(s);
    var stream = new System.IO.MemoryStream();
    ProtoBuf.Serializer.Serialize(stream, c);
    Debug.Log(stream.Length);
    var bytes = stream.ToArray();
    var t = ProtoBuf.Serializer.Deserialize(typeof (mmopb.m_login_c), new System.IO.MemoryStream(bytes)) as mmopb.m_login_c;
    Debug.Log(t.account.snapshots.Count);
    Debug.Log("Update" + t.account.snapshots[3].avatar.account);
}
```

## 更新日志

### 2019.06.06 

增加非反射调用，需要把如下代码放入热更工程，git clone 下来看着就正常了

```cs
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Helper
{
    public sealed class ProtoHelper
    {
        public static byte[] EncodeWithName(object p)
        {
            var type = p.GetType();
            var name = type.FullName;
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, p);
                //var cos = new Google.Protobuf.CodedOutputStream(ms);
                //((IMessage)p).Encode(cos); //非反射方式
                //cos.Flush(); 
                var nbs = Encoding.UTF8.GetBytes(name);
                int nblen = nbs.Length;
                if (nblen > 255)
                {
                    throw new Exception("PB:name->" + name + " is To Long " + nblen + " > 255");
                }
                var buffer = new byte[ms.Length + nbs.Length + 1];
                buffer[0] = (byte)((nblen >> 0) & 0xFF);
                Buffer.BlockCopy(nbs, 0, buffer, 1, nblen);
                ms.Position = 0;
                ms.Read(buffer, 1 + nblen, (int)ms.Length);
                return buffer;
            }
        }

		public static object DecodeWithName(byte[] b, out string name)
		{
    		var bytesLen = b[0];
    		name = Encoding.UTF8.GetString(b, 1, bytesLen);
    		using (var ms = new MemoryStream(b, 1 + bytesLen, b.Length - 1 - bytesLen))
    		{
        		Type T = Type.GetType(name);
        		if (name.Contains(".")) { name = name.Substring(name.LastIndexOf('.') + 1); }
        		return Serializer.Deserialize(T, ms);
        		//var o = Activator.CreateInstance(T);
        		//((IMessage)o).Decode(new Google.Protobuf.CodedInputStream(ms));  //非反射方式
        		//return o;
    		}
		}
	}

	public interface IProtoRecv
	{
    	void OnRecv(string name, object o);
	}

	public interface IMessage
	{
    	void Encode(Google.Protobuf.CodedOutputStream writer);
    	void Decode(Google.Protobuf.CodedInputStream reader);
	}
}
```



### 2018.05.25

更新Protobuf-net



### 2017.10.31 

枚举是确定不能用的，proto2 optional(optional int32 不支持，optional int64 bool string 可以) （推荐用proto3）
src 目录没时间，暂时就不同步了，需要的同学直接拷贝Protobuf目录到U3D就好。



### 2017.10.10

修复对map的支持，现在支持任意map(map<任意类型,任意类型>)。

Mac的同学可以直接使用Protobuf文件夹,把导出的工具做成U3D的Editor了，不需要编译了，导出也比较方便。

