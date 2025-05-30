using dnlib.DotNet;
using FairyGUI;
using Obfuz.ObfusPasses.SymbolObfus.Policies;
using Obfuz.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XIHRenamePolicy : ObfuscationPolicyBase
{
    HashSet<string> noReNameTypeSet;
    HashSet<string> noReNameFieldSet;

    public XIHRenamePolicy(object systemRenameObj) //Obfuz.ObfusPasses.SymbolObfus.SymbolRename
    {
        //Debug.LogWarning($"XIHRenamePolicy:{systemRenameObj}");
        
        noReNameTypeSet = new HashSet<string>() {
           typeof(XiHUI.UIDialog).FullName,
        };
        noReNameFieldSet = new HashSet<string>() {
           typeof(EventDispatcher).FullName,
           typeof(Transition).FullName,
        };
    }
    public override bool NeedRename(dnlib.DotNet.TypeDef typeDef)
    {
        if (noReNameTypeSet.Contains(typeDef.FullName))
        {
            return false;
        }
        for (TypeDef parentType = MetaUtil.GetBaseTypeDef(typeDef); parentType != null; parentType = MetaUtil.GetBaseTypeDef(parentType))
        {
            if (noReNameTypeSet.Contains(parentType.FullName))
            {
                return false;
            }
        }
        return true;
    }

    public override bool NeedRename(dnlib.DotNet.MethodDef methodDef)
    {
        //携程名字不混淆
        if (methodDef.HasReturnType && methodDef.ReturnType.FullName == typeof(IEnumerator).FullName) {
            //Debug.LogError($"NeedRename MethodDef {methodDef.Name}");
            return false;
        }
        return true;
    }

    public override bool NeedRename(dnlib.DotNet.FieldDef fieldDef)
    {
        if (noReNameFieldSet.Contains(fieldDef.FieldType.FullName))
        {
            return false;
        }
        //Debug.Log($"{fieldDef.FieldType} {fieldDef.Name}");
        if (fieldDef.FieldType is ClassSig classSig) {
            var nextType = classSig.TypeDef;
            if (nextType == null) {
                //Debug.LogWarning($"FieldType1 = {fieldDef.FieldType}\n {classSig.TypeRef}\n{classSig.TypeSpec}\n {classSig.Next}");
                nextType = classSig.TypeRef.Resolve();
            }
            if (nextType == null)
            {
                //Debug.LogError($"FieldType2 = {fieldDef.FieldType}\n {classSig.TypeRef}\n{classSig.TypeSpec}\n {classSig.Next}");
                return true;
            }
            for (TypeDef parentType = MetaUtil.GetBaseTypeDef(nextType); parentType != null; parentType = MetaUtil.GetBaseTypeDef(parentType))
            {
                //Debug.LogWarning($"{fieldDef.FieldType}:{parentType.Name} {fieldDef.Name}");
                if (noReNameFieldSet.Contains(parentType.FullName))
                {
                    //Debug.LogError ($"NeedRename FieldType2 {classSig.FullName}:{parentType.FullName} {fieldDef.Name}");
                    return false;
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
