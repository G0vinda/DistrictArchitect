using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace WFC.Editor
{
    public static class EditorUtils
    {
        public static Texture2D GetPrefabPreviewTexture(GameObject prefab, int size)
        {
            var editor = UnityEditor.Editor.CreateEditor(prefab);
            var prefabPath = AssetDatabase.GetAssetPath(prefab);
            var texture = editor.RenderStaticPreview(prefabPath, null, size, size);
            Object.DestroyImmediate(editor);
            
            return texture;
        }

        public static Image GenerateImage(Texture2D texture)
        {
            return new Image
            {
                style =
                {
                    width = texture.width,
                    height = texture.height,
                },
                image = texture,
                scaleMode = ScaleMode.StretchToFill,
                tintColor = Color.white
            };
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