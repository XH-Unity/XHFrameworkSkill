using UnityEngine;
using UnityEditor;
using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 标签管理器 - 在 Project Settings 中提供标签管理界面
    /// </summary>
    public static class GameplayTagsSettingsProvider
    {
        private static GameplayTagsAsset _asset;
        private static UnityEditor.Editor _editor;
        private static Vector2 _scrollPosition;

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider = new SettingsProvider("Project/SkillEditor/Gameplay Tags", SettingsScope.Project)
            {
                label = "Gameplay Tags",
                guiHandler = searchContext => DrawSettingsGUI(),
                keywords = new[] { "Gameplay", "Tags", "Skill", "Editor", "标签" }
            };

            return provider;
        }

        private static void DrawSettingsGUI()
        {
            EditorGUILayout.Space(10);

            // 标题
            EditorGUILayout.LabelField("Gameplay Tags 管理", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // 查找或创建资源
            if (_asset == null)
            {
                LoadAsset();
            }

            // 资源选择
            EditorGUI.BeginChangeCheck();
            _asset = (GameplayTagsAsset)EditorGUILayout.ObjectField(
                "标签资源",
                _asset,
                typeof(GameplayTagsAsset),
                false);

            if (EditorGUI.EndChangeCheck())
            {
                _editor = null;
            }

            EditorGUILayout.Space(5);

            // 创建新资源按钮
            if (_asset == null)
            {
                EditorGUILayout.HelpBox("未找到 GameplayTagsAsset 资源。请创建一个新的资源或选择现有资源。", MessageType.Info);

                if (GUILayout.Button("创建新的标签资源", GUILayout.Height(30)))
                {
                    CreateNewAsset();
                }
            }
            else
            {
                // 显示资源信息
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"标签数量: {_asset.CachedTags.Count}");
                EditorGUILayout.LabelField($"节点数量: {_asset.TreeNodes.Count - 1}");
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(10);

                // 操作按钮
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("打开标签编辑器", GUILayout.Height(30)))
                {
                    GameplayTagsEditorWindow.ShowWindow(_asset);
                }

                if (GUILayout.Button("生成代码", GUILayout.Height(30)))
                {
                    GameplayTagCodeGenerator.GenerateTagCode();
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(10);

                // 标签列表预览
                EditorGUILayout.LabelField("标签列表预览", EditorStyles.boldLabel);

                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.MaxHeight(400));
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                if (_asset.CachedTags.Count == 0)
                {
                    EditorGUILayout.LabelField("暂无标签", EditorStyles.centeredGreyMiniLabel);
                }
                else
                {
                    foreach (var tag in _asset.CachedTags)
                    {
                        EditorGUILayout.BeginHorizontal();

                        // 缩进显示层级
                        var depth = tag.Depth;
                        GUILayout.Space(depth * 15);

                        // 显示短名称和完整路径
                        EditorGUILayout.LabelField(tag.ShortName, GUILayout.Width(150));
                        EditorGUILayout.LabelField(tag.Name, EditorStyles.miniLabel);

                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space(10);

                // 重建缓存按钮
                if (GUILayout.Button("重建标签缓存"))
                {
                    _asset.RebuildCache();
                    EditorUtility.SetDirty(_asset);
                    AssetDatabase.SaveAssets();
                }
            }
        }

        private static void LoadAsset()
        {
            var guids = AssetDatabase.FindAssets("t:GameplayTagsAsset");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _asset = AssetDatabase.LoadAssetAtPath<GameplayTagsAsset>(path);
            }
        }

        private static void CreateNewAsset()
        {
            // 默认保存到 SkillEditor/Data/Tags 目录
            var defaultPath = "Assets/SkillEditor/Data/Tags";
            if (!AssetDatabase.IsValidFolder(defaultPath))
            {
                // 确保目录存在
                if (!AssetDatabase.IsValidFolder("Assets/SkillEditor"))
                    AssetDatabase.CreateFolder("Assets", "SkillEditor");
                if (!AssetDatabase.IsValidFolder("Assets/SkillEditor/Data"))
                    AssetDatabase.CreateFolder("Assets/SkillEditor", "Data");
                if (!AssetDatabase.IsValidFolder("Assets/SkillEditor/Data/Tags"))
                    AssetDatabase.CreateFolder("Assets/SkillEditor/Data", "Tags");
            }

            var path = EditorUtility.SaveFilePanelInProject(
                "创建标签资源",
                "GameplayTagsAsset",
                "asset",
                "选择保存位置",
                defaultPath);

            if (string.IsNullOrEmpty(path)) return;

            var asset = ScriptableObject.CreateInstance<GameplayTagsAsset>();
            asset.Initialize();

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            _asset = asset;
            _editor = null;

            Debug.Log($"标签资源已创建: {path}");
        }
    }
}
