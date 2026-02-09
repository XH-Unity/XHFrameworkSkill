using UnityEngine;
using UnityEngine.UIElements;

using SkillEditor.Data;
namespace SkillEditor.Editor
{
    /// <summary>
    /// 表现节点Inspector基类 - 所有Cue节点的Inspector继承此类
    /// </summary>
    public abstract class CueNodeInspector : NodeInspectorBase
    {
        /// <summary>
        /// 是否显示标签配置区域
        /// </summary>
        protected virtual bool ShowCueTags => true;

        /// <summary>
        /// 标签配置区域是否默认展开
        /// </summary>
        protected virtual bool CueTagsDefaultExpanded => false;

        protected sealed override void BuildInspectorUI(VisualElement container, SkillNodeBase node)
        {
            var cueData = GetCueNodeData(node);
            if (cueData == null) return;

            // 绘制子类的具体内容
            BuildCueInspectorUI(container, node);

            // 绘制基类通用属性
            BuildBaseCueUI(container, node, cueData);

            // 绘制标签配置
            if (ShowCueTags)
            {
                container.Add(CreateCueTagsSection(node, cueData));
            }
        }

        /// <summary>
        /// 绘制基类通用的Cue属性
        /// </summary>
        private void BuildBaseCueUI(VisualElement container, SkillNodeBase node, CueNodeData data)
        {
            // 随节点销毁
            var destroyWithNodeToggle = new Toggle("随节点销毁") { value = data.destroyWithNode };
            destroyWithNodeToggle.style.marginBottom = 4;
            destroyWithNodeToggle.RegisterValueChangedCallback(evt =>
            {
                data.destroyWithNode = evt.newValue;
                node.SyncUIFromData();
            });
            container.Add(destroyWithNodeToggle);
        }

        /// <summary>
        /// 子类实现具体的Cue Inspector UI绘制
        /// </summary>
        protected abstract void BuildCueInspectorUI(VisualElement container, SkillNodeBase node);

        /// <summary>
        /// 获取CueNodeData
        /// </summary>
        protected virtual CueNodeData GetCueNodeData(SkillNodeBase node)
        {
            return node?.NodeData as CueNodeData;
        }

        /// <summary>
        /// 创建表现标签配置区域
        /// </summary>
        protected VisualElement CreateCueTagsSection(SkillNodeBase node, CueNodeData data)
        {
            var section = CreateCollapsibleSection("标签配置", out var content, CueTagsDefaultExpanded);

            content.Add(CreateTagSetField("播放所需标签", data.requiredTags,
                value => { data.requiredTags = value; node.SyncUIFromData(); }));

            content.Add(CreateTagSetField("播放阻止标签", data.immunityTags,
                value => { data.immunityTags = value; node.SyncUIFromData(); }));

            return section;
        }
    }
}
