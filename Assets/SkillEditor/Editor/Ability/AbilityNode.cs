using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 技能节点 - 对应 GameplayAbility
    /// </summary>
    public class AbilityNode : SkillNodeBase<AbilityNodeData>
    {
        // 右侧输出端口
        private Port activatePort;
        private Port animationPort;

        // 左侧输出端口（消耗、冷却）
        private Port costPort;
        private Port cooldownPort;

        // 事件输出端口
        private List<Port> eventOutputPorts = new List<Port>();
        private VisualElement eventPortsContainer;

        public AbilityNode(Vector2 position) : base(NodeType.Ability, position) { }

        protected override string GetNodeTitle() => "技能";
        protected override float GetNodeWidth() => 310;
        // 技能节点没有输入端口
        protected override bool HasDefaultInputPort => false;

        protected override void CreateContent()
        {
            // 右侧输出端口
            activatePort = CreateOutputPort("激活");
            animationPort = CreateOutputPort("动画");

            // 左侧输出端口（消耗、冷却放在左边，视觉上更清晰）
            costPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
            costPort.portName = "消耗";
            inputContainer.Add(costPort);

            cooldownPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
            cooldownPort.portName = "冷却";
            inputContainer.Add(cooldownPort);

            // 创建事件监听端口区域
            CreateEventPortsSection();
        }

        private void UpdateTitle()
        {
            var skillId = TypedData?.skillId ?? 0;
            title = skillId > 0 ? $"技能 [{skillId}]" : "技能";
        }

        #region 事件监听端口

        private void CreateEventPortsSection()
        {
            eventPortsContainer = new VisualElement
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

            var titleLabel = new Label("事件监听")
            {
                style =
                {
                    fontSize = 12,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    color = Color.white
                }
            };

            var addButton = new Button { text = "+" };
            addButton.style.width = 24;
            addButton.style.height = 24;
            ApplyButtonStyle(addButton);
            addButton.clicked += () => AddEventOutputPort();

            headerContainer.Add(titleLabel);
            headerContainer.Add(addButton);
            eventPortsContainer.Add(headerContainer);

            // 事件端口列表容器
            var portsListContainer = new VisualElement { name = "EventPortsListContainer" };
            eventPortsContainer.Add(portsListContainer);

            outputContainer.Add(eventPortsContainer);
        }

        private void AddEventOutputPort(AbilityEventPortData eventData = null)
        {
            if (TypedData == null) return;
            if (TypedData.eventOutputPorts == null)
                TypedData.eventOutputPorts = new List<AbilityEventPortData>();

            // 如果没有传入数据，创建新的
            if (eventData == null)
            {
                eventData = new AbilityEventPortData();
                TypedData.eventOutputPorts.Add(eventData);
                NotifyDataChanged();
            }

            int index = TypedData.eventOutputPorts.IndexOf(eventData);
            if (index < 0) index = TypedData.eventOutputPorts.Count - 1;

            // 创建端口
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
            port.portName = GetEventPortName(eventData);
            eventData.PortId = port.portName;
            port.portColor = new Color(0.3f, 0.7f, 0.9f); // 浅蓝色区分事件端口

            // 创建端口行容器
            var portRowContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginBottom = 4
                }
            };

            // 事件类型下拉框
            var eventTypeField = new EnumField(eventData.eventType);
            eventTypeField.style.width = 100;
            eventTypeField.style.marginRight = 4;
            ApplyFieldStyle(eventTypeField);

            // 自定义标签输入框（仅Custom时显示）
            var customTagField = new TextField { value = eventData.customEventTag ?? "" };
            customTagField.style.width = 60;
            customTagField.style.marginRight = 4;
            customTagField.style.display = eventData.eventType == GameplayEventType.Custom ? DisplayStyle.Flex : DisplayStyle.None;
            ApplyFieldStyle(customTagField);

            // 删除按钮
            var deleteButton = new Button { text = "×" };
            deleteButton.style.width = 20;
            deleteButton.style.height = 20;
            ApplyButtonStyle(deleteButton);

            // 事件绑定
            int currentIndex = index;
            eventTypeField.RegisterValueChangedCallback(evt =>
            {
                var newType = (GameplayEventType)evt.newValue;
                if (TypedData != null && currentIndex < TypedData.eventOutputPorts.Count)
                {
                    TypedData.eventOutputPorts[currentIndex].eventType = newType;
                    port.portName = GetEventPortName(TypedData.eventOutputPorts[currentIndex]);
                    TypedData.eventOutputPorts[currentIndex].PortId = port.portName;
                    customTagField.style.display = newType == GameplayEventType.Custom ? DisplayStyle.Flex : DisplayStyle.None;
                    NotifyDataChanged();
                }
            });

            customTagField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null && currentIndex < TypedData.eventOutputPorts.Count)
                {
                    TypedData.eventOutputPorts[currentIndex].customEventTag = evt.newValue;
                    port.portName = GetEventPortName(TypedData.eventOutputPorts[currentIndex]);
                    TypedData.eventOutputPorts[currentIndex].PortId = port.portName;
                    NotifyDataChanged();
                }
            });

            deleteButton.clicked += () =>
            {
                RemoveEventOutputPort(currentIndex, port, portRowContainer);
            };

            portRowContainer.Add(eventTypeField);
            portRowContainer.Add(customTagField);
            portRowContainer.Add(port);
            portRowContainer.Add(deleteButton);

            var portsListContainer = eventPortsContainer.Q("EventPortsListContainer");
            portsListContainer?.Add(portRowContainer);

            eventOutputPorts.Add(port);
            RefreshPorts();
        }

        private void RemoveEventOutputPort(int index, Port port, VisualElement portRowContainer)
        {
            if (TypedData == null || TypedData.eventOutputPorts == null) return;
            if (index < 0 || index >= TypedData.eventOutputPorts.Count) return;

            TypedData.eventOutputPorts.RemoveAt(index);
            eventOutputPorts.Remove(port);
            portRowContainer.RemoveFromHierarchy();
            NotifyDataChanged();

            // 重建端口列表以更新索引
            RefreshEventPortsList();
        }

        private void RefreshEventPortsList()
        {
            // 清除旧的端口
            foreach (var port in eventOutputPorts)
            {
                port.RemoveFromHierarchy();
            }
            eventOutputPorts.Clear();

            var portsListContainer = eventPortsContainer?.Q("EventPortsListContainer");
            if (portsListContainer != null)
                portsListContainer.Clear();

            if (TypedData == null) return;
            if (TypedData.eventOutputPorts == null)
                TypedData.eventOutputPorts = new List<AbilityEventPortData>();

            // 重新创建所有事件端口
            foreach (var eventData in TypedData.eventOutputPorts)
            {
                AddEventOutputPort(eventData);
            }
        }

        private string GetEventPortName(AbilityEventPortData eventData)
        {
            switch (eventData.eventType)
            {
                case GameplayEventType.OnHit:
                    return "受击时";
                case GameplayEventType.OnDealDamage:
                    return "造成伤害时";
                case GameplayEventType.OnTakeDamage:
                    return "受到伤害时";
                case GameplayEventType.OnDeath:
                    return "死亡时";
                case GameplayEventType.OnKill:
                    return "击杀时";
                case GameplayEventType.Custom:
                    return string.IsNullOrEmpty(eventData.customEventTag) ? "自定义事件" : eventData.customEventTag;
                default:
                    return "事件";
            }
        }

        private void ApplyButtonStyle(Button button)
        {
            button.style.paddingLeft = 0;
            button.style.paddingRight = 0;
            button.style.paddingTop = 0;
            button.style.paddingBottom = 0;
            button.style.borderTopLeftRadius = 4;
            button.style.borderTopRightRadius = 4;
            button.style.borderBottomLeftRadius = 4;
            button.style.borderBottomRightRadius = 4;
        }

        #endregion

        #region 端口查找

        /// <summary>
        /// 根据端口标识符查找输出端口（支持普通端口和事件端口）
        /// </summary>
        public override Port FindOutputPortByIdentifier(string portIdentifier)
        {
            // 1. 先查找普通输出端口（激活、消耗、冷却、动画）
            if (activatePort?.portName == portIdentifier) return activatePort;
            if (costPort?.portName == portIdentifier) return costPort;
            if (cooldownPort?.portName == portIdentifier) return cooldownPort;
            if (animationPort?.portName == portIdentifier) return animationPort;

            // 2. 查找事件输出端口
            foreach (var port in eventOutputPorts)
            {
                if (port.portName == portIdentifier)
                    return port;
            }

            // 3. 回退到基类实现
            return base.FindOutputPortByIdentifier(portIdentifier);
        }

        #endregion

        #region 数据加载/保存

        public override void LoadData(NodeData data)
        {
            base.LoadData(data);
            UpdateTitle();
            SyncUIFromData();
        }

        public override void SyncUIFromData()
        {
            base.SyncUIFromData();
            if (TypedData == null) return;
            UpdateTitle();

            // 刷新事件端口列表
            RefreshEventPortsList();
        }

        #endregion
    }
}
