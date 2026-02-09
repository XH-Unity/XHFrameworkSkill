using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// GameplayTagsAsset 的自定义 Inspector
    /// </summary>
    [CustomEditor(typeof(GameplayTagsAsset))]
    public class GameplayTagsAssetEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var asset = target as GameplayTagsAsset;
            if (asset == null) return root;

            // 标题
            var titleLabel = new Label("Gameplay Tags Asset")
            {
                style =
                {
                    fontSize = 16,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 10
                }
            };
            root.Add(titleLabel);

            // 统计信息
            var statsContainer = new VisualElement
            {
                style =
                {
                    backgroundColor = new Color(0.25f, 0.25f, 0.25f),
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,
                    paddingLeft = 10,
                    paddingRight = 10,
                    paddingTop = 8,
                    paddingBottom = 8,
                    marginBottom = 10
                }
            };

            var tagCountLabel = new Label($"标签数量: {asset.CachedTags.Count}");
            statsContainer.Add(tagCountLabel);

            var nodeCountLabel = new Label($"节点数量: {asset.TreeNodes.Count - 1}"); // 减去根节点
            statsContainer.Add(nodeCountLabel);

            root.Add(statsContainer);

            // 打开编辑器按钮
            var openEditorButton = new Button(() =>
            {
                GameplayTagsEditorWindow.ShowWindow(asset);
            })
            {
                text = "打开标签编辑器",
                style =
                {
                    height = 30,
                    marginBottom = 10
                }
            };
            root.Add(openEditorButton);

            // 标签列表预览
            var foldout = new Foldout
            {
                text = "标签列表预览",
                value = false
            };

            var listContainer = new ScrollView(ScrollViewMode.Vertical)
            {
                style =
                {
                    maxHeight = 300,
                    backgroundColor = new Color(0.2f, 0.2f, 0.2f),
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,
                    paddingLeft = 8,
                    paddingRight = 8,
                    paddingTop = 4,
                    paddingBottom = 4
                }
            };

            foreach (var tag in asset.CachedTags)
            {
                var tagLabel = new Label(tag.Name)
                {
                    style =
                    {
                        paddingTop = 2,
                        paddingBottom = 2,
                        borderBottomWidth = 1,
                        borderBottomColor = new Color(0.25f, 0.25f, 0.25f)
                    }
                };
                listContainer.Add(tagLabel);
            }

            if (asset.CachedTags.Count == 0)
            {
                listContainer.Add(new Label("暂无标签")
                {
                    style = { color = new Color(0.5f, 0.5f, 0.5f) }
                });
            }

            foldout.Add(listContainer);
            root.Add(foldout);

            // 重建缓存按钮
            var rebuildButton = new Button(() =>
            {
                asset.RebuildCache();
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();

                // 刷新 Inspector
                var newRoot = CreateInspectorGUI();
                root.Clear();
                foreach (var child in newRoot.Children())
                {
                    root.Add(child);
                }
            })
            {
                text = "重建标签缓存",
                style = { marginTop = 10 }
            };
            root.Add(rebuildButton);

            return root;
        }
    }
}
