using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;

using SkillEditor.Data;
namespace SkillEditor.Editor
{
    public abstract class SkillNodeBase : Node
    {
        public string Guid { get; set; }
        public NodeType NodeType { get; protected set; }
        public NodeData NodeData { get; set; }

        // 默认输入端口
        protected Port defaultInputPort;

        /// <summary>
        /// 当节点数据变化时触发，用于通知属性面板刷新
        /// </summary>
        public event Action OnDataChanged;

        protected SkillNodeBase(NodeType nodeType, Vector2 position)
        {
            Guid = System.Guid.NewGuid().ToString();
            NodeType = nodeType;

            // 调用子类实现的 CreateNodeData 创建正确类型的数据
            NodeData = CreateNodeData();
            NodeData.guid = Guid;
            NodeData.nodeType = nodeType;
            NodeData.position = position;

            SetPosition(new Rect(position, Vector2.zero));

            // 先创建默认端口
            CreateDefaultPorts();


            style.width = GetNodeWidth();
            style.overflow = Overflow.Visible;  // 允许节点内容溢出显示
            title = GetNodeTitle();
            SetupMainContainerStyle();

            // 再创建子类内容
            CreateContent();

            RefreshExpandedState();
            RefreshPorts();

            // 设置节点的zIndex为最下层
            this.layer = -100;

            // 延迟调用以确保UI元素已创建
            this.schedule.Execute(() => ApplyNodeCategoryColor()).ExecuteLater(50);

            // 为节点添加点击事件处理
            this.RegisterCallback<MouseDownEvent>(OnNodeMouseDown, TrickleDown.TrickleDown);
        }

        /// <summary>
        /// 创建节点数据实例，子类必须实现此方法返回正确类型的 NodeData 子类
        /// </summary>
        protected abstract NodeData CreateNodeData();

        /// <summary>
        /// 创建默认的输入端口，子类可以重写来禁用
        /// 输出端口由各节点自己定义，不提供默认输出端口
        /// </summary>
        protected virtual void CreateDefaultPorts()
        {
            if (HasDefaultInputPort)
            {
                defaultInputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
                defaultInputPort.portName = "输入";
                inputContainer.Add(defaultInputPort);
            }
        }

        /// <summary>
        /// 是否有默认输入端口，子类可重写
        /// </summary>
        protected virtual bool HasDefaultInputPort => true;

        protected abstract void CreateContent();
        protected abstract string GetNodeTitle();
        protected abstract float GetNodeWidth();

        protected virtual Color GetNodeCategoryColor()
        {
            // 根据节点分类返回统一颜色
            var category = NodeFactory.GetNodeCategory(NodeType);
            switch (category)
            {
                case NodeCategory.Root:
                    return new Color(0.8f, 0.5f, 0.2f); // 橙色
                case NodeCategory.Effect:
                    return new Color(0.7f, 0.2f, 0.2f); // 红色
                case NodeCategory.Cue:
                    return new Color(0.2f, 0.6f, 0.3f); // 绿色
                case NodeCategory.Task:
                    return new Color(0.2f, 0.4f, 0.8f); // 蓝色
                case NodeCategory.Condition:
                    return new Color(0.9f, 0.7f, 0.2f); // 黄色
                default:
                    return new Color(0.5f, 0.5f, 0.5f); // 灰色
            }
        }

        protected void SetupMainContainerStyle()
        {
            mainContainer.style.backgroundColor = new Color(70f / 255f, 70f / 255f, 70f / 255f);
            mainContainer.style.borderTopLeftRadius = 4;
            mainContainer.style.borderTopRightRadius = 4;
            mainContainer.style.borderBottomLeftRadius = 4;
            mainContainer.style.borderBottomRightRadius = 4;
            mainContainer.style.paddingLeft = 4;
            mainContainer.style.paddingRight = 4;
            mainContainer.style.paddingTop = 4;
            mainContainer.style.paddingBottom = 4;
            mainContainer.style.overflow = Overflow.Visible;

            // 设置所有内部容器允许溢出
            extensionContainer.style.overflow = Overflow.Visible;
            topContainer.style.overflow = Overflow.Visible;
            inputContainer.style.overflow = Overflow.Visible;
            outputContainer.style.overflow = Overflow.Visible;
        }

