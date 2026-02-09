using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

using SkillEditor.Data;
namespace SkillEditor.Editor
{
    /// <summary>
    /// 效果节点Inspector基类 - 所有Effect节点的Inspector继承此类
    /// 子类通过重写 Show* 属性控制显示哪些配置区域
    /// </summary>
    public abstract class EffectNodeInspector : NodeInspectorBase
    {
        // ============ 显示控制属性 - 子类可重写 ============

        /// <summary>
        /// Effect 节点默认显示节点目标
        /// </summary>
        protected override bool ShowTargetType => true;

        /// <summary>
        /// 是否显示持续时间配置
        /// </summary>
        protected virtual bool ShowDurationConfig => false;

        /// <summary>
        /// 是否显示周期执行配置
        /// </summary>
        protected virtual bool ShowPeriodicConfig => false;

        /// <summary>
        /// 是否显示属性修改器配置
        /// </summary>
        protected virtual bool ShowAttributeModifiers => false;

        /// <summary>
        /// 是否显示堆叠配置
        /// </summary>
        protected virtual bool ShowStackConfig => false;

        /// <summary>
        /// 是否显示标签配置区域
        /// </summary>
        protected virtual bool ShowEffectTags => true;

        /// <summary>
        /// 标签配置区域是否默认展开
        /// </summary>
        protected virtual bool EffectTagsDefaultExpanded => false;

        protected sealed override void BuildInspectorUI(VisualElement container, SkillNodeBase node)
        {
            var effectData = GetEffectNodeData(node);
            if (effectData == null) return;

            // 先绘制子类的具体内容
            BuildEffectInspectorUI(container, node);

            // 根据子类的显示控制属性绘制对应区域
            if (ShowDurationConfig)
            {
                container.Add(CreateDurationSection(node, effectData));
            }

            if (ShowPeriodicConfig)
            {
                container.Add(CreatePeriodicSection(node, effectData));
            }

            if (ShowAttributeModifiers)
            {
                container.Add(CreateCalculationSection(node, effectData));
            }

            if (ShowStackConfig)
            {
                container.Add(CreateStackConfigSection(node, effectData));
            }

            // 移除策略（瞬时类型不需要显示）
            if (effectData.durationType != EffectDurationType.Instant)
            {
                container.Add(CreateRemovalPolicySection(node, effectData));
            }

            if (ShowEffectTags)
            {
                container.Add(CreateEffectTagsSection(node, effectData));
            }
        }

        /// <summary>
        /// 子类实现具体的Effect Inspector UI绘制
        /// </summary>
        protected abstract void BuildEffectInspectorUI(VisualElement container, SkillNodeBase node);

        /// <summary>
        /// 获取EffectNodeData
        /// </summary>
        protected virtual EffectNodeData GetEffectNodeData(SkillNodeBase node)
        {
            return node?.NodeData as EffectNodeData;
        }

        #region 持续时间配置

        /// <summary>
        /// 创建持续时间配置区域
        /// </summary>
        protected VisualElement CreateDurationSection(SkillNodeBase node, EffectNodeData data)
        {
            var section = CreateCollapsibleSection("持续时间", out var content, true);

            // 所有 EffectNodeData 都有持续时间配置
            // 持续时间（公式字段）
            var durationField = CreateFormulaField("持续时间(秒)", data.duration ?? "0", value =>
            {
                data.duration = value;
                node.SyncUIFromData();
            });
            content.Add(durationField);

            return section;
        }

        #endregion

        #region 周期执行配置

