using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.IMGUI.Controls;
using System.Linq;
using System.IO;

using SkillEditor.Data;
namespace SkillEditor.Editor
{
    public class SkillEditorWindow : EditorWindow
    {
        private SkillGraphView graphView;
        private NodeInspectorView inspectorView;
        private SkillAssetTreeView treeView;
        private IMGUIContainer treeViewContainer;
        private SkillGraphData currentGraphData;
        private string currentFilePath;
        private Button saveButton;
        private Button overviewButton;
        private VisualElement centerPanel;
        private bool _isGraphDirty = false;

        [MenuItem("Tools/Skill Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<SkillEditorWindow>();
            window.titleContent = new GUIContent("技能编辑器");
            window.minSize = new Vector2(1000, 600);
        }

        private void OnEnable()
        {
            ConstructUI();
            rootVisualElement.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);

            // 打开编辑器时自动执行全览
            ScheduleFrameAllNodes();
        }

        private void OnDisable()
        {
            // 关闭窗口前自动保存
            if (_isGraphDirty)
                SaveCurrentGraph();

            rootVisualElement.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            rootVisualElement.Clear();
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.ctrlKey && evt.keyCode == KeyCode.S)
            {
                SaveCurrentGraph();
                evt.StopPropagation();
            }
        }

        private void ConstructUI()
        {
            rootVisualElement.Clear();

            var toolbar = CreateToolbar();
            rootVisualElement.Add(toolbar);

            var mainContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 1
                }
            };

            var leftPanel = CreateLeftPanel();
            var centerPanel = CreateCenterPanel();
            var rightPanel = CreateRightPanel();

            mainContainer.Add(leftPanel);
            mainContainer.Add(centerPanel);
            mainContainer.Add(rightPanel);

            rootVisualElement.Add(mainContainer);
        }

        private VisualElement CreateToolbar()
        {
            var toolbar = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    height = 30,
                    backgroundColor = new Color(0.2f, 0.2f, 0.2f),
                    paddingLeft = 5,
                    paddingRight = 5,
                    paddingTop = 3,
                    paddingBottom = 3,
                    borderBottomWidth = 1,
                    borderBottomColor = new Color(0.1f, 0.1f, 0.1f),
                    alignItems = Align.Center
                }
            };

            saveButton = new Button(SaveCurrentGraph)
            {
                text = "保存 (Ctrl+S)",
                style =
                {
                    width = 120,
                    height = 24,
                    borderTopLeftRadius = 5,
                    borderTopRightRadius = 5,
                    borderBottomLeftRadius = 5,
                    borderBottomRightRadius = 5
                }
            };
            saveButton.SetEnabled(false);

            toolbar.Add(saveButton);

            // 添加全览按钮
            overviewButton = new Button(FrameAllNodes)
            {
                text = "全览",
                style =
                {
                    width = 80,
                    height = 24,
                    marginLeft = 10,
                    borderTopLeftRadius = 5,
                    borderTopRightRadius = 5,
                    borderBottomLeftRadius = 5,
                    borderBottomRightRadius = 5
                }
            };
            overviewButton.SetEnabled(false);

            // 保存按钮启用时，全览按钮也启用
            saveButton.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                overviewButton.SetEnabled(evt.newValue);
            });

            toolbar.Add(overviewButton);
            return toolbar;
        }

        /// <summary>
        /// 延迟执行全览，通过 GeometryChangedEvent 确保 layout 真正完成
        /// </summary>
        private void ScheduleFrameAllNodes()
        {
            // 注册一次性的 GeometryChangedEvent，layout 完成后自动触发
            void OnGeometryChanged(GeometryChangedEvent evt)
            {
                graphView.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                // 再延迟一帧，确保子节点的 layout 也完成
                EditorApplication.delayCall += FrameAllNodes;
            }

            if (graphView.layout.size.x > 0 && graphView.layout.size.y > 0)
            {
                // GraphView 自身 layout 已就绪，但节点可能还没有
                // 延迟两帧确保节点 layout 完成
                EditorApplication.delayCall += () => EditorApplication.delayCall += FrameAllNodes;
            }
            else
            {
                graphView.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            }
        }

        private void FrameAllNodes()
        {
            if (graphView == null || graphView.nodes.Count() == 0)
                return;

            graphView.FrameAll();
        }

        private void SaveCurrentGraph()
        {
            if (currentGraphData == null || string.IsNullOrEmpty(currentFilePath) || graphView == null)
                return;

            graphView.SaveGraph(currentGraphData);
            EditorUtility.SetDirty(currentGraphData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _isGraphDirty = false;
            UpdateSaveButtonStyle();
            Debug.Log($"已保存: {currentFilePath}");
        }

        private void MarkGraphDirty()
        {
            if (_isGraphDirty) return;
            _isGraphDirty = true;
            UpdateSaveButtonStyle();
        }

        private void UpdateSaveButtonStyle()
        {
            if (saveButton == null) return;

            if (_isGraphDirty)
            {
                saveButton.text = "● 保存 (Ctrl+S)";
                saveButton.style.backgroundColor = new Color(0.8f, 0.6f, 0.1f, 0.8f);
                saveButton.style.color = Color.white;
            }
            else
            {
                saveButton.text = "保存 (Ctrl+S)";
                saveButton.style.backgroundColor = StyleKeyword.Null;
                saveButton.style.color = StyleKeyword.Null;
            }
        }

        private VisualElement CreateLeftPanel()
        {
            var panel = new VisualElement
            {
                style =
                {
                    width = 250,
                    backgroundColor = new Color(56f/255f, 56f/255f, 56f/255f),
                    borderRightWidth = 1,
                    borderRightColor = new Color(0.1f, 0.1f, 0.1f),
                    borderTopLeftRadius = 8,
                    borderTopRightRadius = 8,
                    borderBottomLeftRadius = 8,
                    borderBottomRightRadius = 8,
                    marginLeft = 5,
                    marginTop = 5,
                    marginBottom = 5,
                    flexDirection = FlexDirection.Column
                }
            };

            var header = new Label("技能资源")
            {
                style =
                {
                    fontSize = 14,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    paddingTop = 5,
                    paddingBottom = 5,
                    paddingLeft = 10,
                    backgroundColor = new Color(0.25f, 0.25f, 0.25f),
                    borderTopLeftRadius = 8,
                    borderTopRightRadius = 8
                }
            };
            panel.Add(header);

            treeView = new SkillAssetTreeView(new TreeViewState());
            treeView.OnFileSelected += LoadGraphFromPath;

            // 创建滚动容器
            var scrollContainer = new ScrollView(ScrollViewMode.Vertical)
            {
                style =
                {
                    flexGrow = 1,
                    backgroundColor = new Color(56f/255f, 56f/255f, 56f/255f)
                }
            };

            treeViewContainer = new IMGUIContainer(() =>
            {
                if (treeView != null)
                {
                    var rect = treeViewContainer.contentRect;

                    // Set background color for TreeView
                    var bgColor = new Color(56f/255f, 56f/255f, 56f/255f);
                    EditorGUI.DrawRect(new Rect(0, 0, rect.width, rect.height), bgColor);

                    treeView.OnGUI(new Rect(0, 0, rect.width, rect.height));
                }
            })
            {
                style = {
                    minHeight = 400,
                    backgroundColor = new Color(56f/255f, 56f/255f, 56f/255f)
                }
            };

            scrollContainer.Add(treeViewContainer);
            panel.Add(scrollContainer);
            return panel;
        }

        private VisualElement CreateCenterPanel()
        {
            centerPanel = new VisualElement
            {
                style = {
                    flexGrow = 1,
                    marginTop = 5,
                    marginBottom = 5,
                    marginLeft = 5,
                    marginRight = 5
                }
            };

            graphView = new SkillGraphView
            {
                name = "Skill Graph",
                style = {
                    flexGrow = 1,
                    borderTopLeftRadius = 8,
                    borderTopRightRadius = 8,
                    borderBottomLeftRadius = 8,
                    borderBottomRightRadius = 8
                }
            };

            graphView.OnNodeSelected += OnNodeSelected;
            graphView.OnGraphModified += MarkGraphDirty;
            graphView.SetNodeSelectionCallback();

            centerPanel.Add(graphView);
            return centerPanel;
        }

        private VisualElement CreateRightPanel()
        {
            inspectorView = new NodeInspectorView();
            inspectorView.OnNodeDataModified += MarkGraphDirty;
            inspectorView.style.borderTopLeftRadius = 8;
            inspectorView.style.borderTopRightRadius = 8;
            inspectorView.style.borderBottomLeftRadius = 8;
            inspectorView.style.borderBottomRightRadius = 8;
            inspectorView.style.marginRight = 5;
            inspectorView.style.marginTop = 5;
            inspectorView.style.marginBottom = 5;
            return inspectorView;
        }

        private void OnNodeSelected(SkillNodeBase node)
        {
            inspectorView.UpdateSelection(node);
        }

        private void LoadGraphFromPath(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path) || !path.EndsWith(".asset"))
            {
                // 切换到文件夹或无效路径前，自动保存
                if (_isGraphDirty)
                    SaveCurrentGraph();

                graphView.ClearGraph();
                currentFilePath = null;
                currentGraphData = null;
                inspectorView.SetGraphContext(null, null, null);
                UpdateUIState(false);
                return;
            }

            // 点击同一个文件时不重复加载
            if (path == currentFilePath && currentGraphData != null)
                return;

            // 切换到其他技能文件前，自动保存当前文件
            if (_isGraphDirty)
                SaveCurrentGraph();

            currentFilePath = path;
            currentGraphData = AssetDatabase.LoadAssetAtPath<SkillGraphData>(path);

            if (currentGraphData != null)
            {
                graphView.ClearGraph();
                graphView.LoadGraph(currentGraphData);
                inspectorView.SetGraphContext(graphView, currentGraphData, currentFilePath);
                UpdateUIState(true);

                _isGraphDirty = false;
                UpdateSaveButtonStyle();

                // 延迟执行全览，等待 layout 计算完成
                ScheduleFrameAllNodes();
            }
            else
            {
                UpdateUIState(false);
            }
        }

        private void UpdateUIState(bool isFileSelected)
        {
            saveButton.SetEnabled(isFileSelected);
            overviewButton.SetEnabled(isFileSelected);
            graphView.SetEnabled(isFileSelected);
        }
    }
}