        protected void ApplyFieldStyle(VisualElement field)
        {
            field.style.borderTopLeftRadius = 4;
            field.style.borderTopRightRadius = 4;
            field.style.borderBottomLeftRadius = 4;
            field.style.borderBottomRightRadius = 4;
            field.style.marginTop = 4;

            // 让标签宽度自适应内容，输入框填充剩余空间
            var label = field.Q<Label>();
            if (label != null)
            {
                label.style.minWidth = StyleKeyword.Auto;
                label.style.width = StyleKeyword.Auto;
                label.style.marginRight = 4;
            }

            // 让输入框填充剩余空间
            var input = field.Q("unity-text-input");
            if (input != null)
            {
                input.style.flexGrow = 1;
            }
        }

        /// <summary>
        /// 创建公式输入框（支持数值或公式）
        /// </summary>
        protected TextField CreateFormulaField(string label, string value, System.Action<string> onChanged)
        {
            var field = new TextField(label) { value = value ?? "0" };
            ApplyFieldStyle(field);
            field.RegisterValueChangedCallback(evt => onChanged?.Invoke(evt.newValue));
            return field;
        }

        protected Port CreateOutputPort(string portName)
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
            port.portName = portName;
            outputContainer.Add(port);
            return port;
        }

        /// <summary>
        /// 创建标签集合选择字段
        /// </summary>
        protected GameplayTagSetField CreateTagSetField(string label, GameplayTagSet value, System.Action<GameplayTagSet> onChanged)
        {
            var field = new GameplayTagSetField(label);
            field.Value = value;
            field.OnValueChanged += onChanged;
            return field;
        }

        private void ApplyNodeCategoryColor()
        {
            // 直接使用Node的titleContainer属性
            if (titleContainer != null)
            {
                titleContainer.style.backgroundColor = GetNodeCategoryColor();

                // 设置标题文本为粗体白色
                var titleLabel = titleContainer.Q<Label>();
                if (titleLabel != null)
                {
                    titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    titleLabel.style.color = Color.white;
                }
            }
        }



        private void OnNodeMouseDown(MouseDownEvent evt)
        {
            // 只处理左键
            if (evt.button != 0) return;
        }

        public virtual void LoadData(NodeData data)
        {
            NodeData = data;
        }

        public virtual NodeData SaveData()
        {
            NodeData.position = GetPosition().position;
            return NodeData;
        }

        /// <summary>
        /// 通知数据已变化，触发属性面板刷新
        /// </summary>
        protected void NotifyDataChanged()
        {
            OnDataChanged?.Invoke();
        }

        /// <summary>
        /// 从NodeData同步UI显示（属性面板修改后调用）
        /// </summary>
        public virtual void SyncUIFromData()
        {
            // 子类重写此方法来更新UI
        }

        /// <summary>
        /// 根据端口标识符查找输出端口（子类可重写以支持自定义端口查找）
        /// </summary>
        /// <param name="portIdentifier">端口标识符（portName 或 portId）</param>
        /// <returns>找到的端口，未找到返回 null</returns>
        public virtual Port FindOutputPortByIdentifier(string portIdentifier)
        {
            // 默认实现：在 outputContainer 中通过 portName 查找
            return outputContainer
                .Query<Port>()
                .ToList()
                .Find(p => p.portName == portIdentifier);
        }
    }

    /// <summary>
    /// 泛型节点基类，提供强类型数据访问
    /// </summary>
    /// <typeparam name="TData">节点数据类型</typeparam>
    public abstract class SkillNodeBase<TData> : SkillNodeBase where TData : NodeData, new()
    {
        /// <summary>
        /// 强类型数据访问
        /// </summary>
        public TData TypedData => NodeData as TData;

        protected SkillNodeBase(NodeType nodeType, Vector2 position) : base(nodeType, position)
        {
        }

        protected override NodeData CreateNodeData()
        {
            return new TData();
        }
    }
}