        /// <summary>
        /// 创建周期执行配置区域
        /// </summary>
        protected VisualElement CreatePeriodicSection(SkillNodeBase node, EffectNodeData data)
        {
            var section = CreateCollapsibleSection("周期执行", out var content);

            // 所有 EffectNodeData 都有周期配置
            // 是否启用周期执行
            var isPeriodicToggle = new Toggle("启用周期执行") { value = data.isPeriodic };
            isPeriodicToggle.style.marginBottom = 4;
            content.Add(isPeriodicToggle);

            // 周期参数容器
            var periodicParamsContainer = new VisualElement();
            content.Add(periodicParamsContainer);

            void UpdatePeriodicParams(bool isPeriodic)
            {
                periodicParamsContainer.Clear();
                if (isPeriodic)
                {
                    // 执行周期（公式字段）
                    periodicParamsContainer.Add(CreateFormulaField("执行周期(秒)", data.period ?? "1", value =>
                    {
                        data.period = value;
                        node.SyncUIFromData();
                    }));

                    var executeOnAppToggle = new Toggle("应用时立即执行一次") { value = data.executeOnApplication };
                    executeOnAppToggle.style.marginTop = 4;
                    executeOnAppToggle.RegisterValueChangedCallback(evt =>
                    {
                        data.executeOnApplication = evt.newValue;
                        node.SyncUIFromData();
                    });
                    periodicParamsContainer.Add(executeOnAppToggle);
                }
            }

            isPeriodicToggle.RegisterValueChangedCallback(evt =>
            {
                data.isPeriodic = evt.newValue;
                UpdatePeriodicParams(evt.newValue);
                node.SyncUIFromData();
            });

            UpdatePeriodicParams(data.isPeriodic);

            return section;
        }

        #endregion

        #region 属性修改器配置

        /// <summary>
        /// 创建计算配置区域
        /// </summary>
        protected VisualElement CreateCalculationSection(SkillNodeBase node, EffectNodeData data)
        {
            var section = CreateCollapsibleSection("效果计算", out var content, true);

            // 所有 EffectNodeData 都有属性修改器
            BuildModifiersContent(content, node, data);

            return section;
        }

        /// <summary>
        /// 构建 Modifiers 内容
        /// </summary>
        private void BuildModifiersContent(VisualElement container, SkillNodeBase node, EffectNodeData data)
        {
            if (data.attributeModifiers == null)
                data.attributeModifiers = new List<AttributeModifierData>();

            RefreshModifiersList(container, node, data.attributeModifiers);
        }

        /// <summary>
        /// 创建属性修改器配置区域（保留旧方法以兼容）
        /// </summary>
        protected VisualElement CreateModifiersSection(SkillNodeBase node, EffectNodeData data)
        {
            var section = CreateCollapsibleSection("属性修改器", out var content);

            if (data.attributeModifiers == null)
                data.attributeModifiers = new List<AttributeModifierData>();

            RefreshModifiersList(content, node, data.attributeModifiers);

            return section;
        }

