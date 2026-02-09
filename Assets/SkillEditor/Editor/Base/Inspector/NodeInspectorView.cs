using UnityEngine;
using UnityEngine.UIElements;
using System.Reflection;

using SkillEditor.Data;
namespace SkillEditor.Editor
{
    public class NodeInspectorView : VisualElement
    {
        private SkillNodeBase currentNode;
        private NodeInspectorBase currentInspector;
        private VisualElement contentContainer;
        private ScrollView scrollView;
        private SkillGraphView graphView;
        private SkillGraphData graphData;
        private string filePath;

        public NodeInspectorView()
        {
            style.width = 300;
            style.backgroundColor = new Color(56f/255f, 56f/255f, 56f/255f);
            style.paddingTop = 10;
            style.paddingBottom = 10;
            style.paddingLeft = 10;
            style.paddingRight = 10;
            style.flexDirection = FlexDirection.Column;

            var title = new Label("节点属性")
            {
                style =
                {
                    fontSize = 16,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 10
                }
            };
            Add(title);

            // 创建滚动视图
            scrollView = new ScrollView(ScrollViewMode.Vertical)
            {
                style =
                {
                    flexGrow = 1
                }
            };

            contentContainer = new VisualElement
            {
                style =
                {
                    flexGrow = 1
                }
            };
            scrollView.Add(contentContainer);
            Add(scrollView);
        }

        public void SetGraphContext(SkillGraphView view, SkillGraphData data, string path)
        {
            graphView = view;
            graphData = data;
            filePath = path;

            if (currentInspector != null)
            {
                currentInspector.SetContext(view, data, path);
            }
        }

        public void UpdateSelection(SkillNodeBase node)
        {
            // 取消订阅旧节点的事件
            if (currentNode != null)
            {
                currentNode.OnDataChanged -= OnNodeDataChanged;
            }

            currentNode = node;
            contentContainer.Clear();

            if (node == null)
            {
                var noSelectionLabel = new Label("未选择节点")
                {
                    style =
                    {
                        fontSize = 14,
                        unityTextAlign = TextAnchor.MiddleCenter,
                        marginTop = 20
                    }
                };
                contentContainer.Add(noSelectionLabel);
                return;
            }

            // 订阅新节点的数据变化事件
            node.OnDataChanged += OnNodeDataChanged;

            var typeLabel = new Label($"类型: {GetInspectorName(node.NodeType)}")
            {
                style = { marginBottom = 10 }
            };
            contentContainer.Add(typeLabel);

            var separator = new VisualElement
            {
                style =
                {
                    height = 1,
                    backgroundColor = new Color(0.5f, 0.5f, 0.5f),
                    marginTop = 5,
                    marginBottom = 10
                }
            };
            contentContainer.Add(separator);

            currentInspector = NodeInspectorFactory.CreateInspector(node);
            if (currentInspector != null)
            {
                // 确保检查器有正确的context
                currentInspector.SetContext(graphView, graphData, filePath);
                currentInspector.BuildUI(contentContainer, node);
            }
        }

        /// <summary>
        /// 当节点数据变化时，刷新属性面板
        /// </summary>
        private void OnNodeDataChanged()
        {
            if (currentNode != null)
            {
                // 重新构建UI
                UpdateSelection(currentNode);
            }
        }

        /// <summary>
        /// 获取枚举的InspectorName特性值
        /// </summary>
        private string GetInspectorName<T>(T enumValue) where T : System.Enum
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            var attribute = fieldInfo?.GetCustomAttribute<InspectorNameAttribute>();
            return attribute?.displayName ?? enumValue.ToString();
        }
    }
}
