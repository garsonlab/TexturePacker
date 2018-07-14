using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 图集管理
/// By Garson https://github.com/garsonlab/
/// </summary>
public class SpriteAtlas : MonoBehaviour 
{
    private Dictionary<string, Sprite> serialized = new Dictionary<string, Sprite>();
    [SerializeField]
    private List<Sprite> sprites = new List<Sprite>();

    public int Count { get { return sprites.Count; } }

    public bool Contains(string name)
    {
        return serialized.ContainsKey(name);
    }

    public Sprite GetSprite(string name)
    {
        if (serialized.Count <= 0 && sprites.Count > 0)
        {
            foreach (var sp in sprites)
            {
                if(!serialized.ContainsKey(sp.name))
                    serialized.Add(sp.name, sp);
            }
        }
        Sprite sprite = null;
        if (serialized.TryGetValue(name, out sprite))
        {
            return sprite;
        }
        return null;
    }

    public Texture2D GetTexture(string name)
    {
        Sprite sprite = GetSprite(name);
        if (sprite)
            return sprite.texture;
        return null;
    }

    public void FillSprite(Image img, string name)
    {
        img.sprite = GetSprite(name);
    }


    #if UNITY_EDITOR
    public void AddToList(Sprite sprite)
    {
        if(!sprites.Contains(sprite))
            sprites.Add(sprite);
    }
    #endif
}
