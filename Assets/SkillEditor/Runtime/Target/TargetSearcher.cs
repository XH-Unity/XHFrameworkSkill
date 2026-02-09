using System.Collections.Generic;
using SkillEditor.Data;
using UnityEngine;

namespace SkillEditor.Runtime.Target
{
    /// <summary>
    /// 目标搜索器 - 提供各种形状的目标搜索功能
    /// </summary>
    public class TargetSearcher
    {
        /// <summary>
        /// 搜索配置
        /// </summary>
        public struct SearchConfig
        {
            /// <summary>
            /// 搜索中心点
            /// </summary>
            public Vector3 Center;

            /// <summary>
            /// 搜索方向
            /// </summary>
            public Vector3 Direction;

            /// <summary>
            /// 搜索者（用于排除自己）
            /// </summary>
            public AbilitySystemComponent Searcher;

            /// <summary>
            /// 目标标签（必须拥有）
            /// </summary>
            public GameplayTagSet TargetTags;

            /// <summary>
            /// 排除标签（不能拥有）
            /// </summary>
            public GameplayTagSet ExcludeTags;

            /// <summary>
            /// 最大目标数（0表示无限制）
            /// </summary>
            public int MaxTargets;

            /// <summary>
            /// 物理层掩码
            /// </summary>
            public int LayerMask;
        }

        /// <summary>
        /// 默认物理层掩码
        /// </summary>
        public static int DefaultLayerMask { get; set; } = -1;

        // ============ 圆形搜索 ============

        /// <summary>
        /// 圆形范围搜索
        /// </summary>
        public static List<AbilitySystemComponent> SearchCircle(Vector3 center, float radius, SearchConfig config)
        {
            var results = new List<AbilitySystemComponent>();

            int layerMask = config.LayerMask != 0 ? config.LayerMask : DefaultLayerMask;
            var colliders = Physics.OverlapSphere(center, radius, layerMask);

            foreach (var collider in colliders)
            {
                var asc = GetASCFromCollider(collider);
                if (asc != null && IsValidTarget(asc, config))
                {
                    results.Add(asc);

                    if (config.MaxTargets > 0 && results.Count >= config.MaxTargets)
                        break;
                }
            }

            return results;
        }

        /// <summary>
        /// 圆形范围搜索（简化版）
        /// </summary>
        public static List<AbilitySystemComponent> SearchCircle(Vector3 center, float radius, AbilitySystemComponent searcher = null)
        {
            return SearchCircle(center, radius, new SearchConfig { Searcher = searcher });
        }

        // ============ 扇形搜索 ============

        /// <summary>
        /// 扇形范围搜索
        /// </summary>
        public static List<AbilitySystemComponent> SearchSector(Vector3 center, Vector3 forward, float radius, float angle, SearchConfig config)
        {
            var results = new List<AbilitySystemComponent>();

            int layerMask = config.LayerMask != 0 ? config.LayerMask : DefaultLayerMask;
            var colliders = Physics.OverlapSphere(center, radius, layerMask);

            float halfAngle = angle * 0.5f;

            foreach (var collider in colliders)
            {
                var asc = GetASCFromCollider(collider);
                if (asc == null || !IsValidTarget(asc, config))
                    continue;

                // 检查是否在扇形范围内
                var dirToTarget = (collider.transform.position - center).normalized;
                float angleToTarget = Vector3.Angle(forward, dirToTarget);

                if (angleToTarget <= halfAngle)
                {
                    results.Add(asc);

                    if (config.MaxTargets > 0 && results.Count >= config.MaxTargets)
                        break;
                }
            }

            return results;
        }

        /// <summary>
        /// 扇形范围搜索（简化版）
        /// </summary>
        public static List<AbilitySystemComponent> SearchSector(Vector3 center, Vector3 forward, float radius, float angle, AbilitySystemComponent searcher = null)
        {
            return SearchSector(center, forward, radius, angle, new SearchConfig { Searcher = searcher });
        }

        // ============ 矩形/直线搜索 ============

        /// <summary>
        /// 矩形范围搜索
        /// </summary>
        public static List<AbilitySystemComponent> SearchBox(Vector3 center, Vector3 halfExtents, Quaternion orientation, SearchConfig config)
        {
            var results = new List<AbilitySystemComponent>();

            int layerMask = config.LayerMask != 0 ? config.LayerMask : DefaultLayerMask;
            var colliders = Physics.OverlapBox(center, halfExtents, orientation, layerMask);

            foreach (var collider in colliders)
            {
                var asc = GetASCFromCollider(collider);
                if (asc != null && IsValidTarget(asc, config))
                {
                    results.Add(asc);

                    if (config.MaxTargets > 0 && results.Count >= config.MaxTargets)
                        break;
                }
            }

            return results;
        }

