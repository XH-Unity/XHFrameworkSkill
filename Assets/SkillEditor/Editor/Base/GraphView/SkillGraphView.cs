using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

using SkillEditor.Data;
namespace SkillEditor.Editor
{
    public partial class SkillGraphView : GraphView
    {
        private Vector2 pastePosition;

        public event Action<SkillNodeBase> OnNodeSelected;
        public event Action OnGraphModified;

        public SkillGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            style.backgroundColor = new Color(157f/255f, 210f/255f, 236f/255f,0.5F);

            styleSheets.Add(Resources.Load<StyleSheet>("SkillGraphViewStyle"));

            this.AddManipulator(CreateContextualMenu());

            serializeGraphElements = SerializeGraphElements;
            unserializeAndPaste = UnserializeAndPaste;
            canPasteSerializedData = CanPasteSerializedData;

            graphViewChanged = OnGraphViewChanged;

            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            pastePosition = contentViewContainer.WorldToLocal(evt.mousePosition);
        }

        private void OnKeyDown(KeyDownEvent evt) 
        {
            if (evt.ctrlKey)
            {
                switch (evt.keyCode)
                {
                    case KeyCode.C:
                        CopySelection();
                        evt.StopPropagation();
                        break;
                    case KeyCode.V:
                        PasteSelection();
                        evt.StopPropagation();
                        break;
                    case KeyCode.X:
                        CutSelection();
                        evt.StopPropagation();
                        break;
                }
            }
        }

        private void CopySelection()
        {
            var selectedElements = selection.OfType<GraphElement>();
            if (selectedElements.Any())
            {
                var data = SerializeGraphElements(selectedElements);
                EditorGUIUtility.systemCopyBuffer = data;
            }
        }

        private void PasteSelection()
        {
            var data = EditorGUIUtility.systemCopyBuffer;
            if (!string.IsNullOrEmpty(data) && CanPasteSerializedData(data))
            {
                UnserializeAndPaste("Paste", data);
            }
        }

        private void CutSelection()
        {
            CopySelection();
            DeleteSelection();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            // 连线增删、节点删除都触发脏标记
            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    if (element is SkillNodeBase)
                    {
                        OnNodeSelected?.Invoke(null);
                    }
                }
                OnGraphModified?.Invoke();
            }

            if (change.edgesToCreate != null && change.edgesToCreate.Count > 0)
            {
                OnGraphModified?.Invoke();
            }

            if (change.movedElements != null && change.movedElements.Count > 0)
            {
                OnGraphModified?.Invoke();
            }

            return change;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
        }

        public void SetNodeSelectionCallback()
        {
            // 监听GraphView中所有元素的鼠标点击
            this.RegisterCallback<MouseDownEvent>(evt =>
            {
                // 只处理左键（button 0）
                if (evt.button != 0) return;

                // 获取点击的元素
                var clickedElement = evt.target as VisualElement;

                // 逐级向上查找SkillNodeBase
                SkillNodeBase node = null;
                var current = clickedElement;
                while (current != null)
                {
                    node = current as SkillNodeBase;
                    if (node != null) break;
                    current = current.parent;
                }

                if (node != null)
                {
                    // 立即触发选择事件
                    OnNodeSelected?.Invoke(node);
                }
                else
                {
                    // 点击空白区域，清除选择
                    OnNodeSelected?.Invoke(null);
                }
            }, TrickleDown.TrickleDown);
        }

        public void ClearGraph()
        {
            foreach (var node in nodes.ToList())
                RemoveElement(node);

            foreach (var edge in edges.ToList())
                RemoveElement(edge);
        }
    }
}
