using System;
using System.Collections.Generic;
using SkillEditor.Data;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// 效果容器 - 管理ASC上所有激活的效果
    /// 职责：存储效果、Tick更新、查询效果
    /// 堆叠判断由 GameplayEffectSpec.ExecuteDurationEffect 处理
    /// </summary>
    public class GameplayEffectContainer
    {
        /// <summary>
        /// 所属的ASC
        /// </summary>
        private readonly AbilitySystemComponent _owner;

        /// <summary>
        /// 激活的效果列表
        /// </summary>
        private readonly List<GameplayEffectSpec> _activeEffects = new List<GameplayEffectSpec>();

        /// <summary>
        /// 待移除的效果（避免遍历时修改）
        /// </summary>
        private readonly List<GameplayEffectSpec> _pendingRemove = new List<GameplayEffectSpec>();

        /// <summary>
        /// 是否正在更新
        /// </summary>
        private bool _isUpdating;

        /// <summary>
        /// 激活效果数量
        /// </summary>
        public int Count => _activeEffects.Count;

        // ============ 构造函数 ============

        public GameplayEffectContainer(AbilitySystemComponent owner)
        {
            _owner = owner;
        }

        // ============ 效果管理 ============

        /// <summary>
        /// 添加效果到容器（由 GameplayEffectSpec 调用）
        /// </summary>
        public void AddEffect(GameplayEffectSpec spec)
        {
            if (spec == null || _activeEffects.Contains(spec))
                return;

            _activeEffects.Add(spec);
        }

        /// <summary>
        /// 查找可堆叠的效果（由 GameplayEffectSpec 调用）
        /// </summary>
        public GameplayEffectSpec FindStackableEffect(GameplayEffectSpec spec)
        {
            var stackType = spec.EffectNodeData?.stackType ?? StackType.None;
            if (stackType == StackType.None)
                return null;

            foreach (var effect in _activeEffects)
            {
                // 检查是否是同类型效果
                if (effect.EffectNodeData?.nodeType != spec.EffectNodeData?.nodeType)
                    continue;

                // 检查资产标签是否匹配
                if (!effect.Tags.AssetTags.Equals(spec.Tags.AssetTags))
                    continue;

                switch (stackType)
                {
                    case StackType.AggregateByTarget:
                        // 按目标堆叠 - 同一目标上的同类效果堆叠
                        return effect;

                    case StackType.AggregateBySource:
                        // 按来源堆叠 - 同一来源的同类效果堆叠
                        if (effect.Source == spec.Source)
                            return effect;
                        break;
                }
            }

            return null;
        }

        /// <summary>
        /// 移除效果
        /// </summary>
        public bool RemoveEffect(GameplayEffectSpec spec)
        {
            if (spec == null || !_activeEffects.Contains(spec))
                return false;

            if (_isUpdating)
            {
                if (!_pendingRemove.Contains(spec))
                {
                    _pendingRemove.Add(spec);
                }
            }
            else
            {
                RemoveEffectInternal(spec);
            }

            return true;
        }

        /// <summary>
        /// 内部移除效果
        /// </summary>
        private void RemoveEffectInternal(GameplayEffectSpec spec)
        {
            spec.Remove();
            _activeEffects.Remove(spec);
        }

        /// <summary>
        /// 移除带有指定标签的所有效果
        /// </summary>
        public int RemoveEffectsWithTags(GameplayTagSet tags)
        {
            if (tags.IsEmpty)
                return 0;

            int removedCount = 0;
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = _activeEffects[i];
                if (effect.Tags.AssetTags.HasAnyTags(tags))
                {
                    RemoveEffect(effect);
                    removedCount++;
                }
            }

            return removedCount;
        }

        /// <summary>
        /// 移除来自指定来源的所有效果
        /// </summary>
        public int RemoveEffectsFromSource(AbilitySystemComponent source)
        {
            if (source == null)
                return 0;

            int removedCount = 0;
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = _activeEffects[i];
                if (effect.Source == source)
                {
                    RemoveEffect(effect);
                    removedCount++;
                }
            }

            return removedCount;
        }

        // ============ 查询方法 ============

        /// <summary>
        /// 获取所有激活的效果
        /// </summary>
        public IReadOnlyList<GameplayEffectSpec> GetActiveEffects()
        {
            return _activeEffects;
        }

        /// <summary>
        /// 根据标签查找效果
        /// </summary>
        public GameplayEffectSpec FindEffectByTag(GameplayTag tag)
        {
            foreach (var effect in _activeEffects)
            {
                if (effect.Tags.AssetTags.HasTag(tag))
                    return effect;
            }
            return null;
        }

        /// <summary>
        /// 根据授予标签查找效果（用于查找冷却效果等）
        /// </summary>
        public GameplayEffectSpec FindEffectByGrantedTag(GameplayTag tag)
        {
            foreach (var effect in _activeEffects)
            {
                if (effect.Tags.GrantedTags.HasTag(tag))
                    return effect;
            }
            return null;
        }

        /// <summary>
        /// 获取带有指定标签的所有效果
        /// </summary>
        public List<GameplayEffectSpec> GetEffectsWithTag(GameplayTag tag)
        {
            var result = new List<GameplayEffectSpec>();
            foreach (var effect in _activeEffects)
            {
                if (effect.Tags.AssetTags.HasTag(tag))
                    result.Add(effect);
            }
            return result;
        }

        /// <summary>
        /// 检查是否有带指定标签的效果
        /// </summary>
        public bool HasEffectWithTag(GameplayTag tag)
        {
            return FindEffectByTag(tag) != null;
        }

        /// <summary>
        /// 根据节点Guid查找效果
        /// </summary>
        public GameplayEffectSpec FindEffectByNodeGuid(string nodeGuid)
        {
            if (string.IsNullOrEmpty(nodeGuid))
                return null;

            foreach (var effect in _activeEffects)
            {
                if (effect.NodeGuid == nodeGuid)
                    return effect;
            }
            return null;
        }

        /// <summary>
        /// 检查是否已有指定效果
        /// </summary>
        public bool HasEffect(GameplayEffectSpec spec)
        {
            return spec != null && _activeEffects.Contains(spec);
        }

        /// <summary>
        /// 获取指定类型的效果数量
        /// </summary>
        public int GetEffectCountByType(NodeType nodeType)
        {
            int count = 0;
            foreach (var effect in _activeEffects)
            {
                if (effect.EffectNodeData?.nodeType == nodeType)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// 根据Buff ID查找Buff效果
        /// </summary>
        public BuffEffectSpec FindBuffById(int buffId)
        {
            foreach (var effect in _activeEffects)
            {
                if (effect is BuffEffectSpec buffSpec)
                {
                    var buffData = buffSpec.EffectNodeData as BuffEffectNodeData;
                    if (buffData != null && buffData.buffId == buffId)
                        return buffSpec;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取所有指定ID的Buff效果
        /// </summary>
        public List<BuffEffectSpec> GetAllBuffsById(int buffId)
        {
            var result = new List<BuffEffectSpec>();
            foreach (var effect in _activeEffects)
            {
                if (effect is BuffEffectSpec buffSpec)
                {
                    var buffData = buffSpec.EffectNodeData as BuffEffectNodeData;
                    if (buffData != null && buffData.buffId == buffId)
                        result.Add(buffSpec);
                }
            }
            return result;
        }

        // ============ 更新 ============

        /// <summary>
        /// 每帧更新
        /// </summary>
        public void Tick(float deltaTime)
        {
            _isUpdating = true;

            for (int i = 0; i < _activeEffects.Count; i++)
            {
                var effect = _activeEffects[i];
                effect.Tick(deltaTime);

                // 检查是否已过期
                if (effect.IsExpired && !_pendingRemove.Contains(effect))
                {
                    _pendingRemove.Add(effect);
                }
            }

            _isUpdating = false;

            // 处理待移除的效果
            if (_pendingRemove.Count > 0)
            {
                foreach (var effect in _pendingRemove)
                {
                    RemoveEffectInternal(effect);
                }
                _pendingRemove.Clear();
            }
        }

        /// <summary>
        /// 清空所有效果
        /// </summary>
        public void Clear()
        {
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                RemoveEffectInternal(_activeEffects[i]);
            }
            _activeEffects.Clear();
            _pendingRemove.Clear();
        }
    }
}
