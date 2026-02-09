using System.Collections.Generic;
using UnityEngine;
using SkillEditor.Data;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// Spec执行上下文 - 提供执行所需的所有信息
    /// 在整个技能执行过程中传递
    /// </summary>
    public class SpecExecutionContext
    {
        /// <summary>
        /// 所属的技能Spec
        /// </summary>
        public GameplayAbilitySpec AbilitySpec { get; set; }

        /// <summary>
        /// 当前触发执行的EffectSpec（用于管理Cue生命周期，如Buff）
        /// </summary>
        public GameplayEffectSpec OwnerEffectSpec { get; set; }

        /// <summary>
        /// 施法者
        /// </summary>
        public AbilitySystemComponent Caster { get; set; }

        /// <summary>
        /// 当前目标列表
        /// </summary>
        public List<AbilitySystemComponent> Targets { get; private set; } = new List<AbilitySystemComponent>();

        /// <summary>
        /// 技能主目标
        /// </summary>
        public AbilitySystemComponent MainTarget { get; set; }

        /// <summary>
        /// 父节点传入的目标（用于范围搜索等场景，子节点通过 TargetType.ParentInput 获取）
        /// </summary>
        public AbilitySystemComponent ParentInputTarget { get; set; }

        /// <summary>
        /// 投射物对象（由 ProjectileEffectSpec 设置）
        /// </summary>
        public GameObject ProjectileObject { get; set; }

        /// <summary>
        /// 放置物对象（由 PlacementEffectSpec 设置）
        /// </summary>
        public GameObject PlacementObject { get; set; }

        /// <summary>
        /// 技能等级
        /// </summary>
        public int AbilityLevel { get; set; } = 1;

        /// <summary>
        /// 堆叠层数（从触发此上下文的 Effect 获取）
        /// </summary>
        public int StackCount { get; set; } = 1;

        /// <summary>
        /// 自定义数据字典（SetByCaller数据）
        /// </summary>
        public Dictionary<string, object> CustomData { get; private set; } = new Dictionary<string, object>();

        // ============ 目标管理 ============

        /// <summary>
        /// 设置目标列表
        /// </summary>
        public void SetTargets(List<AbilitySystemComponent> targets)
        {
            Targets.Clear();
            if (targets != null)
            {
                Targets.AddRange(targets);
            }
        }

        /// <summary>
        /// 添加目标
        /// </summary>
        public void AddTarget(AbilitySystemComponent target)
        {
            if (target != null && !Targets.Contains(target))
            {
                Targets.Add(target);
            }
        }

        /// <summary>
        /// 清空目标列表
        /// </summary>
        public void ClearTargets()
        {
            Targets.Clear();
        }

        /// <summary>
        /// 根据TargetType获取目标
        /// </summary>
        public AbilitySystemComponent GetTarget(TargetType targetType)
        {
            switch (targetType)
            {
                case TargetType.Caster:
                    return Caster;
                case TargetType.MainTarget:
                    return MainTarget;
                case TargetType.ParentInput:
                    return ParentInputTarget;
                default:
                    return MainTarget;
            }
        }

        /// <summary>
        /// 根据TargetType获取目标列表
        /// </summary>
        public List<AbilitySystemComponent> GetTargets(TargetType targetType)
        {
            switch (targetType)
            {
                case TargetType.Caster:
                    return new List<AbilitySystemComponent> { Caster };
                case TargetType.MainTarget:
                    return MainTarget != null
                        ? new List<AbilitySystemComponent> { MainTarget }
                        : new List<AbilitySystemComponent>();
                case TargetType.ParentInput:
                    return ParentInputTarget != null
                        ? new List<AbilitySystemComponent> { ParentInputTarget }
                        : new List<AbilitySystemComponent>();
                default:
                    return new List<AbilitySystemComponent>(Targets);
            }
        }

        // ============ 自定义数据 ============

        /// <summary>
        /// 设置自定义数据
        /// </summary>
        public void SetCustomData(string key, object value)
        {
            CustomData[key] = value;
        }

        /// <summary>
        /// 获取自定义数据
        /// </summary>
        public T GetCustomData<T>(string key, T defaultValue = default)
        {
            if (CustomData.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }

        /// <summary>
        /// 检查是否存在自定义数据
        /// </summary>
        public bool HasCustomData(string key)
        {
            return CustomData.ContainsKey(key);
        }

        // ============ 创建上下文 ============

        /// <summary>
        /// 创建带有父节点目标的上下文（用于范围搜索等场景）
        /// </summary>
        public SpecExecutionContext CreateWithParentInput(AbilitySystemComponent parentInputTarget)
        {
            var newContext = new SpecExecutionContext
            {
                AbilitySpec = this.AbilitySpec,
                OwnerEffectSpec = this.OwnerEffectSpec,
                Caster = this.Caster,
                MainTarget = this.MainTarget,
                ParentInputTarget = parentInputTarget,
                ProjectileObject = this.ProjectileObject,
                PlacementObject = this.PlacementObject,
                AbilityLevel = this.AbilityLevel,
                StackCount = this.StackCount
            };

            // 复制自定义数据
            foreach (var kvp in CustomData)
            {
                newContext.CustomData[kvp.Key] = kvp.Value;
            }

            return newContext;
        }

        // ============ 位置获取 ============

        /// <summary>
        /// 根据 PositionSourceType 和挂点名称获取位置
        /// </summary>
        public Vector3 GetPosition(PositionSourceType sourceType, string bindingName = null)
        {
            GameObject sourceObject = GetSourceObject(sourceType);
            if (sourceObject == null)
            {
                return Vector3.zero;
            }

            return GetPositionFromObject(sourceObject, bindingName);
        }

        /// <summary>
        /// 根据 PositionSourceType 获取源对象
        /// </summary>
        public GameObject GetSourceObject(PositionSourceType sourceType)
        {
            switch (sourceType)
            {
                case PositionSourceType.Caster:
                    return Caster?.Owner;
                case PositionSourceType.MainTarget:
                    return MainTarget?.Owner;
                case PositionSourceType.ParentInput:
                    return ParentInputTarget?.Owner;
                case PositionSourceType.Projectile:
                    return ProjectileObject;
                case PositionSourceType.Placement:
                    return PlacementObject;
                default:
                    return null;
            }
        }

        /// <summary>
        /// 从对象获取位置（支持挂点）
        /// </summary>
        private Vector3 GetPositionFromObject(GameObject obj, string bindingName)
        {
            if (obj == null) return Vector3.zero;

            // 如果没有指定挂点，返回对象位置
            if (string.IsNullOrEmpty(bindingName))
            {
                return obj.transform.position;
            }

            // 查找挂点
            Transform bindingPoint = obj.transform.Find(bindingName);
            if (bindingPoint != null)
            {
                return bindingPoint.position;
            }

            // 递归查找挂点
            bindingPoint = FindChildRecursive(obj.transform, bindingName);
            if (bindingPoint != null)
            {
                return bindingPoint.position;
            }

            // 找不到挂点，返回对象位置
            return obj.transform.position;
        }

        /// <summary>
        /// 递归查找子对象
        /// </summary>
        private Transform FindChildRecursive(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;

                var found = FindChildRecursive(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }

        /// <summary>
        /// 创建修改器计算上下文
        /// </summary>
        public ModifierCalculationContext CreateModifierContext(AbilitySystemComponent target)
        {
            return new ModifierCalculationContext
            {
                SourceAttributes = Caster?.Attributes,
                TargetAttributes = target?.Attributes,
                EffectLevel = AbilityLevel
            };
        }
    }
}
