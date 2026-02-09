using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

using SkillEditor.Data;
namespace SkillEditor.Editor
{
    public interface INodeInspector
    {
        void BuildUI(VisualElement container, SkillNodeBase node);
    }

    public abstract class NodeInspectorBase : INodeInspector
    {
        protected SkillNodeBase currentNode;
        protected SkillGraphData currentGraphData;
        protected string currentFilePath;
        protected SkillGraphView graphView;

        /// <summary>
        /// 是否显示节点目标选择，子类可重写
        /// 默认不显示，只有 Effect 节点需要显示
        /// </summary>
        protected virtual bool ShowTargetType => false;

        public void SetContext(SkillGraphView view, SkillGraphData data, string path)
        {
            graphView = view;
            currentGraphData = data;
            currentFilePath = path;
        }

        public void BuildUI(VisualElement container, SkillNodeBase node)
        {
            currentNode = node;

            // 如果需要显示节点目标，先绘制
            if (ShowTargetType && node?.NodeData != null)
            {
                var targetTypeField = new EnumField("节点目标", node.NodeData.targetType) { style = { marginBottom = 8 } };
                ApplyEnumFieldStyle(targetTypeField);
                targetTypeField.RegisterValueChangedCallback(evt =>
                {
                    node.NodeData.targetType = (TargetType)evt.newValue;
                    node.SyncUIFromData();
                });
                container.Add(targetTypeField);
            }

            // 调用子类的具体绘制
            BuildInspectorUI(container, node);
        }

        /// <summary>
        /// 子类实现具体的Inspector UI绘制
        /// </summary>
        protected abstract void BuildInspectorUI(VisualElement container, SkillNodeBase node);

        protected void ApplyEnumFieldStyle(EnumField field)
        {
            field.style.borderTopLeftRadius = 4;
            field.style.borderTopRightRadius = 4;
            field.style.borderBottomLeftRadius = 4;
            field.style.borderBottomRightRadius = 4;

            // 让标签宽度自适应内容
            var label = field.Q<Label>();
            if (label != null)
            {
                label.style.minWidth = StyleKeyword.Auto;
                label.style.width = StyleKeyword.Auto;
                label.style.marginRight = 4;
            }
        }

        protected FloatField CreateFloatField(string label, float value, System.Action<float> onChanged)
        {
            var field = new FloatField(label) { value = value };
            field.style.borderTopLeftRadius = 4;
            field.style.borderTopRightRadius = 4;
            field.style.borderBottomLeftRadius = 4;
            field.style.borderBottomRightRadius = 4;

            // 让标签宽度自适应内容
            var labelElement = field.Q<Label>();
            if (labelElement != null)
            {
                labelElement.style.minWidth = StyleKeyword.Auto;
                labelElement.style.width = StyleKeyword.Auto;
                labelElement.style.marginRight = 4;
            }

            field.RegisterValueChangedCallback(evt =>
            {
                onChanged?.Invoke(evt.newValue);
            });
            return field;
        }
        protected IntegerField CreateIntField(string label, int value, System.Action<int> onChanged)
        {
            var field = new IntegerField(label) { value = value };
            field.style.borderTopLeftRadius = 4;
            field.style.borderTopRightRadius = 4;
            field.style.borderBottomLeftRadius = 4;
            field.style.borderBottomRightRadius = 4;

            // 让标签宽度自适应内容
            var labelElement = field.Q<Label>();
            if (labelElement != null)
            {
                labelElement.style.minWidth = StyleKeyword.Auto;
                labelElement.style.width = StyleKeyword.Auto;
                labelElement.style.marginRight = 4;
            }

            field.RegisterValueChangedCallback(evt =>
            {
                onChanged?.Invoke(evt.newValue);
            });
            return field;
        }

        /// <summary>
        /// 创建公式输入框（支持数值或公式）
        /// </summary>
        protected TextField CreateFormulaField(string label, string value, System.Action<string> onChanged)
        {
            var field = new TextField(label) { value = value ?? "0" };
            field.style.borderTopLeftRadius = 4;
            field.style.borderTopRightRadius = 4;
            field.style.borderBottomLeftRadius = 4;
            field.style.borderBottomRightRadius = 4;

            // 让标签宽度自适应内容
            var labelElement = field.Q<Label>();
            if (labelElement != null)
            {
                labelElement.style.minWidth = StyleKeyword.Auto;
                labelElement.style.width = StyleKeyword.Auto;
                labelElement.style.marginRight = 4;
            }

            // 让输入框填充剩余空间
            var textInput = field.Q("unity-text-input");
            if (textInput != null)
            {
                textInput.style.flexGrow = 1;
            }

            field.RegisterValueChangedCallback(evt =>
            {
                onChanged?.Invoke(evt.newValue);
            });
            return field;
        }

