using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "TextureHalfSchema", menuName = "YooAssetArt/Create TextureHalfSchema")]
public class TextureHalfSchema : BaseTextureSchema
{
    public override int GetMaxTextSize(TextureImporter importer)
    {
        importer.GetSourceTextureWidthAndHeight(out var width, out var height);
        var nativeSize = Mathf.NextPowerOfTwo(Mathf.Max(width, height));
        var maxTextureSize = 4096;
        return Mathf.Min(nativeSize, maxTextureSize) >> 1;
    }
}