        /// <summary>
        /// 刷新属性修改器列表（通用方法，支持任何 List<AttributeModifierData>）
        /// </summary>
        protected void RefreshModifiersList(VisualElement content, SkillNodeBase node, List<AttributeModifierData> modifiers, string addButtonText = "+ 添加属性修改器")
        {
            content.Clear();
            if (modifiers == null) return;

            for (int i = 0; i < modifiers.Count; i++)
            {
                int index = i;
                var modifier = modifiers[index];

                var modifierContainer = new VisualElement
                {
                    style =
                    {
                        backgroundColor = new Color(45f / 255f, 45f / 255f, 45f / 255f),
                        borderTopLeftRadius = 4, borderTopRightRadius = 4,
                        borderBottomLeftRadius = 4, borderBottomRightRadius = 4,
                        paddingLeft = 6, paddingRight = 6, paddingTop = 6, paddingBottom = 6,
                        marginBottom = 4
                    }
                };

                // 使用新的属性选择器
                var attributeField = new AttributeField("目标属性");
                attributeField.Value = modifier.attrType;
                attributeField.OnValueChanged += value =>
                {
                    modifiers[index].attrType = value;
                    node.SyncUIFromData();
                };
                modifierContainer.Add(attributeField);

                var operationField = new EnumField("操作", modifier.operation) { style = { marginBottom = 4 } };
                ApplyEnumFieldStyle(operationField);
                operationField.RegisterValueChangedCallback(evt =>
                {
                    modifiers[index].operation = (ModifierOperation)evt.newValue;
                    node.SyncUIFromData();
                });
                modifierContainer.Add(operationField);

                // ========== 数值行：来源类型下拉框 + 输入框 ==========
                var valueRow = new VisualElement();
                valueRow.style.flexDirection = FlexDirection.Row;
                valueRow.style.marginBottom = 4;

                // 数值来源类型下拉框
                var sourceTypeField = new EnumField(modifier.magnitudeSourceType);
                sourceTypeField.style.width = 100;
                sourceTypeField.style.marginRight = 4;
                ApplyEnumFieldStyle(sourceTypeField);
                valueRow.Add(sourceTypeField);

                // ===== 具体值输入框 =====
                var fixedValueField = new FloatField { value = modifier.fixedValue };
                fixedValueField.style.flexGrow = 1;
                fixedValueField.style.display = modifier.magnitudeSourceType == ModifierMagnitudeSourceType.FixedValue
                    ? DisplayStyle.Flex : DisplayStyle.None;
                fixedValueField.RegisterValueChangedCallback(evt =>
                {
                    modifiers[index].fixedValue = evt.newValue;
                    node.SyncUIFromData();
                });
                valueRow.Add(fixedValueField);

                // ===== 公式输入框 =====
                var formulaField = new TextField { value = modifier.formula ?? "" };
                formulaField.style.flexGrow = 1;
                formulaField.style.display = modifier.magnitudeSourceType == ModifierMagnitudeSourceType.Formula
                    ? DisplayStyle.Flex : DisplayStyle.None;
                formulaField.RegisterValueChangedCallback(evt =>
                {
                    modifiers[index].formula = evt.newValue;
                    node.SyncUIFromData();
                });
                valueRow.Add(formulaField);

                // ===== MMC 类型枚举选择 =====
                var mmcTypeField = new EnumField(modifier.mmcType);
                mmcTypeField.style.flexGrow = 1;
                mmcTypeField.style.display = modifier.magnitudeSourceType == ModifierMagnitudeSourceType.ModifierMagnitudeCalculation
                    ? DisplayStyle.Flex : DisplayStyle.None;
                ApplyEnumFieldStyle(mmcTypeField);
                mmcTypeField.RegisterValueChangedCallback(evt =>
                {
                    modifiers[index].mmcType = (MMCType)evt.newValue;
                    node.SyncUIFromData();
                });
                valueRow.Add(mmcTypeField);

                // ===== 上下文数据键名输入框 =====
                var setByCallerField = new TextField { value = modifier.setByCallerKey ?? "" };
                setByCallerField.style.flexGrow = 1;
                setByCallerField.style.display = modifier.magnitudeSourceType == ModifierMagnitudeSourceType.SetByCaller
                    ? DisplayStyle.Flex : DisplayStyle.None;
                setByCallerField.RegisterValueChangedCallback(evt =>
                {
                    modifiers[index].setByCallerKey = evt.newValue;
                    node.SyncUIFromData();
                });
                valueRow.Add(setByCallerField);

                modifierContainer.Add(valueRow);

                // ===== MMC 详细配置容器（当选择 MMC 且类型为 AttributeBased 时显示）=====
                var mmcDetailContainer = new VisualElement();
                mmcDetailContainer.style.marginTop = 4;
                mmcDetailContainer.style.marginLeft = 8;
                mmcDetailContainer.style.paddingLeft = 8;
                mmcDetailContainer.style.borderLeftWidth = 2;
                mmcDetailContainer.style.borderLeftColor = new Color(0.3f, 0.6f, 0.9f);
                mmcDetailContainer.style.display = (modifier.magnitudeSourceType == ModifierMagnitudeSourceType.ModifierMagnitudeCalculation
                    && modifier.mmcType == MMCType.AttributeBased) ? DisplayStyle.Flex : DisplayStyle.None;

                // MMC 捕获属性
                var mmcCaptureAttrField = new AttributeField("捕获属性");
                mmcCaptureAttrField.Value = modifier.mmcCaptureAttribute;
                mmcCaptureAttrField.OnValueChanged += value =>
                {
                    modifiers[index].mmcCaptureAttribute = value;
                    node.SyncUIFromData();
                };
                mmcDetailContainer.Add(mmcCaptureAttrField);

                // MMC 属性来源
                var mmcSourceField = new EnumField("属性来源", modifier.mmcAttributeSource);
                mmcSourceField.style.marginBottom = 4;
                ApplyEnumFieldStyle(mmcSourceField);
                mmcSourceField.RegisterValueChangedCallback(evt =>
                {
                    modifiers[index].mmcAttributeSource = (MMCAttributeSource)evt.newValue;
                    node.SyncUIFromData();
                });
                mmcDetailContainer.Add(mmcSourceField);

                // MMC 系数
                var mmcCoefficientField = new FloatField("系数") { value = modifier.mmcCoefficient };
                mmcCoefficientField.style.marginBottom = 4;
                mmcCoefficientField.RegisterValueChangedCallback(evt =>
                {
                    modifiers[index].mmcCoefficient = evt.newValue;
                    node.SyncUIFromData();
                });
                mmcDetailContainer.Add(mmcCoefficientField);

                // MMC 快照模式
                var mmcSnapshotToggle = new Toggle("使用快照（施放时捕获）") { value = modifier.mmcUseSnapshot };
                mmcSnapshotToggle.tooltip = "勾选：施放时捕获属性值，后续不变\n不勾选：每次计算时实时读取属性值";
                mmcSnapshotToggle.RegisterValueChangedCallback(evt =>
                {
                    modifiers[index].mmcUseSnapshot = evt.newValue;
                    node.SyncUIFromData();
                });
                mmcDetailContainer.Add(mmcSnapshotToggle);

                modifierContainer.Add(mmcDetailContainer);

                // 数值来源类型切换事件
                sourceTypeField.RegisterValueChangedCallback(evt =>
                {
                    var newType = (ModifierMagnitudeSourceType)evt.newValue;
                    modifiers[index].magnitudeSourceType = newType;

                    fixedValueField.style.display = newType == ModifierMagnitudeSourceType.FixedValue
                        ? DisplayStyle.Flex : DisplayStyle.None;
                    formulaField.style.display = newType == ModifierMagnitudeSourceType.Formula
                        ? DisplayStyle.Flex : DisplayStyle.None;
                    mmcTypeField.style.display = newType == ModifierMagnitudeSourceType.ModifierMagnitudeCalculation
                        ? DisplayStyle.Flex : DisplayStyle.None;
                    setByCallerField.style.display = newType == ModifierMagnitudeSourceType.SetByCaller
                        ? DisplayStyle.Flex : DisplayStyle.None;

                    // 更新 MMC 详细配置显示
                    mmcDetailContainer.style.display = (newType == ModifierMagnitudeSourceType.ModifierMagnitudeCalculation
                        && modifiers[index].mmcType == MMCType.AttributeBased) ? DisplayStyle.Flex : DisplayStyle.None;

                    node.SyncUIFromData();
                });

                // MMC 类型切换时更新详细配置显示
                mmcTypeField.RegisterValueChangedCallback(evt =>
                {
                    modifiers[index].mmcType = (MMCType)evt.newValue;
                    mmcDetailContainer.style.display = (modifiers[index].magnitudeSourceType == ModifierMagnitudeSourceType.ModifierMagnitudeCalculation
                        && (MMCType)evt.newValue == MMCType.AttributeBased) ? DisplayStyle.Flex : DisplayStyle.None;
                    node.SyncUIFromData();
                });

                var removeBtn = new Button { text = "删除" };
                removeBtn.style.marginTop = 4;
                removeBtn.clicked += () =>
                {
                    modifiers.RemoveAt(index);
                    RefreshModifiersList(content, node, modifiers, addButtonText);
                    node.SyncUIFromData();
                };
                modifierContainer.Add(removeBtn);

                content.Add(modifierContainer);
            }

            var addBtn = new Button { text = addButtonText };
            addBtn.style.marginTop = 4;
            addBtn.clicked += () =>
            {
                modifiers.Add(new AttributeModifierData());
                RefreshModifiersList(content, node, modifiers, addButtonText);
                node.SyncUIFromData();
            };
            content.Add(addBtn);
        }