        /// <summary>
        /// 直线范围搜索
        /// </summary>
        public static List<AbilitySystemComponent> SearchLine(Vector3 start, Vector3 direction, float length, float width, SearchConfig config)
        {
            var results = new List<AbilitySystemComponent>();

            // 计算盒子中心和尺寸
            var center = start + direction * (length * 0.5f);
            var halfExtents = new Vector3(width * 0.5f, 1f, length * 0.5f);
            var rotation = Quaternion.LookRotation(direction);

            int layerMask = config.LayerMask != 0 ? config.LayerMask : DefaultLayerMask;
            var colliders = Physics.OverlapBox(center, halfExtents, rotation, layerMask);

            foreach (var collider in colliders)
            {
                var asc = GetASCFromCollider(collider);
                if (asc != null && IsValidTarget(asc, config))
                {
                    results.Add(asc);

                    if (config.MaxTargets > 0 && results.Count >= config.MaxTargets)
                        break;
                }
            }

            return results;
        }

        // ============ 射线搜索 ============

        /// <summary>
        /// 射线搜索（返回第一个命中的目标）
        /// </summary>
        public static AbilitySystemComponent SearchRay(Vector3 origin, Vector3 direction, float maxDistance, SearchConfig config)
        {
            int layerMask = config.LayerMask != 0 ? config.LayerMask : DefaultLayerMask;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance, layerMask))
            {
                var asc = GetASCFromCollider(hit.collider);
                if (asc != null && IsValidTarget(asc, config))
                {
                    return asc;
                }
            }

            return null;
        }

        /// <summary>
        /// 射线搜索（返回所有命中的目标）
        /// </summary>
        public static List<AbilitySystemComponent> SearchRayAll(Vector3 origin, Vector3 direction, float maxDistance, SearchConfig config)
        {
            var results = new List<AbilitySystemComponent>();

            int layerMask = config.LayerMask != 0 ? config.LayerMask : DefaultLayerMask;
            var hits = Physics.RaycastAll(origin, direction, maxDistance, layerMask);

            foreach (var hit in hits)
            {
                var asc = GetASCFromCollider(hit.collider);
                if (asc != null && IsValidTarget(asc, config))
                {
                    results.Add(asc);

                    if (config.MaxTargets > 0 && results.Count >= config.MaxTargets)
                        break;
                }
            }

            return results;
        }

        // ============ 最近目标搜索 ============

        /// <summary>
        /// 搜索最近的目标
        /// </summary>
        public static AbilitySystemComponent SearchNearest(Vector3 center, float radius, SearchConfig config)
        {
            var targets = SearchCircle(center, radius, config);
            if (targets.Count == 0)
                return null;

            AbilitySystemComponent nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var target in targets)
            {
                if (target?.Owner == null)
                    continue;

                float distance = Vector3.Distance(center, target.Owner.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = target;
                }
            }

            return nearest;
        }

        // ============ 辅助方法 ============

        /// <summary>
        /// 从Collider获取ASC
        /// </summary>
        private static AbilitySystemComponent GetASCFromCollider(Collider collider)
        {
            if (collider == null)
                return null;

            // 尝试从GameObject获取ASC
            var asc = collider.GetComponent<AbilitySystemComponent>();
            if (asc != null)
                return asc;

            // 尝试从父对象获取
            asc = collider.GetComponentInParent<AbilitySystemComponent>();
            return asc;
        }

        /// <summary>
        /// 检查目标是否有效
        /// </summary>
        private static bool IsValidTarget(AbilitySystemComponent target, SearchConfig config)
        {
            if (target == null)
                return false;

            // 排除搜索者自己
            if (config.Searcher != null && target == config.Searcher)
                return false;

            // 检查目标标签
            if (!config.TargetTags.IsEmpty)
            {
                if (!target.HasAnyTags(config.TargetTags))
                    return false;
            }

            // 检查排除标签
            if (!config.ExcludeTags.IsEmpty)
            {
                if (target.HasAnyTags(config.ExcludeTags))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 按距离排序目标
        /// </summary>
        public static void SortByDistance(List<AbilitySystemComponent> targets, Vector3 center, bool ascending = true)
        {
            targets.Sort((a, b) =>
            {
                if (a?.Owner == null) return 1;
                if (b?.Owner == null) return -1;

                float distA = Vector3.Distance(center, a.Owner.transform.position);
                float distB = Vector3.Distance(center, b.Owner.transform.position);

                return ascending ? distA.CompareTo(distB) : distB.CompareTo(distA);
            });
        }

        /// <summary>
        /// 过滤目标列表
        /// </summary>
        public static List<AbilitySystemComponent> FilterTargets(List<AbilitySystemComponent> targets, SearchConfig config)
        {
            var filtered = new List<AbilitySystemComponent>();

            foreach (var target in targets)
            {
                if (IsValidTarget(target, config))
                {
                    filtered.Add(target);

                    if (config.MaxTargets > 0 && filtered.Count >= config.MaxTargets)
                        break;
                }
            }

            return filtered;
        }
    }
}
