using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "TextureNormalSchema", menuName = "YooAssetArt/Create TextureNormalSchema")]
public class TextureNormalSchema : BaseTextureSchema
{
    public override int GetMaxTextSize(TextureImporter importer)
    {
        importer.GetSourceTextureWidthAndHeight(out var width, out var height);
        var nativeSize = Mathf.NextPowerOfTwo(Mathf.Max(width, height));
        var maxTextureSize = 4096;
        return Mathf.Min(nativeSize, maxTextureSize);
    }
}