        #endregion

        #region 堆叠配置

        /// <summary>
        /// 创建堆叠配置区域 - 完整版（包含所有GAS堆叠策略）
        /// </summary>
        protected VisualElement CreateStackConfigSection(SkillNodeBase node, EffectNodeData data)
        {
            var section = CreateCollapsibleSection("堆叠配置", out var content);

            // 堆叠类型
            var stackTypeField = new EnumField("堆叠类型", data.stackType) { style = { marginBottom = 8 } };
            ApplyEnumFieldStyle(stackTypeField);
            content.Add(stackTypeField);

            var stackParamsContainer = new VisualElement();
            content.Add(stackParamsContainer);

            void UpdateStackParams(StackType stackType)
            {
                stackParamsContainer.Clear();
                if (stackType == StackType.AggregateByTarget || stackType == StackType.AggregateBySource)
                {
                    // 堆叠上限
                    var stackLimitField = new IntegerField("堆叠上限(0=无限)") { value = data.stackLimit };
                    stackLimitField.style.marginBottom = 8;
                    stackLimitField.RegisterValueChangedCallback(evt =>
                    {
                        data.stackLimit = evt.newValue;
                        node.SyncUIFromData();
                    });
                    stackParamsContainer.Add(stackLimitField);

                    // === 持续时间刷新策略 ===
                    var durationRefreshField = new EnumField("持续时间刷新策略", data.stackDurationRefreshPolicy);
                    durationRefreshField.style.marginBottom = 4;
                    ApplyEnumFieldStyle(durationRefreshField);
                    durationRefreshField.RegisterValueChangedCallback(evt =>
                    {
                        data.stackDurationRefreshPolicy = (StackDurationRefreshPolicy)evt.newValue;
                        node.SyncUIFromData();
                    });
                    stackParamsContainer.Add(durationRefreshField);

                    // === 周期重置策略 ===
                    var periodResetField = new EnumField("周期重置策略", data.stackPeriodResetPolicy);
                    periodResetField.style.marginBottom = 4;
                    ApplyEnumFieldStyle(periodResetField);
                    periodResetField.RegisterValueChangedCallback(evt =>
                    {
                        data.stackPeriodResetPolicy = (StackPeriodResetPolicy)evt.newValue;
                        // 同步旧字段
                        node.SyncUIFromData();
                    });
                    stackParamsContainer.Add(periodResetField);

                    // === 堆叠过期策略 ===
                    var expirationPolicyField = new EnumField("过期策略", data.stackExpirationPolicy);
                    expirationPolicyField.style.marginBottom = 4;
                    ApplyEnumFieldStyle(expirationPolicyField);
                    expirationPolicyField.RegisterValueChangedCallback(evt =>
                    {
                        data.stackExpirationPolicy = (StackExpirationPolicy)evt.newValue;
                        node.SyncUIFromData();
                    });
                    stackParamsContainer.Add(expirationPolicyField);

                    // === 堆叠溢出策略 ===
                    var overflowPolicyField = new EnumField("溢出策略", data.stackOverflowPolicy);
                    overflowPolicyField.style.marginBottom = 4;
                    ApplyEnumFieldStyle(overflowPolicyField);
                    overflowPolicyField.RegisterValueChangedCallback(evt =>
                    {
                        data.stackOverflowPolicy = (StackOverflowPolicy)evt.newValue;
                        node.SyncUIFromData();
                    });
                    stackParamsContainer.Add(overflowPolicyField);

                    // 溢出策略说明
                    var overflowHint = new Label("溢出策略说明：允许溢出效果时，可通过\"溢出\"端口触发额外效果");
                    overflowHint.style.fontSize = 10;
                    overflowHint.style.color = new Color(0.6f, 0.6f, 0.6f);
                    overflowHint.style.marginTop = 4;
                    overflowHint.style.whiteSpace = WhiteSpace.Normal;
                    stackParamsContainer.Add(overflowHint);
                }
            }

            stackTypeField.RegisterValueChangedCallback(evt =>
            {
                data.stackType = (StackType)evt.newValue;
                UpdateStackParams((StackType)evt.newValue);
                node.SyncUIFromData();
            });

            UpdateStackParams(data.stackType);

            return section;
        }

