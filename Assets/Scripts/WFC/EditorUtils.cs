using UnityEditor;
using UnityEngine;

namespace WFC
{
    public static class EditorUtils
    {
        public static Texture2D GetPrefabPreviewTexture(GameObject prefab, int size)
        {
            var editor = Editor.CreateEditor(prefab);
            var prefabPath = AssetDatabase.GetAssetPath(prefab);
            var texture = editor.RenderStaticPreview(prefabPath, null, size, size);
            Object.DestroyImmediate(editor);
            
            return texture;
        }

        public static void ApplyOtherTextureInBottomRightCorner(this Texture2D originalTexture, Texture2D textureToApply)
        {
            const int padding = 5;
            var xStart = originalTexture.width - textureToApply.width - padding;
            var yStart = padding;
            
            var axesIconPixels = textureToApply.GetPixels();
            var pixelIndex = 0;
            for (var y = 0; y < textureToApply.height; y++)
            {
                for (var x = 0; x < textureToApply.width; x++)
                {
                    var axesIconPixel = axesIconPixels[pixelIndex]; 
                    if (axesIconPixel.a > float.Epsilon)
                    {
                        originalTexture.SetPixel(x + xStart, y + yStart, axesIconPixel);   
                    }
                    
                    pixelIndex++; 
                }
            }
            originalTexture.Apply();
        }
    }
}