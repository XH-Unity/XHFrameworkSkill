using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 标签选择器控件 - 用于在节点中选择单个标签
    /// </summary>
    public class GameplayTagField : VisualElement
    {
        private static GameplayTagsAsset _cachedAsset;

        private string _label;
        private GameplayTag _value;
        private Button _selectButton;
        private Label _valueLabel;

        public event Action<GameplayTag> OnValueChanged;

        public GameplayTag Value
        {
            get => _value;
            set
            {
                _value = value;
                UpdateDisplay();
            }
        }

        public GameplayTagField(string label = "标签")
        {
            _label = label;
            CreateUI();
        }

        private void CreateUI()
        {
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            style.marginTop = 2;
            style.marginBottom = 2;

            // 标签
            var labelElement = new Label(_label)
            {
                style =
                {
                    width = 80,
                    minWidth = 80,
                    unityTextAlign = TextAnchor.MiddleLeft
                }
            };
            Add(labelElement);

            // 值显示
            _valueLabel = new Label("None")
            {
                style =
                {
                    flexGrow = 1,
                    backgroundColor = new Color(0.2f, 0.2f, 0.2f),
                    borderTopLeftRadius = 3,
                    borderBottomLeftRadius = 3,
                    paddingLeft = 6,
                    paddingRight = 6,
                    paddingTop = 2,
                    paddingBottom = 2,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    overflow = Overflow.Hidden
                }
            };
            Add(_valueLabel);

            // 选择按钮
            _selectButton = new Button(() => ShowTagSelector())
            {
                text = "...",
                style =
                {
                    width = 24,
                    borderTopLeftRadius = 0,
                    borderBottomLeftRadius = 0,
                    marginLeft = 0
                }
            };
            Add(_selectButton);

            // 清除按钮
            var clearButton = new Button(() =>
            {
                _value = GameplayTag.None;
                UpdateDisplay();
                OnValueChanged?.Invoke(_value);
            })
            {
                text = "×",
                style =
                {
                    width = 20,
                    marginLeft = 2,
                    color = new Color(0.8f, 0.3f, 0.3f)
                }
            };
            Add(clearButton);
        }

        private void UpdateDisplay()
        {
            _valueLabel.text = _value.IsValid ? _value.Name : "None";
            _valueLabel.tooltip = _value.IsValid ? _value.Name : "";
        }

        private void ShowTagSelector()
        {
            var asset = GetTagsAsset();
            if (asset == null)
            {
                EditorUtility.DisplayDialog("错误", "请先创建 GameplayTagsAsset 资源", "确定");
                return;
            }

            GameplayTagSelectorPopup.Show(_selectButton.worldBound, asset, _value, tag =>
            {
                _value = tag;
                UpdateDisplay();
                OnValueChanged?.Invoke(_value);
            });
        }

        private static GameplayTagsAsset GetTagsAsset()
        {
            if (_cachedAsset != null) return _cachedAsset;

            // 查找项目中的 GameplayTagsAsset
            var guids = AssetDatabase.FindAssets("t:GameplayTagsAsset");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _cachedAsset = AssetDatabase.LoadAssetAtPath<GameplayTagsAsset>(path);
            }

            return _cachedAsset;
        }

        public static void ClearAssetCache()
        {
            _cachedAsset = null;
        }
    }

    /// <summary>
    /// 标签集合选择器控件 - 用于在节点中选择多个标签
    /// </summary>
    public class GameplayTagSetField : VisualElement
    {
        private static GameplayTagsAsset _cachedAsset;

        private string _label;
        private List<GameplayTag> _tags = new List<GameplayTag>();
        private VisualElement _tagsContainer;
        private bool _isExpanded = true;

        public event Action<GameplayTagSet> OnValueChanged;

        public GameplayTagSet Value
        {
            get => new GameplayTagSet(_tags);
            set
            {
                _tags.Clear();
                if (!value.IsEmpty)
                {
                    _tags.AddRange(value.Tags);
                }
                RefreshTagsDisplay();
            }
        }

        public GameplayTagSetField(string label = "标签集合")
        {
            _label = label;
            CreateUI();
        }

        private void CreateUI()
        {
            style.marginTop = 4;
            style.marginBottom = 4;

            // 标题行
            var headerRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginBottom = 4
                }
            };

            // 折叠按钮
            Button foldButton = null;
            foldButton = new Button(() =>
            {
                _isExpanded = !_isExpanded;
                _tagsContainer.style.display = _isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
                foldButton.text = _isExpanded ? "▼" : "▶";
            })
            {
                text = "▼",
                style =
                {
                    width = 18,
                    height = 18,
                    paddingLeft = 0,
                    paddingRight = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
                    marginRight = 4,
                    fontSize = 10
                }
            };
            headerRow.Add(foldButton);

            // 标签
            var labelElement = new Label(_label)
            {
                style = { flexGrow = 1 }
            };
            headerRow.Add(labelElement);

            // 数量显示
            var countLabel = new Label($"({_tags.Count})")
            {
                name = "CountLabel",
                style = { marginRight = 8, color = new Color(0.6f, 0.6f, 0.6f) }
            };
            headerRow.Add(countLabel);

            // 添加按钮
            var addButton = new Button(() => ShowAddTagSelector())
            {
                text = "+",
                style =
                {
                    width = 20,
                    height = 18,
                    paddingLeft = 0,
                    paddingRight = 0,
                    paddingTop = 0,
                    paddingBottom = 0
                }
            };
            headerRow.Add(addButton);

            Add(headerRow);

            // 标签列表容器
            _tagsContainer = new VisualElement
            {
                style =
                {
                    backgroundColor = new Color(0.2f, 0.2f, 0.2f),
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,
                    paddingLeft = 4,
                    paddingRight = 4,
                    paddingTop = 4,
                    paddingBottom = 4,
                    minHeight = 24
                }
            };
            Add(_tagsContainer);

            RefreshTagsDisplay();
        }

        private void RefreshTagsDisplay()
        {
            _tagsContainer.Clear();

            // 更新数量显示
            var countLabel = this.Q<Label>("CountLabel");
            if (countLabel != null)
                countLabel.text = $"({_tags.Count})";

            if (_tags.Count == 0)
            {
                _tagsContainer.Add(new Label("无标签")
                {
                    style = { color = new Color(0.5f, 0.5f, 0.5f), fontSize = 11 }
                });
                return;
            }

            foreach (var tag in _tags)
            {
                var tagItem = CreateTagItem(tag);
                _tagsContainer.Add(tagItem);
            }
        }

        private VisualElement CreateTagItem(GameplayTag tag)
        {
            var container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    backgroundColor = new Color(0.3f, 0.3f, 0.3f),
                    borderTopLeftRadius = 3,
                    borderTopRightRadius = 3,
                    borderBottomLeftRadius = 3,
                    borderBottomRightRadius = 3,
                    paddingLeft = 6,
                    paddingRight = 2,
                    paddingTop = 2,
                    paddingBottom = 2,
                    marginBottom = 2
                }
            };

            var nameLabel = new Label(tag.Name)
            {
                style =
                {
                    flexGrow = 1,
                    fontSize = 11
                }
            };
            container.Add(nameLabel);

            var removeButton = new Button(() =>
            {
                _tags.Remove(tag);
                RefreshTagsDisplay();
                OnValueChanged?.Invoke(Value);
            })
            {
                text = "×",
                style =
                {
                    width = 16,
                    height = 16,
                    paddingLeft = 0,
                    paddingRight = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
                    fontSize = 10,
                    color = new Color(0.8f, 0.3f, 0.3f),
                    backgroundColor = Color.clear,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    borderTopWidth = 0,
                    borderBottomWidth = 0
                }
            };
            container.Add(removeButton);

            return container;
        }

        private void ShowAddTagSelector()
        {
            var asset = GetTagsAsset();
            if (asset == null)
            {
                EditorUtility.DisplayDialog("错误", "请先创建 GameplayTagsAsset 资源", "确定");
                return;
            }

            var addButton = this.Q<Button>();
            GameplayTagSelectorPopup.Show(addButton.worldBound, asset, GameplayTag.None, tag =>
            {
                if (tag.IsValid && !_tags.Exists(t => t == tag))
                {
                    _tags.Add(tag);
                    RefreshTagsDisplay();
                    OnValueChanged?.Invoke(Value);
                }
            });
        }

        private static GameplayTagsAsset GetTagsAsset()
        {
            if (_cachedAsset != null) return _cachedAsset;

            var guids = AssetDatabase.FindAssets("t:GameplayTagsAsset");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _cachedAsset = AssetDatabase.LoadAssetAtPath<GameplayTagsAsset>(path);
            }

            return _cachedAsset;
        }
    }

    /// <summary>
    /// 标签选择弹出窗口
    /// </summary>
    public class GameplayTagSelectorPopup : EditorWindow
    {
        private GameplayTagsAsset _asset;
        private GameplayTag _currentValue;
        private Action<GameplayTag> _onSelect;
        private string _searchText = "";
        private Vector2 _scrollPosition;

        private HashSet<int> _expandedNodes = new HashSet<int>();
        private VisualElement _treeContainer;
        private TextField _searchField;

        public static void Show(Rect buttonRect, GameplayTagsAsset asset, GameplayTag currentValue, Action<GameplayTag> onSelect)
        {
            var window = CreateInstance<GameplayTagSelectorPopup>();
            window._asset = asset;
            window._currentValue = currentValue;
            window._onSelect = onSelect;
            window._expandedNodes.Add(0); // 默认展开根节点

            // 计算窗口位置
            var windowSize = new Vector2(300, 400);
            var screenPos = GUIUtility.GUIToScreenPoint(new Vector2(buttonRect.x, buttonRect.yMax));

            window.ShowAsDropDown(new Rect(screenPos, Vector2.zero), windowSize);
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.paddingLeft = 4;
            root.style.paddingRight = 4;
            root.style.paddingTop = 4;
            root.style.paddingBottom = 4;

            // 搜索框
            _searchField = new TextField
            {
                style = { marginBottom = 4 }
            };
            _searchField.RegisterValueChangedCallback(evt =>
            {
                _searchText = evt.newValue;
                RefreshTree();
            });
            root.Add(_searchField);

            // "None" 选项
            var noneButton = new Button(() =>
            {
                _onSelect?.Invoke(GameplayTag.None);
                Close();
            })
            {
                text = "None",
                style =
                {
                    marginBottom = 4,
                    unityTextAlign = TextAnchor.MiddleLeft
                }
            };
            root.Add(noneButton);

            // 树形容器
            var scrollView = new ScrollView(ScrollViewMode.Vertical)
            {
                style =
                {
                    flexGrow = 1,
                    backgroundColor = new Color(0.2f, 0.2f, 0.2f),
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4
                }
            };
            _treeContainer = new VisualElement();
            scrollView.Add(_treeContainer);
            root.Add(scrollView);

            RefreshTree();

            // 聚焦搜索框
            _searchField.schedule.Execute(() => _searchField.Focus());
        }

        private void RefreshTree()
        {
            _treeContainer.Clear();

            if (_asset == null) return;

            _asset.Initialize();

            if (string.IsNullOrEmpty(_searchText))
            {
                // 树形显示
                var root = _asset.GetRootNode();
                BuildTreeUI(root, 0);
            }
            else
            {
                // 搜索结果
                var results = _asset.SearchTags(_searchText);
                foreach (var tag in results)
                {
                    var item = CreateSearchResultItem(tag);
                    _treeContainer.Add(item);
                }
            }
        }

        private void BuildTreeUI(GameplayTagTreeNode node, int depth)
        {
            if (node.id != 0)
            {
                var item = CreateTreeItem(node, depth);
                _treeContainer.Add(item);
            }

            if (node.id == 0 || _expandedNodes.Contains(node.id))
            {
                foreach (var childId in node.childrenIds)
                {
                    var child = _asset.GetNodeById(childId);
                    if (child != null)
                    {
                        BuildTreeUI(child, node.id == 0 ? 0 : depth + 1);
                    }
                }
            }
        }

        private VisualElement CreateTreeItem(GameplayTagTreeNode node, int depth)
        {
            var fullPath = _asset.GetFullTagPath(node.id);
            var isSelected = _currentValue.IsValid && _currentValue.Name == fullPath;

            var container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    paddingLeft = depth * 16 + 4,
                    paddingTop = 2,
                    paddingBottom = 2,
                    backgroundColor = isSelected ? new Color(0.3f, 0.5f, 0.8f, 0.5f) : Color.clear
                }
            };

            // 展开按钮
            var hasChildren = node.childrenIds.Count > 0;
            var foldButton = new Button(() =>
            {
                if (_expandedNodes.Contains(node.id))
                    _expandedNodes.Remove(node.id);
                else
                    _expandedNodes.Add(node.id);
                RefreshTree();
            })
            {
                text = hasChildren ? (_expandedNodes.Contains(node.id) ? "▼" : "▶") : " ",
                style =
                {
                    width = 16,
                    height = 16,
                    paddingLeft = 0,
                    paddingRight = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
                    marginRight = 2,
                    fontSize = 9,
                    backgroundColor = Color.clear,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    borderTopWidth = 0,
                    borderBottomWidth = 0
                }
            };
            if (!hasChildren) foldButton.SetEnabled(false);
            container.Add(foldButton);

            // 名称按钮（点击选择）
            var nameButton = new Button(() =>
            {
                _onSelect?.Invoke(new GameplayTag(fullPath));
                Close();
            })
            {
                text = node.name,
                style =
                {
                    flexGrow = 1,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    backgroundColor = Color.clear,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    borderTopWidth = 0,
                    borderBottomWidth = 0
                }
            };
            nameButton.tooltip = fullPath;
            container.Add(nameButton);

            return container;
        }

        private VisualElement CreateSearchResultItem(GameplayTag tag)
        {
            var isSelected = _currentValue.IsValid && _currentValue == tag;

            var button = new Button(() =>
            {
                _onSelect?.Invoke(tag);
                Close();
            })
            {
                text = tag.Name,
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft,
                    backgroundColor = isSelected ? new Color(0.3f, 0.5f, 0.8f, 0.5f) : Color.clear,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    borderTopWidth = 0,
                    borderBottomWidth = 0,
                    marginBottom = 1
                }
            };

            return button;
        }
    }
}
