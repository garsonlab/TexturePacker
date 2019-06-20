using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Sprites;
using UnityEngine;

public class ViewAtlas : EditorWindow
{
    private static int selected = -1;
    private static string[] infoNamesArray;
    private static Texture2D texture;

    Vector2 posLeft;
    Vector2 posRight;
    GUIStyle normalStyle;
    GUIStyle selectStyle;
    float splitterPos = 300;
    Rect splitterRect;
    Vector2 dragStartPos;
    bool dragging;
    float splitterWidth = 5;


    [MenuItem("Tools/快速浏览图集")]
    static void ShowWin()
    {
        if (EditorSettings.spritePackerMode != SpritePackerMode.AlwaysOn)
        {
            if (!EditorUtility.DisplayDialog("Worroming", "当前图集打包模式非 AlwaysOn, 是否更改？", "更改", "取消"))
            {
                return;
            }
        }

        string tag = GetSelectPackingTag();
        if (infoNamesArray == null)
            GetAtlasNames();

        GetWindow<ViewAtlas>().Show();

        if (!string.IsNullOrEmpty(tag) && infoNamesArray != null)
        {
            int idx = -1;

            for (int i = 0; i < infoNamesArray.Length; i++)
            {
                if (infoNamesArray[i] == tag)
                {
                    idx = i;
                    break;
                }
                else if (idx < 0 && infoNamesArray[i].StartsWith(tag))
                {
                    idx = i;
                }
            }

            if (idx >= 0)
            {
                SetSelectAtlas(idx);
            }
        }

        if (selected < 0 && infoNamesArray != null && infoNamesArray.Length > 0)
        {
            SetSelectAtlas(0);
        }
    }

    static string GetSelectPackingTag()
    {
        var obj = Selection.activeObject;
        if (obj == null)
            return null;
        string path = AssetDatabase.GetAssetPath(obj);
        if (!path.ToLower().EndsWith(".png"))
            return null;
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);

        return importer.spritePackingTag;
    }

    static void GetAtlasNames()
    {
        EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOn;
        Packer.RebuildAtlasCacheIfNeeded(EditorUserBuildSettings.activeBuildTarget, true);
        //EditorApplication.ExecuteMenuItem("Window/Sprite Packer");

        //var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.Sprites.PackerWindow");
        //var window = EditorWindow.GetWindow(type);
        //FieldInfo infoNames = type.GetField("m_AtlasNames", BindingFlags.NonPublic | BindingFlags.Instance);
        //infoNamesArray = (string[])infoNames.GetValue(window);

        infoNamesArray = Packer.atlasNames;
    }



    void OnGUI()
    {
        if (normalStyle == null)
            normalStyle = new GUIStyle() { alignment = TextAnchor.LowerLeft, };
        if (selectStyle == null)
        {
            selectStyle = new GUIStyle();
            selectStyle.alignment = TextAnchor.LowerLeft;
            selectStyle.normal.background = Resources.Load<Texture2D>("selected");
            selectStyle.wordWrap = true;
        }

        GUILayout.BeginHorizontal();
        // Left view
        posLeft = GUILayout.BeginScrollView(posLeft,
            GUILayout.Width(splitterPos),
            GUILayout.MaxWidth(splitterPos),
            GUILayout.MinWidth(splitterPos));



            DrawLeft();
        GUILayout.EndScrollView();

        // Splitter
        GUILayout.Box("",
            GUILayout.Width(splitterWidth),
            GUILayout.MaxWidth(splitterWidth),
            GUILayout.MinWidth(splitterWidth),
            GUILayout.ExpandHeight(true));
        splitterRect = GUILayoutUtility.GetLastRect();
        

        // Right view
        posRight = GUILayout.BeginScrollView(posRight,
            GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        DrawRight();
        GUILayout.EndScrollView();

        GUILayout.EndHorizontal();

        // Splitter events
        if (Event.current != null)
        {
            switch (Event.current.rawType)
            {
                case EventType.MouseDown:
                    if (splitterRect.Contains(Event.current.mousePosition))
                    {
                        dragging = true;
                    }
                    break;
                case EventType.MouseDrag:
                    if (dragging)
                    {
                        splitterPos += Event.current.delta.x;
                        splitterPos = Mathf.Clamp(splitterPos, 200, 400);
                        Repaint();
                    }
                    break;
                case EventType.MouseUp:
                    if (dragging)
                    {
                        dragging = false;
                    }
                    break;
            }
        }
    }

    void DrawLeft()
    {
        GUILayout.BeginVertical();
        if (GUILayout.Button("Refresh", GUILayout.Width(60)))
        {
            selected = -1;
            GetAtlasNames();
            if (infoNamesArray != null && infoNamesArray.Length > 0)
            {
                SetSelectAtlas(0);
            }
            return;
        }
        GUILayout.Space(8);

        if (infoNamesArray == null || infoNamesArray.Length < 0)
            return;
        
        for (int i = 0; i < infoNamesArray.Length; i++)
        {
            int idx = i;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(infoNamesArray[i], i == selected? selectStyle : normalStyle))
            {
                SetSelectAtlas(idx);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }

    void DrawRight()
    {
        GUILayout.BeginVertical();
        if (selected >= 0)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("View In SpritePacker Windom", GUILayout.Width(200)))
            {
                ShowInPackerWindom(selected);
            }
            GUILayout.Space(10);
            GUILayout.Label("Atlas Name:\t" + infoNamesArray[selected]);
            GUILayout.EndHorizontal();
        }

        if (texture != null)
        {
            GUILayout.Space(20);

            GUILayout.Label(String.Format("{0}x{1},{2}", texture.width, texture.height, texture.format));
            float zoom = 1;
            if (texture.width > 1024)
                zoom = 1024 * 1.0f / texture.width;
            if (texture.height > 512)
                zoom = Mathf.Min(zoom, 512 * 1.0f / texture.height);


            Rect rect = new Rect(0, 60, texture.width*zoom, texture.height*zoom);

            EditorGUI.DrawTextureTransparent(rect, texture);
        }

        GUILayout.EndVertical();
    }



    static void SetSelectAtlas(int idx)
    {
        texture = null;

        selected = idx;
        string atlasName = infoNamesArray[idx];
        Texture2D[] texturesForAtlas = Packer.GetTexturesForAtlas(atlasName);
        if(texturesForAtlas.Length <= 0)
            return;
        texture = texturesForAtlas[0];
        
    }

    static void ShowInPackerWindom(int idx)
    {
        EditorApplication.ExecuteMenuItem("Window/Sprite Packer");
        var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.Sprites.PackerWindow");
        var window = EditorWindow.GetWindow(type);

        FieldInfo info = type.GetField("m_SelectedAtlas", BindingFlags.NonPublic | BindingFlags.Instance);
        info.SetValue(window, idx);

    }

    
}