        #endregion

        #region 数值来源四选项 UI

        /// <summary>
        /// 创建数值来源四选项 UI（具体值/公式/MMC/上下文数据）- 紧凑布局
        /// </summary>
        protected VisualElement CreateMagnitudeSourceUI(
            string label,
            ModifierMagnitudeSourceType sourceType,
            float fixedValue,
            string formula,
            MMCType mmcType,
            string setByCallerKey,
            System.Action<ModifierMagnitudeSourceType> onSourceTypeChanged,
            System.Action<float> onFixedValueChanged,
            System.Action<string> onFormulaChanged,
            System.Action<MMCType> onMMCTypeChanged,
            System.Action<string> onSetByCallerKeyChanged)
        {
            var container = new VisualElement();
            container.style.marginBottom = 8;

            // 标签行
            var labelElement = new Label(label);
            labelElement.style.marginBottom = 4;
            container.Add(labelElement);

            // 数值行：来源类型下拉框 + 输入框
            var valueRow = new VisualElement();
            valueRow.style.flexDirection = FlexDirection.Row;

            // 数值来源类型下拉框
            var sourceTypeField = new EnumField(sourceType);
            sourceTypeField.style.width = 100;
            sourceTypeField.style.marginRight = 4;
            ApplyEnumFieldStyle(sourceTypeField);
            valueRow.Add(sourceTypeField);

            // ===== 具体值输入框 =====
            var fixedValueField = new FloatField { value = fixedValue };
            fixedValueField.style.flexGrow = 1;
            fixedValueField.style.display = sourceType == ModifierMagnitudeSourceType.FixedValue
                ? DisplayStyle.Flex : DisplayStyle.None;
            fixedValueField.RegisterValueChangedCallback(evt =>
            {
                onFixedValueChanged?.Invoke(evt.newValue);
            });
            valueRow.Add(fixedValueField);

            // ===== 公式输入框 =====
            var formulaField = new TextField { value = formula ?? "" };
            formulaField.style.flexGrow = 1;
            formulaField.style.display = sourceType == ModifierMagnitudeSourceType.Formula
                ? DisplayStyle.Flex : DisplayStyle.None;
            formulaField.RegisterValueChangedCallback(evt =>
            {
                onFormulaChanged?.Invoke(evt.newValue);
            });
            valueRow.Add(formulaField);

            // ===== MMC 类型枚举选择 =====
            var mmcTypeField = new EnumField(mmcType);
            mmcTypeField.style.flexGrow = 1;
            mmcTypeField.style.display = sourceType == ModifierMagnitudeSourceType.ModifierMagnitudeCalculation
                ? DisplayStyle.Flex : DisplayStyle.None;
            ApplyEnumFieldStyle(mmcTypeField);
            mmcTypeField.RegisterValueChangedCallback(evt =>
            {
                onMMCTypeChanged?.Invoke((MMCType)evt.newValue);
            });
            valueRow.Add(mmcTypeField);

            // ===== 上下文数据键名输入框 =====
            var setByCallerField = new TextField { value = setByCallerKey ?? "" };
            setByCallerField.style.flexGrow = 1;
            setByCallerField.style.display = sourceType == ModifierMagnitudeSourceType.SetByCaller
                ? DisplayStyle.Flex : DisplayStyle.None;
            setByCallerField.RegisterValueChangedCallback(evt =>
            {
                onSetByCallerKeyChanged?.Invoke(evt.newValue);
            });
            valueRow.Add(setByCallerField);

            container.Add(valueRow);

            // 数值来源类型切换事件
            sourceTypeField.RegisterValueChangedCallback(evt =>
            {
                var newType = (ModifierMagnitudeSourceType)evt.newValue;
                onSourceTypeChanged?.Invoke(newType);

                fixedValueField.style.display = newType == ModifierMagnitudeSourceType.FixedValue
                    ? DisplayStyle.Flex : DisplayStyle.None;
                formulaField.style.display = newType == ModifierMagnitudeSourceType.Formula
                    ? DisplayStyle.Flex : DisplayStyle.None;
                mmcTypeField.style.display = newType == ModifierMagnitudeSourceType.ModifierMagnitudeCalculation
                    ? DisplayStyle.Flex : DisplayStyle.None;
                setByCallerField.style.display = newType == ModifierMagnitudeSourceType.SetByCaller
                    ? DisplayStyle.Flex : DisplayStyle.None;
            });

            return container;
        }

