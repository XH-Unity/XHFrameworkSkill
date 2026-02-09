using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System;
using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 属性选择器控件 - 用于在编辑器中选择属性类型
    /// 使用枚举下拉框选择 AttrType
    /// </summary>
    public class AttributeField : VisualElement
    {
        private Label labelElement;
        private EnumField enumField;
        private AttrType currentValue;

        public event Action<AttrType> OnValueChanged;

        public AttrType Value
        {
            get => currentValue;
            set
            {
                currentValue = value;
                enumField?.SetValueWithoutNotify(value);
            }
        }

        public AttributeField(string label = "属性")
        {
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            style.marginBottom = 4;

            // 标签
            labelElement = new Label(label)
            {
                style =
                {
                    width = 100,
                    minWidth = 100
                }
            };
            Add(labelElement);

            // 枚举下拉框
            enumField = new EnumField(AttrType.None)
            {
                style =
                {
                    flexGrow = 1
                }
            };
            enumField.RegisterValueChangedCallback(evt =>
            {
                currentValue = (AttrType)evt.newValue;
                OnValueChanged?.Invoke(currentValue);
            });
            Add(enumField);
        }

        public void SetLabel(string label)
        {
            labelElement.text = label;
        }
    }

    /// <summary>
    /// 属性修改器编辑器控件 - 用于编辑 AttributeModifierData
    /// </summary>
    public class AttributeModifierField : VisualElement
    {
        private AttributeModifierData data;
        private AttributeField attributeField;
        private EnumField operationField;
        private VisualElement valueContainer;
        private EnumField sourceTypeField;
        private FloatField fixedValueField;
        private TextField formulaField;
        private EnumField mmcTypeField;
        private TextField setByCallerField;

        public event Action OnDataChanged;

        public AttributeModifierField(AttributeModifierData modifierData)
        {
            data = modifierData;

            style.backgroundColor = new Color(45f / 255f, 45f / 255f, 45f / 255f);
            style.borderTopLeftRadius = 4;
            style.borderTopRightRadius = 4;
            style.borderBottomLeftRadius = 4;
            style.borderBottomRightRadius = 4;
            style.paddingLeft = 6;
            style.paddingRight = 6;
            style.paddingTop = 6;
            style.paddingBottom = 6;
            style.marginBottom = 4;

            // 属性选择
            attributeField = new AttributeField("目标属性");
            attributeField.Value = data.attrType;
            attributeField.OnValueChanged += value =>
            {
                data.attrType = value;
                OnDataChanged?.Invoke();
            };
            Add(attributeField);

            // 操作类型
            operationField = new EnumField("操作", data.operation);
            operationField.style.marginBottom = 4;
            operationField.RegisterValueChangedCallback(evt =>
            {
                data.operation = (ModifierOperation)evt.newValue;
                OnDataChanged?.Invoke();
            });
            Add(operationField);

            // 数值行 - 输入框 + 来源类型下拉框在同一行
            valueContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginBottom = 4
                }
            };

            valueContainer.Add(new Label("数值")
            {
                style = { width = 100, minWidth = 100 }
            });

            // 固定值输入框
            fixedValueField = new FloatField
            {
                value = data.fixedValue,
                style = { flexGrow = 1 }
            };
            fixedValueField.RegisterValueChangedCallback(evt =>
            {
                data.fixedValue = evt.newValue;
                OnDataChanged?.Invoke();
            });

            // 公式输入框
            formulaField = new TextField
            {
                value = data.formula ?? "",
                style = { flexGrow = 1 }
            };
            formulaField.RegisterValueChangedCallback(evt =>
            {
                data.formula = evt.newValue;
                OnDataChanged?.Invoke();
            });

            // MMC 类型下拉框
            mmcTypeField = new EnumField(data.mmcType)
            {
                style = { flexGrow = 1 }
            };
            mmcTypeField.RegisterValueChangedCallback(evt =>
            {
                data.mmcType = (MMCType)evt.newValue;
                OnDataChanged?.Invoke();
            });

            // SetByCaller 键名输入框
            setByCallerField = new TextField
            {
                value = data.setByCallerKey ?? "",
                style = { flexGrow = 1 }
            };
            setByCallerField.RegisterValueChangedCallback(evt =>
            {
                data.setByCallerKey = evt.newValue;
                OnDataChanged?.Invoke();
            });

            // 来源类型下拉框
            sourceTypeField = new EnumField(data.magnitudeSourceType)
            {
                style = { width = 120, marginLeft = 4 }
            };
            sourceTypeField.RegisterValueChangedCallback(evt =>
            {
                data.magnitudeSourceType = (ModifierMagnitudeSourceType)evt.newValue;
                RefreshValueInput();
                OnDataChanged?.Invoke();
            });

            Add(valueContainer);
            RefreshValueInput();
        }

        private void RefreshValueInput()
        {
            // 清除当前输入控件（保留标签）
            while (valueContainer.childCount > 1)
            {
                valueContainer.RemoveAt(1);
            }

            // 根据来源类型添加对应的输入控件
            switch (data.magnitudeSourceType)
            {
                case ModifierMagnitudeSourceType.FixedValue:
                    fixedValueField.SetValueWithoutNotify(data.fixedValue);
                    valueContainer.Add(fixedValueField);
                    break;
                case ModifierMagnitudeSourceType.Formula:
                    formulaField.SetValueWithoutNotify(data.formula ?? "");
                    valueContainer.Add(formulaField);
                    break;
                case ModifierMagnitudeSourceType.ModifierMagnitudeCalculation:
                    mmcTypeField.SetValueWithoutNotify(data.mmcType);
                    valueContainer.Add(mmcTypeField);
                    break;
                case ModifierMagnitudeSourceType.SetByCaller:
                    setByCallerField.SetValueWithoutNotify(data.setByCallerKey ?? "");
                    valueContainer.Add(setByCallerField);
                    break;
            }

            // 添加来源类型下拉框
            valueContainer.Add(sourceTypeField);
        }

        public void Refresh()
        {
            attributeField.Value = data.attrType;
            operationField.SetValueWithoutNotify(data.operation);
            sourceTypeField.SetValueWithoutNotify(data.magnitudeSourceType);
            RefreshValueInput();
        }
    }

    /// <summary>
    /// 属性修改器列表编辑器控件
    /// </summary>
    public class AttributeModifierListField : VisualElement
    {
        private List<AttributeModifierData> dataList;
        private VisualElement listContainer;

        public event Action OnDataChanged;

        public AttributeModifierListField(List<AttributeModifierData> modifiers, string title = "属性修改器")
        {
            dataList = modifiers;

            // 标题栏
            var header = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.SpaceBetween,
                    alignItems = Align.Center,
                    marginBottom = 4
                }
            };

            header.Add(new Label(title)
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold }
            });

            var addButton = new Button(AddModifier)
            {
                text = "+",
                style = { width = 24 }
            };
            header.Add(addButton);

            Add(header);

            // 列表容器
            listContainer = new VisualElement();
            Add(listContainer);

            RefreshList();
        }

        private void RefreshList()
        {
            listContainer.Clear();

            for (int i = 0; i < dataList.Count; i++)
            {
                int index = i;
                var modifierData = dataList[index];

                var itemContainer = new VisualElement();

                var modifierField = new AttributeModifierField(modifierData);
                modifierField.OnDataChanged += () => OnDataChanged?.Invoke();
                itemContainer.Add(modifierField);

                // 删除按钮
                var deleteButton = new Button(() =>
                {
                    dataList.RemoveAt(index);
                    RefreshList();
                    OnDataChanged?.Invoke();
                })
                {
                    text = "删除",
                    style = { marginBottom = 8 }
                };
                itemContainer.Add(deleteButton);

                listContainer.Add(itemContainer);
            }

            if (dataList.Count == 0)
            {
                listContainer.Add(new Label("暂无修改器，点击 + 添加")
                {
                    style = { color = new Color(0.5f, 0.5f, 0.5f) }
                });
            }
        }

        private void AddModifier()
        {
            dataList.Add(new AttributeModifierData());
            RefreshList();
            OnDataChanged?.Invoke();
        }
    }
}
