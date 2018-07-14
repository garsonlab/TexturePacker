#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TexturePacker
{
    /// <summary>
    /// 图集编辑器，用于手动构建图集，仿Unity2017 SpriteAtlas
    /// By Garson https://github.com/garsonlab/
    /// </summary>
    public class UIAtlas : ScriptableObject
    {
        public string savePath = "Assets/";
        public AtlasSize atlasSize = AtlasSize.Atlas_64x64;
        public List<Object> objectsForPacking = new List<Object>();

        public Vector2 genreteSize;
        public Texture2D preview;
        public SpriteAtlas spriteAtlas;

        public enum AtlasSize
        {
            Atlas_32x32 = 32,
            Atlas_64x64 = 64,
            Atlas_128x128 = 128,
            Atlas_256x512 = 512,
            Atlas_1024x1024 = 1024,
            Atlas_2048x2048 = 2048,
        }
    }
    
}
#endif