using UnityEngine.UIElements;
using UnityEngine;

using SkillEditor.Data;
using UnityEditor.UIElements;

namespace SkillEditor.Editor
{
    public class FloatingTextCueNodeInspector : CueNodeInspector
    {
        protected override void BuildCueInspectorUI(VisualElement container, SkillNodeBase node)
        {
            if (node is FloatingTextCueNode floatingTextNode)
            {
                var data = floatingTextNode.TypedData;
                if (data == null) return;

                // ============ 位置设置 ============
                var positionSection = CreateCollapsibleSection("位置设置", out var positionContent, true);

                // 位置来源
                var positionSourceField = new EnumField("位置来源", data.positionSource);
                ApplyEnumFieldStyle(positionSourceField);
                positionSourceField.RegisterValueChangedCallback(evt =>
                {
                    data.positionSource = (PositionSourceType)evt.newValue;
                    floatingTextNode.SyncUIFromData();
                });
                positionContent.Add(positionSourceField);

                // 挂点
                positionContent.Add(CreateTextField("挂点", data.positionBindingName ?? "", value =>
                {
                    data.positionBindingName = value;
                    floatingTextNode.SyncUIFromData();
                }));

                container.Add(positionSection);

                // ============ 飘字设置 ============
                var textSection = CreateCollapsibleSection("飘字设置", out var textContent, true);

                // 飘字类型
                var textTypeField = new EnumField("飘字类型", data.textType);
                ApplyEnumFieldStyle(textTypeField);
                textTypeField.RegisterValueChangedCallback(evt =>
                {
                    data.textType = (FloatingTextType)evt.newValue;
                    floatingTextNode.SyncUIFromData();
                });
                textContent.Add(textTypeField);

                // 上下文数据Key
                textContent.Add(CreateTextField("数据Key", data.contextDataKey ?? "Damage", value =>
                {
                    data.contextDataKey = value;
                    floatingTextNode.SyncUIFromData();
                }));

                // 固定文本
                textContent.Add(CreateTextField("固定文本", data.fixedText ?? "", value =>
                {
                    data.fixedText = value;
                    floatingTextNode.SyncUIFromData();
                }));

                // 颜色
                var colorField = new ColorField("颜色") { value = data.textColor };
                colorField.style.marginBottom = 4;
                colorField.RegisterValueChangedCallback(evt =>
                {
                    data.textColor = evt.newValue;
                    floatingTextNode.SyncUIFromData();
                });
                textContent.Add(colorField);

                // 字体大小
                textContent.Add(CreateFloatField("字体大小", data.fontSize, value =>
                {
                    data.fontSize = value;
                    floatingTextNode.SyncUIFromData();
                }));

                // 持续时间
                textContent.Add(CreateFloatField("持续时间", data.duration, value =>
                {
                    data.duration = value;
                    floatingTextNode.SyncUIFromData();
                }));

                // 偏移
                var offsetContainer = new VisualElement { style = { marginBottom = 8 } };
                offsetContainer.Add(new Label("偏移") { style = { marginBottom = 4 } });
                offsetContainer.Add(CreateFloatField("X", data.offset.x, value =>
                {
                    data.offset = new Vector2(value, data.offset.y);
                    floatingTextNode.SyncUIFromData();
                }));
                offsetContainer.Add(CreateFloatField("Y", data.offset.y, value =>
                {
                    data.offset = new Vector2(data.offset.x, value);
                    floatingTextNode.SyncUIFromData();
                }));
                textContent.Add(offsetContainer);

                // 移动方向
                var moveContainer = new VisualElement { style = { marginBottom = 8 } };
                moveContainer.Add(new Label("移动方向") { style = { marginBottom = 4 } });
                moveContainer.Add(CreateFloatField("X", data.moveDirection.x, value =>
                {
                    data.moveDirection = new Vector2(value, data.moveDirection.y);
                    floatingTextNode.SyncUIFromData();
                }));
                moveContainer.Add(CreateFloatField("Y", data.moveDirection.y, value =>
                {
                    data.moveDirection = new Vector2(data.moveDirection.x, value);
                    floatingTextNode.SyncUIFromData();
                }));
                textContent.Add(moveContainer);

                container.Add(textSection);
            }
        }
    }
}
