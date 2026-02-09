using UnityEngine;
using UnityEngine.UIElements;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 任务节点统一基类 - 所有Task节点的基类
    /// 任务节点特点：瞬时执行、无属性修改、无堆叠、无持续时间
    /// </summary>
    public abstract class TaskNode<TData> : SkillNodeBase<TData> where TData : TaskNodeData, new()
    {
        protected TaskNode(NodeType nodeType, Vector2 position) : base(nodeType, position)
        {
        }

        /// <summary>
        /// 任务节点默认有输入端口
        /// </summary>
        protected override bool HasDefaultInputPort => true;

        /// <summary>
        /// 任务节点的分类颜色 - 蓝色
        /// </summary>
        protected override Color GetNodeCategoryColor() => new Color(0.2f, 0.4f, 0.8f);

        /// <summary>
        /// 基类绘制逻辑
        /// </summary>
        protected override void CreateContent()
        {
            // 让子类绘制自己的内容
            CreateTaskContent();
        }

        /// <summary>
        /// 子类实现具体的节点内容绘制
        /// </summary>
        protected virtual void CreateTaskContent() { }

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
            SyncTaskUIFromData();
        }

        public override void SyncUIFromData()
        {
            base.SyncUIFromData();
            SyncTaskUIFromData();
        }

        private void SyncTaskUIFromData()
        {
            if (TypedData == null) return;
            // 调用子类的同步
            SyncTaskContentFromData();
        }

        /// <summary>
        /// 子类实现自己的数据同步
        /// </summary>
        protected virtual void SyncTaskContentFromData() { }

        #endregion
    }
}
