using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 效果节点统一基类 - 所有Effect节点的基类
    /// 重构后的统一基类，通过属性控制显示哪些配置和端口
    /// </summary>
    public abstract class EffectNode<TData> : SkillNodeBase<TData> where TData : EffectNodeData, new()
    {
        // ============ UI 字段 ============
        private EnumField durationTypeField;
        private TextField durationField;
        private VisualElement durationContainer;
        private Toggle isPeriodicToggle;
        private TextField periodField;
        private Toggle executeOnAppToggle;
        private VisualElement periodicContainer;
        private EnumField stackTypeField;
        private VisualElement stackParamsContainer;
        private IntegerField stackLimitField;
        private Toggle refreshDurationToggle;
        private Toggle resetPeriodToggle;
        private VisualElement modifiersContainer;

        // ============ 输出端口 ============
        protected Port initialEffectPort;
        protected Port periodicEffectPort;
        protected Port refreshEffectPort;
        protected Port completeEffectPort;
        protected Port removeAllEffectPort;
        protected Port overflowEffectPort;  // 新增：溢出效果端口

        protected EffectNode(NodeType nodeType, Vector2 position) : base(nodeType, position)
        {
        }

        /// <summary>
        /// 效果节点默认有输入端口
        /// </summary>
        protected override bool HasDefaultInputPort => true;
        // ============ 显示控制属性 - 子类可重写 ============
        /// 是否显示持续类型配置（瞬时/持续/永久）
        protected virtual bool ShowDurationTypeConfig => false;
        /// 是否显示持续时间配置
        protected virtual bool ShowDurationConfig => false;
        /// 是否显示周期配置
        protected virtual bool ShowPeriodicConfig => false;
        /// 是否显示属性修改器配置
        protected virtual bool ShowAttributeModifiers => false;
        /// 是否显示堆叠配置
        protected virtual bool ShowStackConfig => false;

        // ============ 端口显示控制 - 子类可重写 ============

        protected virtual bool ShowInitialEffectPort => false;
        protected virtual bool ShowPeriodicEffectPort => false;
        protected virtual bool ShowRefreshEffectPort => false;
        protected virtual bool ShowCompleteEffectPort => false;
        protected virtual bool ShowRemoveAllEffectPort => false;
        protected virtual bool ShowOverflowEffectPort => false;  // 新增：溢出效果端口

    

        /// <summary>
        /// 基类绘制逻辑
        /// </summary>
        protected override void CreateContent()
        {
            // 创建输出端口
            CreateEffectOutputPorts();

            // 让子类绘制自己的内容
            CreateEffectContent();

            // 根据配置绘制通用UI
            if (ShowDurationTypeConfig)
            {
                CreateDurationTypeUI();
            }

            if (ShowDurationConfig)
            {
                CreateDurationUI();
            }

            if (ShowPeriodicConfig)
            {
                CreatePeriodicUI();
            }

            if (ShowAttributeModifiers)
            {
                CreateModifiersUI();
            }

            if (ShowStackConfig)
            {
                CreateStackConfigUI();
            }
        }

        /// <summary>
        /// 子类实现具体的节点内容绘制
        /// </summary>
        protected virtual void CreateEffectContent() { }

        #region 输出端口

        private void CreateEffectOutputPorts()
        {
            if (ShowInitialEffectPort)
            {
                initialEffectPort = CreateOutputPort("初始效果");
            }

            if (ShowPeriodicEffectPort)
            {
                periodicEffectPort = CreateOutputPort("每周期执行");
            }

            if (ShowRefreshEffectPort)
            {
                refreshEffectPort = CreateOutputPort("刷新时");
            }

            if (ShowCompleteEffectPort)
            {
                completeEffectPort = CreateOutputPort("完成效果");
            }

            if (ShowRemoveAllEffectPort)
            {
                removeAllEffectPort = CreateOutputPort("全部移除后");
            }

            // 溢出效果端口（橙色，表示堆叠溢出时触发）
            if (ShowOverflowEffectPort)
            {
                overflowEffectPort = CreateOutputPort("溢出");
                overflowEffectPort.portColor = new Color(1f, 0.6f, 0.2f);
            }
        }

        #endregion

        #region 持续类型配置 UI

        private void CreateDurationTypeUI()
        {
            var container = CreateConfigContainer("持续类型");

            durationTypeField = new EnumField(TypedData?.durationType ?? EffectDurationType.Instant);
            ApplyFieldStyle(durationTypeField);
            durationTypeField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.durationType = (EffectDurationType)evt.newValue;
                    UpdateDurationVisibility();
                    NotifyDataChanged();
                }
            });
            container.Add(durationTypeField);

            mainContainer.Add(container);
        }

        private void UpdateDurationVisibility()
        {
            if (durationContainer != null && TypedData != null)
            {
                bool showDuration = TypedData.durationType == EffectDurationType.Duration;
                durationContainer.style.display = showDuration ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        #endregion

        #region 持续时间配置 UI

        private void CreateDurationUI()
        {
            durationContainer = CreateConfigContainer("持续时间");

            // 持续时间（公式字段）
            durationField = CreateFormulaField("时间(秒)", TypedData?.duration ?? "0", value =>
            {
                if (TypedData != null)
                {
                    TypedData.duration = value;
                    NotifyDataChanged();
                }
            });
            durationContainer.Add(durationField);

            mainContainer.Add(durationContainer);
        }

        #endregion

        #region 周期执行配置 UI

        private void CreatePeriodicUI()
        {
            periodicContainer = CreateConfigContainer("周期执行");

            // 是否周期执行
            isPeriodicToggle = new Toggle("启用周期执行") { value = TypedData?.isPeriodic ?? false };
            isPeriodicToggle.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.isPeriodic = evt.newValue;
                    UpdatePeriodicParamsVisibility();
                    NotifyDataChanged();
                }
            });
            periodicContainer.Add(isPeriodicToggle);

            // 周期参数容器
            var periodicParamsContainer = new VisualElement { name = "periodicParams" };

            // 周期（公式字段）
            periodField = CreateFormulaField("周期(秒)", TypedData?.period ?? "1", value =>
            {
                if (TypedData != null)
                {
                    TypedData.period = value;
                    NotifyDataChanged();
                }
            });
            periodicParamsContainer.Add(periodField);

            executeOnAppToggle = new Toggle("立即执行一次") { value = TypedData?.executeOnApplication ?? false };
            executeOnAppToggle.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.executeOnApplication = evt.newValue;
                    NotifyDataChanged();
                }
            });
            periodicParamsContainer.Add(executeOnAppToggle);

            periodicContainer.Add(periodicParamsContainer);
            mainContainer.Add(periodicContainer);
            UpdatePeriodicParamsVisibility();
        }

        private void UpdatePeriodicParamsVisibility()
        {
            var periodicParams = periodicContainer?.Q("periodicParams");
            if (periodicParams != null && TypedData != null)
            {
                periodicParams.style.display = TypedData.isPeriodic ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        #endregion

        #region 属性修改器配置 UI

        private void CreateModifiersUI()
        {
            var container = CreateConfigContainer("属性修改器");

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

            var addButton = new Button { text = "+" };
            addButton.style.width = 20;
            addButton.style.height = 20;
            addButton.clicked += () => AddModifier();
            headerContainer.Add(addButton);

            container.Add(headerContainer);

            modifiersContainer = new VisualElement();
            container.Add(modifiersContainer);

            mainContainer.Add(container);
            RefreshModifiersUI();
        }

        private void AddModifier()
        {
            if (TypedData == null) return;
            if (TypedData.attributeModifiers == null)
                TypedData.attributeModifiers = new List<AttributeModifierData>();

            TypedData.attributeModifiers.Add(new AttributeModifierData());
            RefreshModifiersUI();
            NotifyDataChanged();
        }

        private void RefreshModifiersUI()
        {
            if (modifiersContainer == null || TypedData == null) return;
            modifiersContainer.Clear();

            if (TypedData.attributeModifiers == null)
                TypedData.attributeModifiers = new List<AttributeModifierData>();

            for (int i = 0; i < TypedData.attributeModifiers.Count; i++)
            {
                int index = i;
                var modifier = TypedData.attributeModifiers[index];

                var itemContainer = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        alignItems = Align.Center,
                        marginBottom = 2
                    }
                };

                // 属性类型
                var attrTypeField = new EnumField(modifier.attrType);
                attrTypeField.style.width = 60;
                attrTypeField.RegisterValueChangedCallback(evt =>
                {
                    if (TypedData != null && index < TypedData.attributeModifiers.Count)
                    {
                        TypedData.attributeModifiers[index].attrType = (AttrType)evt.newValue;
                        NotifyDataChanged();
                    }
                });
                itemContainer.Add(attrTypeField);

                // 操作类型
                var opField = new EnumField(modifier.operation);
                opField.style.width = 60;
                opField.RegisterValueChangedCallback(evt =>
                {
                    if (TypedData != null && index < TypedData.attributeModifiers.Count)
                    {
                        TypedData.attributeModifiers[index].operation = (ModifierOperation)evt.newValue;
                        NotifyDataChanged();
                    }
                });
                itemContainer.Add(opField);

                // 数值
                var valueField = new FloatField { value = modifier.fixedValue };
                valueField.style.width = 50;
                valueField.RegisterValueChangedCallback(evt =>
                {
                    if (TypedData != null && index < TypedData.attributeModifiers.Count)
                    {
                        TypedData.attributeModifiers[index].fixedValue = evt.newValue;
                        NotifyDataChanged();
                    }
                });
                itemContainer.Add(valueField);

                // 删除按钮
                var deleteBtn = new Button { text = "×" };
                deleteBtn.style.width = 20;
                deleteBtn.style.height = 20;
                deleteBtn.clicked += () =>
                {
                    if (TypedData != null && index < TypedData.attributeModifiers.Count)
                    {
                        TypedData.attributeModifiers.RemoveAt(index);
                        RefreshModifiersUI();
                        NotifyDataChanged();
                    }
                };
                itemContainer.Add(deleteBtn);

                modifiersContainer.Add(itemContainer);
            }
        }

        #endregion

        #region 堆叠配置 UI

        private void CreateStackConfigUI()
        {
            var container = CreateConfigContainer("堆叠配置");

            // 堆叠类型
            stackTypeField = new EnumField(TypedData?.stackType ?? StackType.None);
            ApplyFieldStyle(stackTypeField);
            stackTypeField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.stackType = (StackType)evt.newValue;
                    UpdateStackParamsVisibility();
                    NotifyDataChanged();
                }
            });
            container.Add(stackTypeField);

            // 堆叠参数容器
            stackParamsContainer = new VisualElement();

            stackLimitField = new IntegerField("上限(0=无限)") { value = TypedData?.stackLimit ?? 0 };
            ApplyFieldStyle(stackLimitField);
            stackLimitField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.stackLimit = evt.newValue;
                    NotifyDataChanged();
                }
            });
            stackParamsContainer.Add(stackLimitField);

            // 使用新的策略枚举（简化显示，详细配置在Inspector中）
            refreshDurationToggle = new Toggle("刷新持续时间")
            {
                value = TypedData?.stackDurationRefreshPolicy == StackDurationRefreshPolicy.RefreshOnSuccessfulApplication
            };
            refreshDurationToggle.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.stackDurationRefreshPolicy = evt.newValue
                        ? StackDurationRefreshPolicy.RefreshOnSuccessfulApplication
                        : StackDurationRefreshPolicy.NeverRefresh;
                    NotifyDataChanged();
                }
            });
            stackParamsContainer.Add(refreshDurationToggle);

            resetPeriodToggle = new Toggle("重置周期")
            {
                value = TypedData?.stackPeriodResetPolicy == StackPeriodResetPolicy.ResetOnSuccessfulApplication
            };
            resetPeriodToggle.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.stackPeriodResetPolicy = evt.newValue
                        ? StackPeriodResetPolicy.ResetOnSuccessfulApplication
                        : StackPeriodResetPolicy.NeverReset;
                    NotifyDataChanged();
                }
            });
            stackParamsContainer.Add(resetPeriodToggle);

            container.Add(stackParamsContainer);
            mainContainer.Add(container);
            UpdateStackParamsVisibility();
        }

        private void UpdateStackParamsVisibility()
        {
            if (stackParamsContainer != null && TypedData != null)
            {
                bool showParams = TypedData.stackType == StackType.AggregateByTarget ||
                                  TypedData.stackType == StackType.AggregateBySource;
                stackParamsContainer.style.display = showParams ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 创建配置容器
        /// </summary>
        protected VisualElement CreateConfigContainer(string title)
        {
            var container = new VisualElement
            {
                style =
                {
                    backgroundColor = new Color(56f / 255f, 56f / 255f, 56f / 255f),
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,
                    paddingLeft = 6,
                    paddingRight = 6,
                    paddingTop = 4,
                    paddingBottom = 4,
                    marginTop = 4
                }
            };

            var titleLabel = new Label(title)
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 4 }
            };
            container.Add(titleLabel);

            return container;
        }

        #endregion

        #region 数据同步

        public override void LoadData(NodeData data)
        {
            base.LoadData(data);
            SyncEffectUIFromData();
        }

        public override void SyncUIFromData()
        {
            base.SyncUIFromData();
            SyncEffectUIFromData();
        }

        private void SyncEffectUIFromData()
        {
            if (TypedData == null) return;

            // 同步持续类型配置
            if (durationTypeField != null)
                durationTypeField.SetValueWithoutNotify(TypedData.durationType);
            UpdateDurationVisibility();

            // 同步持续时间配置
            if (durationField != null)
                durationField.SetValueWithoutNotify(TypedData.duration ?? "0");

            // 同步周期配置
            if (isPeriodicToggle != null)
                isPeriodicToggle.SetValueWithoutNotify(TypedData.isPeriodic);
            if (periodField != null)
                periodField.SetValueWithoutNotify(TypedData.period ?? "1");
            if (executeOnAppToggle != null)
                executeOnAppToggle.SetValueWithoutNotify(TypedData.executeOnApplication);
            UpdatePeriodicParamsVisibility();

            // 同步堆叠配置
            if (stackTypeField != null)
                stackTypeField.SetValueWithoutNotify(TypedData.stackType);
            if (stackLimitField != null)
                stackLimitField.SetValueWithoutNotify(TypedData.stackLimit);
            if (refreshDurationToggle != null)
                refreshDurationToggle.SetValueWithoutNotify(
                    TypedData.stackDurationRefreshPolicy == StackDurationRefreshPolicy.RefreshOnSuccessfulApplication);
            if (resetPeriodToggle != null)
                resetPeriodToggle.SetValueWithoutNotify(
                    TypedData.stackPeriodResetPolicy == StackPeriodResetPolicy.ResetOnSuccessfulApplication);
            UpdateStackParamsVisibility();

            // 同步属性修改器
            RefreshModifiersUI();

            // 调用子类的同步
            SyncEffectContentFromData();
        }

        /// <summary>
        /// 子类实现自己的数据同步
        /// </summary>
        protected virtual void SyncEffectContentFromData() { }

        #endregion
    }
}
