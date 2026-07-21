using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ModelTools
{
    [MenuItem("GameUtil/ModelImportSet")]
    static void SetOptimize()
    {
        var resPath = "Assets/Res/Models";
        string[] allFBX = AssetDatabase.FindAssets("t:Model", new[] { resPath });
        int totalCount = 0;
        int changedCount = 0;
        int unchangedCount = 0;
        int preservedRigAndAnimationCount = 0;

        foreach (string guid in allFBX)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
            {
                totalCount++;
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null)
                {
                    unchangedCount++;
                    continue;
                }

                bool preserveRigAndAnimation = HasSkeletonOrAnimation(path);
                if (preserveRigAndAnimation)
                {
                    preservedRigAndAnimationCount++;
                }

                if (ApplySettings(importer, preserveRigAndAnimation))
                {
                    importer.SaveAndReimport();
                    changedCount++;
                }
                else
                {
                    unchangedCount++;
                }
            }
        }

        AssetDatabase.Refresh();
        Debug.LogFormat(
            "ModelImportSet complete. FBX: {0}, changed: {1}, unchanged: {2}, preserved Rig/Animation: {3}.",
            totalCount,
            changedCount,
            unchangedCount,
            preservedRigAndAnimationCount);
    }

    private static bool ApplySettings(ModelImporter importer, bool preserveRigAndAnimation)
    {
        bool changed = false;

        if (!preserveRigAndAnimation)
        {
            changed |= SetValue(importer.animationType, ModelImporterAnimationType.None, value => importer.animationType = value);
            changed |= SetValue(importer.importAnimation, false, value => importer.importAnimation = value);
            changed |= SetValue(importer.importConstraints, false, value => importer.importConstraints = value);
            changed |= SetValue(importer.importBlendShapes, false, value => importer.importBlendShapes = value);
            changed |= SetValue(importer.importVisibility, false, value => importer.importVisibility = value);
        }

        changed |= SetValue(importer.materialImportMode, ModelImporterMaterialImportMode.None, value => importer.materialImportMode = value);
        changed |= SetValue(importer.importCameras, false, value => importer.importCameras = value);
        changed |= SetValue(importer.importLights, false, value => importer.importLights = value);
        changed |= SetValue(importer.meshCompression, ModelImporterMeshCompression.Low, value => importer.meshCompression = value);
        changed |= SetValue(importer.isReadable, false, value => importer.isReadable = value);

        return changed;
    }

    private static bool HasSkeletonOrAnimation(string path)
    {
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (model != null)
        {
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                Transform[] bones = skinnedMeshRenderer.bones;
                if (skinnedMeshRenderer.rootBone != null || (bones != null && bones.Length > 0))
                {
                    return true;
                }
            }
        }

        foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
        {
            AnimationClip clip = asset as AnimationClip;
            if (clip != null && !clip.empty && !clip.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool SetValue<T>(T currentValue, T nextValue, Action<T> setter)
    {
        if (EqualityComparer<T>.Default.Equals(currentValue, nextValue))
        {
            return false;
        }

        setter(nextValue);
        return true;
    }
}