        protected TextField CreateTextField(string label, string value, System.Action<string> onChanged)
        {
            var field = new TextField(label) { value = value ?? "" };
            field.style.borderTopLeftRadius = 4;
            field.style.borderTopRightRadius = 4;
            field.style.borderBottomLeftRadius = 4;
            field.style.borderBottomRightRadius = 4;
            field.style.whiteSpace = WhiteSpace.Normal;
            field.style.unityTextAlign = TextAnchor.UpperLeft;
            field.style.paddingTop = 4;
            field.style.paddingLeft = 4;

            // 让标签宽度自适应内容
            var labelElement = field.Q<Label>();
            if (labelElement != null)
            {
                labelElement.style.minWidth = StyleKeyword.Auto;
                labelElement.style.width = StyleKeyword.Auto;
                labelElement.style.marginRight = 4;
            }

            // 获取内部的输入元素并设置其样式
            var textInput = field.Q<TextElement>();
            if (textInput != null)
            {
                textInput.style.unityTextAlign = TextAnchor.UpperLeft;
                textInput.style.whiteSpace = WhiteSpace.Normal;
                textInput.style.paddingTop = 4;
                textInput.style.paddingLeft = 4;
            }

            field.RegisterValueChangedCallback(evt =>
            {
                onChanged?.Invoke(evt.newValue);
            });
            return field;
        }

        protected VisualElement CreateTagListField(string label, List<string> tags, System.Action<List<string>> onChanged)
        {
            var container = new VisualElement
            {
                style =
                {
                    backgroundColor = new Color(70f / 255f, 70f / 255f, 70f / 255f),
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,
                    paddingLeft = 4,
                    paddingRight = 4,
                    paddingTop = 4,
                    paddingBottom = 4,
                    marginBottom = 4
                }
            };

            // 标题和折叠按钮
            var headerContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.SpaceBetween,
                    alignItems = Align.Center,
                    marginBottom = 4
                }
            };

            var titleLabel = new Label(label)
            {
                style =
                {
                    fontSize = 12,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    color = Color.white
                }
            };

            var foldoutButton = new Button { text = "▼" };
            foldoutButton.style.width = 20;
            foldoutButton.style.height = 20;
            foldoutButton.style.paddingLeft = 0;
            foldoutButton.style.paddingRight = 0;
            foldoutButton.style.paddingTop = 0;
            foldoutButton.style.paddingBottom = 0;

            headerContainer.Add(titleLabel);
            headerContainer.Add(foldoutButton);
            container.Add(headerContainer);

            // 内容容器
            var contentContainer = new VisualElement
            {
                style =
                {
                    display = DisplayStyle.Flex
                }
            };

            // 创建标签项
            void RefreshTagList()
            {
                contentContainer.Clear();

                for (int i = 0; i < tags.Count; i++)
                {
                    int index = i;
                    var tagContainer = new VisualElement
                    {
                        style =
                        {
                            flexDirection = FlexDirection.Row,
                            marginBottom = 4
                        }
                    };

                    var tagField = new TextField { value = tags[index] };
                    tagField.style.flexGrow = 1;
                    tagField.style.borderTopLeftRadius = 4;
                    tagField.style.borderTopRightRadius = 4;
                    tagField.style.borderBottomLeftRadius = 4;
                    tagField.style.borderBottomRightRadius = 4;
                    tagField.style.whiteSpace = WhiteSpace.Normal;
                    tagField.style.unityTextAlign = TextAnchor.UpperLeft;
                    tagField.style.paddingTop = 2;
                    tagField.style.paddingLeft = 4;

                    // 获取内部的输入元素并设置其样式
                    var tagTextInput = tagField.Q<TextElement>();
                    if (tagTextInput != null)
                    {
                        tagTextInput.style.unityTextAlign = TextAnchor.UpperLeft;
                        tagTextInput.style.whiteSpace = WhiteSpace.Normal;
                        tagTextInput.style.paddingTop = 2;
                        tagTextInput.style.paddingLeft = 4;
                    }

                    tagField.RegisterValueChangedCallback(evt =>
                    {
                        tags[index] = evt.newValue;
                        onChanged?.Invoke(tags);
                    });

                    var removeBtn = new Button { text = "×" };
                    removeBtn.style.width = 24;
                    removeBtn.style.height = 24;
                    removeBtn.style.marginLeft = 4;
                    removeBtn.style.paddingLeft = 0;
                    removeBtn.style.paddingRight = 0;
                    removeBtn.style.paddingTop = 0;
                    removeBtn.style.paddingBottom = 0;
                    removeBtn.clicked += () =>
                    {
                        tags.RemoveAt(index);
                        onChanged?.Invoke(tags);
                        RefreshTagList();
                    };

                    tagContainer.Add(tagField);
                    tagContainer.Add(removeBtn);
                    contentContainer.Add(tagContainer);
                }

                // 添加新标签按钮
                var addBtn = new Button { text = "+ 添加标签" };
                addBtn.style.marginTop = 4;
                addBtn.clicked += () =>
                {
                    tags.Add("");
                    onChanged?.Invoke(tags);
                    RefreshTagList();
                };
                contentContainer.Add(addBtn);
            }

