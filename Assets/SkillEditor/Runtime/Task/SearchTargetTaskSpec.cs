using System.Collections.Generic;
using SkillEditor.Data;
using UnityEngine;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// 搜索目标任务Spec
    /// 使用Physics2D进行碰撞检测
    /// </summary>
    public class SearchTargetTaskSpec : TaskSpec
    {
        private List<AbilitySystemComponent> _foundTargets = new List<AbilitySystemComponent>();
        private SearchTargetTaskNodeData SearchNodeData => NodeData as SearchTargetTaskNodeData;

        /// <summary>
        /// 是否开启调试绘制
        /// </summary>
        public static bool DebugDraw = true;

        /// <summary>
        /// 调试绘制持续时间
        /// </summary>
        public static float DebugDrawDuration = 2f;

        /// <summary>
        /// 调试绘制颜色
        /// </summary>
        public static Color DebugDrawColor = Color.green;

        protected override void OnExecute(AbilitySystemComponent target)
        {
            _foundTargets.Clear();

            var nodeData = SearchNodeData;
            if (nodeData == null)
            {
                SpecExecutor.ExecuteConnectedNodes(SkillId, NodeGuid, "无目标", GetExecutionContext());
                return;
            }

            // 使用 PositionSourceType 获取检测中心位置
            Vector2 centerPosition = Context.GetPosition(nodeData.positionSource, nodeData.positionBindingName);

            // 获取位置来源对象（用于获取朝向）
            GameObject sourceObject = Context.GetSourceObject(nodeData.positionSource);
            Transform centerTransform = sourceObject?.transform;

            switch (nodeData.searchShapeType)
            {
                case SearchShapeType.Circle:
                    SearchCircle(centerPosition, nodeData.searchCircleRadius);
                    DebugDrawCircle(centerPosition, nodeData.searchCircleRadius);
                    break;
                case SearchShapeType.Sector:
                    if (centerTransform != null)
                    {
                        var sectorForward = GetFacingDirection(centerTransform);
                        SearchSector(centerPosition, centerTransform, nodeData.searchSectorRadius, nodeData.searchSectorAngle);
                        DebugDrawSector(centerPosition, sectorForward, nodeData.searchSectorRadius, nodeData.searchSectorAngle);
                    }
                    break;
                case SearchShapeType.Line:
                    SearchLine(centerPosition, centerTransform);
                    break;
            }

            if (nodeData.maxTargets > 0 && _foundTargets.Count > nodeData.maxTargets)
                _foundTargets.RemoveRange(nodeData.maxTargets, _foundTargets.Count - nodeData.maxTargets);

            var ctx = GetExecutionContext();
            if (_foundTargets.Count == 0)
            {
                SpecExecutor.ExecuteConnectedNodes(SkillId, NodeGuid, "无目标", ctx);
            }
            else
            {
                // 为每个目标创建带有 ParentInputTarget 的上下文并执行
                foreach (var findTarget in _foundTargets)
                {
                    var targetCtx = ctx.CreateWithParentInput(findTarget);
                    SpecExecutor.ExecuteConnectedNodes(SkillId, NodeGuid, "对每个目标", targetCtx);
                }
            }

            // 执行完成效果
            SpecExecutor.ExecuteConnectedNodes(SkillId, NodeGuid, "完成效果", ctx);
        }

        /// <summary>
        /// 圆形范围检测 - 使用Physics2D.OverlapCircleAll
        /// </summary>
        private void SearchCircle(Vector2 center, float radius)
        {
            var colliders = Physics2D.OverlapCircleAll(center, radius);
            foreach (var collider in colliders)
            {
                var asc = GetASCFromCollider(collider);
                if (asc != null && IsValidTarget(asc))
                {
                    _foundTargets.Add(asc);
                }
            }
        }

        /// <summary>
        /// 扇形范围检测 - 先用圆形检测，再过滤角度
        /// </summary>
        private void SearchSector(Vector2 center, Transform casterTransform, float radius, float angle)
        {
            float halfAngle = angle * 0.5f;

            // 获取角色朝向（角色默认朝左，所以使用 -transform.right）
            Vector2 forward = GetFacingDirection(casterTransform);

            var colliders = Physics2D.OverlapCircleAll(center, radius);
            foreach (var collider in colliders)
            {
                var asc = GetASCFromCollider(collider);
                if (asc == null || !IsValidTarget(asc)) continue;

                // 计算到目标的方向
                Vector2 toTarget = (Vector2)collider.transform.position - center;

                // 跳过距离为0的情况（自己或重叠的目标）
                if (toTarget.sqrMagnitude < 0.001f) continue;

                // 检查是否在扇形角度内
                float angleToTarget = Vector2.Angle(forward, toTarget);

                if (angleToTarget <= halfAngle)
                {
                    _foundTargets.Add(asc);
                }
            }
        }

        /// <summary>
        /// 直线/矩形范围检测 - 使用Physics2D.OverlapBoxAll
        /// </summary>
        private void SearchLine(Vector2 center, Transform casterTransform)
        {
            var nodeData = SearchNodeData;
            if (nodeData == null) return;

            Vector2 direction;
            float width, length;
            Vector2 startPos = center;

            // 获取角色朝向（角色默认朝左，所以使用 -transform.right）
            Vector2 baseForward = casterTransform != null ? GetFacingDirection(casterTransform) : Vector2.right;

            switch (nodeData.searchLineType)
            {
                case LineType.UnitDirection:
                    direction = RotateVector2(baseForward, nodeData.searchLineDirectionOffsetAngle);
                    width = nodeData.searchLineDirectionWidth;
                    length = nodeData.searchLineDirectionLength;
                    break;
                case LineType.BetweenPoints:
                    // 使用 PositionSourceType 获取起点和终点位置
                    startPos = Context.GetPosition(nodeData.lineStartPositionSource, nodeData.lineStartBindingName);
                    Vector2 endPos = Context.GetPosition(nodeData.lineEndPositionSource, nodeData.lineEndBindingName);
                    direction = (endPos - startPos).normalized;
                    width = nodeData.searchLineBetweenWidth;
                    length = Vector2.Distance(startPos, endPos);
                    break;
                default:
                    direction = RotateVector2(Vector2.right, nodeData.searchLineAbsoluteAngle);
                    width = nodeData.searchLineAbsoluteWidth;
                    length = nodeData.searchLineAbsoluteLength;
                    break;
            }

            // 计算Box的中心点和尺寸
            Vector2 boxCenter = startPos + direction * (length * 0.5f);
            Vector2 boxSize = new Vector2(length, width);
            float boxAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // 调试绘制
            DebugDrawBox(boxCenter, boxSize, boxAngle);

            var colliders = Physics2D.OverlapBoxAll(boxCenter, boxSize, boxAngle);
            foreach (var collider in colliders)
            {
                var asc = GetASCFromCollider(collider);
                if (asc != null && IsValidTarget(asc))
                {
                    _foundTargets.Add(asc);
                }
            }
        }

        /// <summary>
        /// 从Collider2D获取AbilitySystemComponent
        /// </summary>
        private AbilitySystemComponent GetASCFromCollider(Collider2D collider)
        {
            if (collider == null) return null;

            // 尝试从GameObject获取Unit，再获取ASC
            var unit = collider.GetComponent<Unit>();
            if (unit != null) return unit.ownerASC;

            // 尝试从父对象获取
            unit = collider.GetComponentInParent<Unit>();
            if (unit != null) return unit.ownerASC;

            return null;
        }

        /// <summary>
        /// 旋转2D向量
        /// </summary>
        private Vector2 RotateVector2(Vector2 v, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        /// <summary>
        /// 获取角色朝向（角色默认朝左）
        /// scale.x >= 0 时朝左，scale.x < 0 时朝右
        /// </summary>
        private Vector2 GetFacingDirection(Transform casterTransform)
        {
            // 角色默认朝左，所以：
            // scale.x >= 0（默认）-> 朝左 -> Vector2.left
            // scale.x < 0（翻转）-> 朝右 -> Vector2.right
            return casterTransform.localScale.x >= 0 ? Vector2.left : Vector2.right;
        }

        private bool IsValidTarget(AbilitySystemComponent target)
        {
            if (target == null) return false;
            if (target == GetTarget()) return false;
            var nodeData = SearchNodeData;
            if (nodeData == null) return false;
            if (!nodeData.searchTargetTags.IsEmpty && !target.HasAnyTags(nodeData.searchTargetTags)) return false;
            if (!nodeData.searchExcludeTags.IsEmpty && target.HasAnyTags(nodeData.searchExcludeTags)) return false;
            return true;
        }

        #region 调试绘制

        /// <summary>
        /// 绘制圆形
        /// </summary>
        private void DebugDrawCircle(Vector2 center, float radius)
        {
            if (!DebugDraw) return;

            int segments = 32;
            float angleStep = 360f / segments;

            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * angleStep * Mathf.Deg2Rad;
                float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

                Vector3 p1 = new Vector3(center.x + Mathf.Cos(angle1) * radius, center.y + Mathf.Sin(angle1) * radius, 0);
                Vector3 p2 = new Vector3(center.x + Mathf.Cos(angle2) * radius, center.y + Mathf.Sin(angle2) * radius, 0);

                Debug.DrawLine(p1, p2, DebugDrawColor, DebugDrawDuration);
            }
        }

        /// <summary>
        /// 绘制扇形
        /// </summary>
        private void DebugDrawSector(Vector2 center, Vector2 forward, float radius, float angle)
        {
            if (!DebugDraw) return;

            float halfAngle = angle * 0.5f;
            int arcSegments = Mathf.Max(8, (int)(angle / 10f));
            float angleStep = angle / arcSegments;

            // 计算起始角度（基于forward方向）
            float forwardAngle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
            float startAngle = forwardAngle - halfAngle;

            Vector3 center3D = new Vector3(center.x, center.y, 0);

            // 绘制两条边线
            Vector3 leftEdge = center3D + new Vector3(
                Mathf.Cos((forwardAngle - halfAngle) * Mathf.Deg2Rad) * radius,
                Mathf.Sin((forwardAngle - halfAngle) * Mathf.Deg2Rad) * radius, 0);
            Vector3 rightEdge = center3D + new Vector3(
                Mathf.Cos((forwardAngle + halfAngle) * Mathf.Deg2Rad) * radius,
                Mathf.Sin((forwardAngle + halfAngle) * Mathf.Deg2Rad) * radius, 0);

            Debug.DrawLine(center3D, leftEdge, DebugDrawColor, DebugDrawDuration);
            Debug.DrawLine(center3D, rightEdge, DebugDrawColor, DebugDrawDuration);

            // 绘制弧线
            for (int i = 0; i < arcSegments; i++)
            {
                float a1 = (startAngle + i * angleStep) * Mathf.Deg2Rad;
                float a2 = (startAngle + (i + 1) * angleStep) * Mathf.Deg2Rad;

                Vector3 p1 = center3D + new Vector3(Mathf.Cos(a1) * radius, Mathf.Sin(a1) * radius, 0);
                Vector3 p2 = center3D + new Vector3(Mathf.Cos(a2) * radius, Mathf.Sin(a2) * radius, 0);

                Debug.DrawLine(p1, p2, DebugDrawColor, DebugDrawDuration);
            }
        }

        /// <summary>
        /// 绘制矩形（旋转）
        /// </summary>
        private void DebugDrawBox(Vector2 center, Vector2 size, float angle)
        {
            if (!DebugDraw) return;

            float rad = angle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);

            float halfWidth = size.x * 0.5f;
            float halfHeight = size.y * 0.5f;

            // 计算四个角点（相对于中心的偏移，然后旋转）
            Vector2[] localCorners = new Vector2[]
            {
                new Vector2(-halfWidth, -halfHeight),
                new Vector2(halfWidth, -halfHeight),
                new Vector2(halfWidth, halfHeight),
                new Vector2(-halfWidth, halfHeight)
            };

            Vector3[] worldCorners = new Vector3[4];
            for (int i = 0; i < 4; i++)
            {
                float rotatedX = localCorners[i].x * cos - localCorners[i].y * sin;
                float rotatedY = localCorners[i].x * sin + localCorners[i].y * cos;
                worldCorners[i] = new Vector3(center.x + rotatedX, center.y + rotatedY, 0);
            }

            // 绘制四条边
            Debug.DrawLine(worldCorners[0], worldCorners[1], DebugDrawColor, DebugDrawDuration);
            Debug.DrawLine(worldCorners[1], worldCorners[2], DebugDrawColor, DebugDrawDuration);
            Debug.DrawLine(worldCorners[2], worldCorners[3], DebugDrawColor, DebugDrawDuration);
            Debug.DrawLine(worldCorners[3], worldCorners[0], DebugDrawColor, DebugDrawDuration);
        }

        #endregion
    }
}
