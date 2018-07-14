using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace TexturePacker
{
    /// <summary>
    /// 图集编辑器，用于手动构建图集，仿Unity2017 SpriteAtlas
    /// By Garson https://github.com/garsonlab/
    /// </summary>
    [CustomEditor(typeof(UIAtlas))]
    public class UIAtlasEditor : Editor
    {
        private UIAtlas atlas;
        private SerializedProperty list;

        public override void OnInspectorGUI()
        {
            if (!atlas)
                atlas = (UIAtlas) target;
            if(list == null)
                list = serializedObject.FindProperty("objectsForPacking");
            serializedObject.Update();
            EditorGUILayout.HelpBox("You Can Select Folder or Texture Here To Packger Atlas.\n Remember to Select Save Path and Click Packer Button.", MessageType.Info);

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Save Path:");
            GUILayout.Label(atlas.savePath);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Folder To Save Atlas", atlas.savePath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    string[] ps = path.Split(new[] {"Assets/"}, StringSplitOptions.None);
                    if (ps.Length == 2)
                    {
                        atlas.savePath = "Assets/" + ps[1];
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Note", "You Can Select Assets Folder Only!", "Ok, I will.");
                    }
                }
            }
            GUILayout.EndHorizontal();

            atlas.atlasSize = (UIAtlas.AtlasSize)EditorGUILayout.EnumPopup("Atlas Size:", atlas.atlasSize);
            EditorGUILayout.PropertyField(list, true);

            GUILayout.Space(10);
            if (atlas.objectsForPacking.Count > 0)
            {
                if (GUILayout.Button("    Packer    ", GUILayout.ExpandWidth(false)))
                {
                    List<Texture2D> textures = ScanForTextures(atlas.objectsForPacking);
                    Texture2D texture = new Texture2D((int)atlas.atlasSize, (int)atlas.atlasSize);
                    Rect[] rs = texture.PackTextures(textures.ToArray(), 1);
                    byte[] bs = texture.EncodeToPNG();


                    string selfPath = AssetDatabase.GetAssetPath(target);
                    string atlasName = selfPath.Replace(Path.GetExtension(selfPath), ".png"); //atlas.savePath + "/" + target.name + ".png";

                    File.WriteAllBytes(atlasName, bs);

                    List<SpriteMetaData> sheet = new List<SpriteMetaData>();
                    for (int i = 0; i < rs.Length; i++)
                    {
                        var rect = rs[i];
                        rect.x *= texture.width;
                        rect.y *= texture.height;
                        rect.width *= texture.width;
                        rect.height *= texture.height;

                        SpriteMetaData meta = new SpriteMetaData();
                        meta.rect = rect;
                        meta.name = textures[i].name;
                        meta.border = Vector4.zero;
                        meta.pivot = Vector2.zero;
                        sheet.Add(meta);
                    }
                    AssetDatabase.Refresh();
                    TextureImporter texImp = AssetImporter.GetAtPath(atlasName) as TextureImporter;
                    texImp.spritesheet = sheet.ToArray();
                    texImp.textureType = TextureImporterType.Sprite;
                    texImp.spriteImportMode = SpriteImportMode.Multiple;
                    AssetDatabase.ImportAsset(atlasName, ImportAssetOptions.ForceUpdate);

                    GameObject obj = new GameObject(target.name);
                    SpriteAtlas spriteAtlas = obj.AddComponent<SpriteAtlas>();
                    object[] sps = AssetDatabase.LoadAllAssetsAtPath(atlasName);
                    foreach (var sp in sps)
                    {
                        if (sp is Sprite)
                        {
                            Sprite sprite = (Sprite) sp;
                            spriteAtlas.AddToList(sprite);
                        }
                    }

                    string prefabPath = atlas.savePath + "/" + target.name + ".prefab";
                    PrefabUtility.CreatePrefab(prefabPath, obj);
                    AssetDatabase.Refresh();
                    DestroyImmediate(obj);

                    var genetare = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasName);
                    atlas.preview = genetare;
                    atlas.genreteSize = new Vector2(genetare.width, genetare.height);
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    atlas.spriteAtlas = prefab.GetComponent<SpriteAtlas>();
                }
            }

            GUILayout.Space(30);
            GUILayout.Label("Preview Parms");
            GUI.enabled = false;
            //if (atlas.spriteAtlas)
                EditorGUILayout.ObjectField("Sprite Atlas:", atlas.spriteAtlas, typeof(SpriteAtlas), false);
            //if(atlas.preview)
                EditorGUILayout.Vector2Field("Generated Size:", atlas.genreteSize);
                EditorGUILayout.ObjectField("Preview Atlas:", atlas.preview, typeof(Texture2D), false);
            GUI.enabled = true;

            GUILayout.EndVertical();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
            serializedObject.ApplyModifiedProperties();
        }


        private List<Texture2D> ScanForTextures(List<UnityEngine.Object> objs)
        {
            List<Texture2D> textures = new List<Texture2D>();
            foreach (var obj in objs)
            {
                if (obj is Texture2D)
                {
                    var tex = (Texture2D)obj;
                    if (!textures.Contains(tex))
                        textures.Add(tex);
                }
                else
                {
                    var path = AssetDatabase.GetAssetPath(obj);
                    if (!File.Exists(path))
                    {
                        string[] files = Directory.GetFiles(path, "*.png");
                        foreach (var file in files)
                        {
                            string p = file.Replace("\\", "/");
                            if (!p.Contains("Assets/"))
                                continue;

                            if (!p.StartsWith("Assets/"))
                            {
                                p = "Assets/" + (p.Split(new[] { "Assets/" }, StringSplitOptions.None))[1];
                            }

                            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                            if(!textures.Contains(tex))
                                textures.Add(tex);
                        }
                    }
                }
            }
            return textures;
        }



        [MenuItem("Assets/Create/SpriteAtlas", false, 80)]
        public static void CreatNewLua()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
            ScriptableObject.CreateInstance<CreateAtlasAssets>(),
            GetSelectedPathOrFallback() + "/Sprite Atlas.asset",
            null,
           null);
        }

        private static string GetSelectedPathOrFallback()
        {
            string path = "Assets";
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }
            return path;
        }
    }


    public class CreateAtlasAssets : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            UIAtlas obj = ScriptableObject.CreateInstance<UIAtlas>();
            AssetDatabase.CreateAsset(obj, pathName);
           AssetDatabase.ImportAsset(pathName);
        }
    }


    //[CustomEditor(typeof(SpriteAtlas))]
    //public class SpriteAtlasEditor : Editor
    //{
    //    private SerializedProperty dict;
    //    public override void OnInspectorGUI()
    //    {
    //        //base.OnInspectorGUI();
    //        if(dict == null)
    //            dict = serializedObject.FindProperty("spriteDic");
    //        EditorGUILayout.PropertyField(dict, true);
    //    }
    //}

}
