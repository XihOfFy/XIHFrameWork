using FairyGUI;
using Obfuz.ObfusPasses.SymbolObfus.Policies;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class XIHRenamePolicy : ObfuscationPolicyBase
{
    List<Assembly> hotAssemblies;
    List<Assembly> thirdAssembies;
    List<Assembly> allAssmies;
    List<Type> noReNameType;
    List<Type> noReNameFiled;

    public XIHRenamePolicy(object systemRenameObj) //Obfuz.ObfusPasses.SymbolObfus.SymbolRename
    {
        //Debug.LogWarning($"XIHRenamePolicy:{systemRenameObj}");
        hotAssemblies = new List<Assembly>
        {
            typeof(Aot2Hot.Aot2HotMgr).Assembly,
            typeof(Hot.HotMgr).Assembly,
        };
        thirdAssembies = new List<Assembly>
        {
            typeof(FairyGUI.GObject).Assembly
        };
        allAssmies = new List<Assembly>();
        allAssmies.AddRange(hotAssemblies);
        allAssmies.AddRange(thirdAssembies);
        noReNameType = new List<Type> { 
            typeof(XiHUI.UIDialog),
            typeof(EventDispatcher),//因为自定义ui组件无法获取基类，所以基类也不混淆
        };
        noReNameFiled = new List<Type> { 
            typeof(EventDispatcher),
            typeof(Transition),
        };
    }
    public override bool NeedRename(dnlib.DotNet.TypeDef typeDef)
    {
        foreach (var ass in hotAssemblies) {
            var type = ass.GetType(typeDef.FullName, false);
            if (type != null) {
                foreach (var bt in noReNameType) {
                    if (bt.IsAssignableFrom(type)) {
                        //Debug.LogWarning($"Skip NeedRename TypeDef  {typeDef.FullName} {typeDef.ReflectionFullName}");
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public override bool NeedRename(dnlib.DotNet.MethodDef methodDef)
    {
        return true;
    }

    public override bool NeedRename(dnlib.DotNet.FieldDef fieldDef)
    {
        foreach (var ass in allAssmies)
        {
            var type = ass.GetType(fieldDef.FieldType.FullName, false);
            if (type != null)
            {
                foreach (var bt in noReNameFiled)
                {
                    if (bt.IsAssignableFrom(type))
                    {
                        Debug.LogWarning($"Skip NeedRename FieldDef  {fieldDef.FieldType.FullName}");
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public override bool NeedRename(dnlib.DotNet.PropertyDef propertyDef)
    {
        return true;
    }

    public override bool NeedRename(dnlib.DotNet.EventDef eventDef)
    {
        return true;
    }
}
