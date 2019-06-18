using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CombineSprites : Editor
{
    // 是否去除原始图片周围的透明
    private static bool _trimBorder = true;

    [MenuItem("Tools/CustomAtlas")]
    static void Combine()
    {
        Texture2D[] sprites = Selection.GetFiltered<Texture2D>(SelectionMode.DeepAssets);
        if (sprites.Length == 0)
        {
            EditorUtility.DisplayDialog("", "未选中任何图片", "OK");
            return;
        }

        if (EditorUtility.DisplayDialog("提示", "是否去除原图片透明边框, 去除后可能会减小图集大小", "当然", "算了"))
            _trimBorder = true;
        else
            _trimBorder = false;

        if (_trimBorder)
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                sprites[i] = TrimBorder(sprites[i]);
            }
        }

        Texture2D texture = new Texture2D(1024, 1024, TextureFormat.ARGB32, false);
        Rect[] rects = texture.PackTextures(sprites, 1);

        string path = EditorUtility.SaveFilePanel("SavePath", "Assets/Res", "atlas", "png");
        if (!string.IsNullOrEmpty(path))
        {
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            if (!path.Contains("Assets/"))
                return;

            path = "Assets/" + path.Split(new []{"Assets/"}, StringSplitOptions.None)[1];
            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();
            
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;

            int width = texture.width;
            int height = texture.height;

            SpriteMetaData[] datas = new SpriteMetaData[rects.Length];
            for (int j = 0; j < rects.Length; j++)
            {
                Rect rect = rects[j];
                rect.x *= width;
                rect.y *= height;
                rect.width *= width;
                rect.height *= height;

                var data = new SpriteMetaData();
                data.name = sprites[j].name;
                data.rect = rect;
                data.pivot = Vector2.one*0.5f;
                data.border = Vector4.zero;
                datas[j] = data;
            }

            importer.spritesheet = datas;
            AssetDatabase.ImportAsset(path);

            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
        }
    }

    static Texture2D TrimBorder(Texture2D texture)
    {
        int left = 0, right = texture.width - 1, bottom = 0, top = texture.height - 1;

        for (int i = 0; i < texture.width; i++)
        {
            bool isTransparent = true;
            for (int j = 0; j < texture.height; j++)
            {
                if (texture.GetPixel(i, j).a > 0)
                {
                    isTransparent = false;
                    break;
                }
            }

            if (isTransparent)
                left = i + 1;
            else
                break;
        }

        for (int i = texture.width - 1; i > left; i--)
        {
            bool isTransparent = true;
            for (int j = 0; j < texture.height; j++)
            {
                if (texture.GetPixel(i, j).a > 0)
                {
                    isTransparent = false;
                    break;
                }
            }

            if (isTransparent)
                right = i - 1;
            else
                break;
        }

        for (int i = 0; i < texture.height; i++)
        {
            bool isTransparent = true;
            for (int j = 0; j < texture.width; j++)
            {
                if (texture.GetPixel(j, i).a > 0)
                {
                    isTransparent = false;
                    break;
                }
            }

            if (isTransparent)
                bottom = i + 1;
            else
                break;
        }

        for (int i = texture.height-1; i > bottom; i--)
        {
            bool isTransparent = true;
            for (int j = 0; j < texture.width; j++)
            {
                if (texture.GetPixel(j, i).a > 0)
                {
                    isTransparent = false;
                    break;
                }
            }

            if (isTransparent)
                top = i - 1;
            else
                break;
        }

        Texture2D result = new Texture2D(right-left+1, top-bottom+1);
        result.name = texture.name;
        result.SetPixels(texture.GetPixels(left, bottom, result.width, result.height));
        return result;
    }
    
}