            // 折叠/展开功能
            bool isExpanded = true;
            foldoutButton.clicked += () =>
            {
                isExpanded = !isExpanded;
                contentContainer.style.display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
                foldoutButton.text = isExpanded ? "▼" : "▶";
            };

            RefreshTagList();
            container.Add(contentContainer);

            return container;
        }

        /// <summary>
        /// 创建 GameplayTagSet 字段（使用标签选择器）
        /// </summary>
        protected VisualElement CreateTagSetField(string label, GameplayTagSet value, System.Action<GameplayTagSet> onChanged)
        {
            var field = new GameplayTagSetField(label);
            field.Value = value;
            field.OnValueChanged += onChanged;
            return field;
        }

        /// <summary>
        /// 创建枚举字段
        /// </summary>
        protected EnumField CreateEnumField<T>(string label, T value, System.Action<System.Enum> onChanged) where T : System.Enum
        {
            var field = new EnumField(label, value);
            ApplyEnumFieldStyle(field);
            field.RegisterValueChangedCallback(evt =>
            {
                onChanged?.Invoke(evt.newValue);
            });
            return field;
        }

        /// <summary>
        /// 创建单个 GameplayTag 字段
        /// </summary>
        protected VisualElement CreateTagField(string label, GameplayTag value, System.Action<GameplayTag> onChanged)
        {
            var container = new VisualElement();
            container.style.marginBottom = 4;

            var labelElement = new Label(label);
            labelElement.style.marginBottom = 2;
            container.Add(labelElement);

            var textField = new UnityEngine.UIElements.TextField();
            textField.value = value.Name ?? "";
            textField.RegisterValueChangedCallback(evt =>
            {
                var newTag = string.IsNullOrEmpty(evt.newValue)
                    ? GameplayTag.None
                    : new GameplayTag(evt.newValue);
                onChanged?.Invoke(newTag);
            });
            container.Add(textField);

            return container;
        }

        protected VisualElement CreateCollapsibleSection(string title, out VisualElement contentContainer)
        {
            return CreateCollapsibleSection(title, out contentContainer, true);
        }

        protected VisualElement CreateCollapsibleSection(string title, out VisualElement contentContainer, bool defaultExpanded)
        {
            var container = new VisualElement
            {
                style =
                {
                    backgroundColor = new Color(56f / 255f, 56f / 255f, 56f / 255f),
                    borderTopLeftRadius = 8,
                    borderTopRightRadius = 8,
                    borderBottomLeftRadius = 8,
                    borderBottomRightRadius = 8,
                    paddingLeft = 8,
                    paddingRight = 8,
                    paddingTop = 8,
                    paddingBottom = 8,
                    marginTop = 8
                }
            };

            var headerContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.SpaceBetween,
                    alignItems = Align.Center,
                    marginBottom = 8
                }
            };

            var titleLabel = new Label(title)
            {
                style =
                {
                    fontSize = 12,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    color = Color.white
                }
            };

            var foldButton = new Button { text = defaultExpanded ? "▼" : "▶" };
            foldButton.style.width = 20;
            foldButton.style.height = 20;
            foldButton.style.paddingLeft = 0;
            foldButton.style.paddingRight = 0;
            foldButton.style.paddingTop = 0;
            foldButton.style.paddingBottom = 0;

            headerContainer.Add(titleLabel);
            headerContainer.Add(foldButton);
            container.Add(headerContainer);

            var content = new VisualElement();
            content.style.display = defaultExpanded ? DisplayStyle.Flex : DisplayStyle.None;
            contentContainer = content;
            container.Add(content);

            bool isExpanded = defaultExpanded;
            foldButton.clicked += () =>
            {
                isExpanded = !isExpanded;
                content.style.display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
                foldButton.text = isExpanded ? "▼" : "▶";
            };

            return container;
        }
    }
    
 
}
