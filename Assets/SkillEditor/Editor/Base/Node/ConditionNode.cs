using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 条件节点统一基类 - 所有Condition节点的基类
    /// 条件节点特点：瞬时执行、返回布尔结果、有"是/否"两个输出端口
    /// </summary>
    public abstract class ConditionNode<TData> : SkillNodeBase<TData> where TData : ConditionNodeData, new()
    {
        protected Port truePort;
        protected Port falsePort;

        protected ConditionNode(NodeType nodeType, Vector2 position) : base(nodeType, position)
        {
        }

        /// <summary>
        /// 条件节点默认有输入端口
        /// </summary>
        protected override bool HasDefaultInputPort => true;

        /// <summary>
        /// 条件节点的分类颜色 - 黄色
        /// </summary>
        protected override Color GetNodeCategoryColor() => new Color(0.9f, 0.7f, 0.2f);

        /// <summary>
        /// 创建默认端口，包括"是"和"否"两个输出端口
        /// </summary>
        protected override void CreateDefaultPorts()
        {
            base.CreateDefaultPorts();
            // 创建"是"和"否"两个输出端口，实现分支逻辑
            truePort = CreateOutputPort("是");
            falsePort = CreateOutputPort("否");
        }

        /// <summary>
        /// 基类绘制逻辑
        /// </summary>
        protected override void CreateContent()
        {
            // 让子类绘制自己的内容
            CreateConditionContent();
        }

        /// <summary>
        /// 子类实现具体的节点内容绘制
        /// </summary>
        protected virtual void CreateConditionContent() { }

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

        #region 数据同步

        public override void LoadData(NodeData data)
        {
            base.LoadData(data);
            SyncConditionUIFromData();
        }

        public override void SyncUIFromData()
        {
            base.SyncUIFromData();
            SyncConditionUIFromData();
        }

        private void SyncConditionUIFromData()
        {
            if (TypedData == null) return;
            SyncConditionContentFromData();
        }

        /// <summary>
        /// 子类实现自己的数据同步
        /// </summary>
        protected virtual void SyncConditionContentFromData() { }

        #endregion
    }
}
