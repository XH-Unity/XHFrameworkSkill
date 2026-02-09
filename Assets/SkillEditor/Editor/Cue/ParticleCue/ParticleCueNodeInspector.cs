using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;

using SkillEditor.Data;
namespace SkillEditor.Editor
{
    public class ParticleCueNodeInspector : CueNodeInspector
    {
        protected override void BuildCueInspectorUI(VisualElement container, SkillNodeBase node)
        {
            if (node is ParticleCueNode particleCueNode)
            {
                var data = particleCueNode.TypedData;
                if (data == null) return;

                // ============ 位置设置 ============
                var positionSection = CreateCollapsibleSection("位置设置", out var positionContent, true);

                // 位置来源
                var positionSourceField = new EnumField("位置来源", data.positionSource);
                ApplyEnumFieldStyle(positionSourceField);
                positionSourceField.RegisterValueChangedCallback(evt =>
                {
                    data.positionSource = (PositionSourceType)evt.newValue;
                    particleCueNode.SyncUIFromData();
                });
                positionContent.Add(positionSourceField);

                // 挂点
                positionContent.Add(CreateTextField("挂点", data.particleBindingName ?? "", value =>
                {
                    data.particleBindingName = value;
                    particleCueNode.SyncUIFromData();
                }));

                container.Add(positionSection);

                // ============ 粒子设置 ============
                var particleSection = CreateCollapsibleSection("粒子设置", out var particleContent, true);

                // 粒子预制体（拖拽）
                var prefabField = new ObjectField("粒子预制体") { objectType = typeof(GameObject) };
                prefabField.value = data.particlePrefab;
                prefabField.RegisterValueChangedCallback(evt =>
                {
                    data.particlePrefab = evt.newValue as GameObject;
                    particleCueNode.SyncUIFromData();
                    particleCueNode.NotifyConnectedTracksUpdateDuration();
                });
                particleContent.Add(prefabField);

                // 偏移
                var offsetContainer = new VisualElement { style = { marginBottom = 8 } };
                offsetContainer.Add(new Label("偏移") { style = { marginBottom = 4 } });
                var offsetXField = CreateFloatField("X", data.particleOffset.x, value =>
                {
                    data.particleOffset = new Vector3(value, data.particleOffset.y, data.particleOffset.z);
                    particleCueNode.SyncUIFromData();
                });
                var offsetYField = CreateFloatField("Y", data.particleOffset.y, value =>
                {
                    data.particleOffset = new Vector3(data.particleOffset.x, value, data.particleOffset.z);
                    particleCueNode.SyncUIFromData();
                });
                var offsetZField = CreateFloatField("Z", data.particleOffset.z, value =>
                {
                    data.particleOffset = new Vector3(data.particleOffset.x, data.particleOffset.y, value);
                    particleCueNode.SyncUIFromData();
                });
                offsetContainer.Add(offsetXField);
                offsetContainer.Add(offsetYField);
                offsetContainer.Add(offsetZField);
                particleContent.Add(offsetContainer);

                // 缩放 - Vector3
                var scaleContainer = new VisualElement { style = { marginBottom = 8 } };
                scaleContainer.Add(new Label("缩放") { style = { marginBottom = 4 } });
                var scaleXField = CreateFloatField("X", data.particleScale.x, value =>
                {
                    data.particleScale = new Vector3(value, data.particleScale.y, data.particleScale.z);
                    particleCueNode.SyncUIFromData();
                });
                var scaleYField = CreateFloatField("Y", data.particleScale.y, value =>
                {
                    data.particleScale = new Vector3(data.particleScale.x, value, data.particleScale.z);
                    particleCueNode.SyncUIFromData();
                });
                var scaleZField = CreateFloatField("Z", data.particleScale.z, value =>
                {
                    data.particleScale = new Vector3(data.particleScale.x, data.particleScale.y, value);
                    particleCueNode.SyncUIFromData();
                });
                scaleContainer.Add(scaleXField);
                scaleContainer.Add(scaleYField);
                scaleContainer.Add(scaleZField);
                particleContent.Add(scaleContainer);

                var attachToggle = new Toggle("附着目标") { value = data.attachToTarget };
                attachToggle.style.marginBottom = 4;
                attachToggle.RegisterValueChangedCallback(evt =>
                {
                    data.attachToTarget = evt.newValue;
                    particleCueNode.SyncUIFromData();
                });
                particleContent.Add(attachToggle);

                var loopingToggle = new Toggle("循环播放") { value = data.particleLoop };
                loopingToggle.style.marginBottom = 4;
                loopingToggle.RegisterValueChangedCallback(evt =>
                {
                    data.particleLoop = evt.newValue;
                    particleCueNode.SyncUIFromData();
                    particleCueNode.NotifyConnectedTracksUpdateDuration();
                });
                particleContent.Add(loopingToggle);

                container.Add(particleSection);
            }
        }
    }
}