        #endregion

        #region 标签配置

        /// <summary>
        /// 创建效果标签配置区域
        /// </summary>
        protected VisualElement CreateEffectTagsSection(SkillNodeBase node, EffectNodeData data)
        {
            var section = CreateCollapsibleSection("标签配置", out var content, EffectTagsDefaultExpanded);

            content.Add(CreateTagSetField("自身标签", data.assetTags,
                value => { data.assetTags = value; node.SyncUIFromData(); }));

            content.Add(CreateTagSetField("激活时授予标签", data.grantedTags,
                value => { data.grantedTags = value; node.SyncUIFromData(); }));

            content.Add(CreateTagSetField("应用所需标签", data.applicationRequiredTags,
                value => { data.applicationRequiredTags = value; node.SyncUIFromData(); }));

            content.Add(CreateTagSetField("应用阻止标签", data.applicationImmunityTags,
                value => { data.applicationImmunityTags = value; node.SyncUIFromData(); }));

            // 持续所需标签现在在基类 EffectNodeData 中
            content.Add(CreateTagSetField("持续所需标签", data.ongoingRequiredTags,
                value => { data.ongoingRequiredTags = value; node.SyncUIFromData(); }));

            content.Add(CreateTagSetField("移除带有标签的效果", data.removeGameEffectsWithTags,
                value => { data.removeGameEffectsWithTags = value; node.SyncUIFromData(); }));

            return section;
        }

        #endregion

        #region 移除策略

        /// <summary>
        /// 创建移除策略配置区域
        /// </summary>
        protected VisualElement CreateRemovalPolicySection(SkillNodeBase node, EffectNodeData data)
        {
            var container = new VisualElement
            {
                style =
                {
                    backgroundColor = new Color(50f / 255f, 50f / 255f, 50f / 255f),
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,
                    paddingLeft = 8,
                    paddingRight = 8,
                    paddingTop = 8,
                    paddingBottom = 8,
                    marginTop = 8
                }
            };

            var toggle = new Toggle("技能结束时取消") { value = data.cancelOnAbilityEnd };
            toggle.tooltip = "勾选：技能结束时取消此Effect\n不勾选：Effect独立存在，由ASC管理生命周期（如CD、Buff）";
            toggle.RegisterValueChangedCallback(evt =>
            {
                data.cancelOnAbilityEnd = evt.newValue;
                node.SyncUIFromData();
            });
            container.Add(toggle);

            return container;
        }

        #endregion
    }
}
