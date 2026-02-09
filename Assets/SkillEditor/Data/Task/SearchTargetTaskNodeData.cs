using System;
using UnityEngine.Scripting.APIUpdating;

namespace SkillEditor.Data
{
    /// <summary>
    /// 搜索目标任务节点数据
    /// 用于在指定范围内搜索目标，并对每个目标执行后续节点
    /// </summary>
    [Serializable]
    public class SearchTargetTaskNodeData : TaskNodeData
    {
        public SearchShapeType searchShapeType = SearchShapeType.Circle;
        public LineType searchLineType = LineType.UnitDirection;

        /// <summary>
        /// 搜索中心位置来源
        /// </summary>
        public PositionSourceType positionSource = PositionSourceType.Caster;

        /// <summary>
        /// 检测中心点挂点名称，空则使用对象位置
        /// </summary>
        public string positionBindingName;

        // 圆形参数
        public float searchCircleRadius = 5f;

        // 扇形参数
        public float searchSectorRadius = 5f;
        public float searchSectorAngle = 90f;

        // 直线参数 - 单位朝向
        public float searchLineDirectionOffsetAngle = 0f;
        public float searchLineDirectionWidth = 1f;
        public float searchLineDirectionLength = 10f;

        // 直线参数 - 两点之间
        public PositionSourceType lineStartPositionSource = PositionSourceType.Caster;
        public string lineStartBindingName;
        public PositionSourceType lineEndPositionSource = PositionSourceType.MainTarget;
        public string lineEndBindingName;
        public float searchLineBetweenWidth = 1f;

        // 直线参数 - 绝对角度
        public float searchLineAbsoluteAngle = 0f;
        public float searchLineAbsoluteWidth = 1f;
        public float searchLineAbsoluteLength = 10f;

        public int maxTargets = 0;  // 最大目标数，0表示无限

        /// <summary>
        /// 搜索目标标签
        /// </summary>
        public GameplayTagSet searchTargetTags;

        /// <summary>
        /// 排除标签
        /// </summary>
        public GameplayTagSet searchExcludeTags;
    }

    /// <summary>
    /// 搜索目标标签容器 - 用于运行时快速访问标签配置
    /// </summary>
    public struct SearchTargetTagContainer
    {
        public GameplayTagSet TargetTags;
        public GameplayTagSet ExcludeTags;

        public SearchTargetTagContainer(SearchTargetTaskNodeData data)
        {
            TargetTags = data.searchTargetTags;
            ExcludeTags = data.searchExcludeTags;
        }
    }
}
