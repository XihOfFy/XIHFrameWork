using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "SpriteAtlasHalfSchema", menuName = "YooAssetArt/Create SpriteAtlasHalfSchema")]
public class SpriteAtlasHalfSchema : BaseSpriteAtlasSchema
{
    public override int GetMaxTextSize(TextureImporter importer)
    {
        importer.GetSourceTextureWidthAndHeight(out var width, out var height);
        var nativeSize = Mathf.NextPowerOfTwo(Mathf.Max(width, height));
        var maxTextureSize = 4096;
        return Mathf.Min(nativeSize, maxTextureSize) >> 1;
    }
}
