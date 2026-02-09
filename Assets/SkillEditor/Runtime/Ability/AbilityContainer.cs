using System;
using System.Collections.Generic;
using SkillEditor.Data;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// 技能容器 - 管理ASC拥有的所有技能
    /// </summary>
    public class AbilityContainer
    {
        /// <summary>
        /// 所属的ASC
        /// </summary>
        private readonly AbilitySystemComponent _owner;

        /// <summary>
        /// 已授予的技能列表
        /// </summary>
        private readonly List<GameplayAbilitySpec> _grantedAbilities = new List<GameplayAbilitySpec>();

        /// <summary>
        /// 正在激活的技能列表
        /// </summary>
        private readonly List<GameplayAbilitySpec> _activeAbilities = new List<GameplayAbilitySpec>();

        /// <summary>
        /// 待移除的技能（避免遍历时修改）
        /// </summary>
        private readonly List<GameplayAbilitySpec> _pendingRemove = new List<GameplayAbilitySpec>();

        /// <summary>
        /// 是否正在更新
        /// </summary>
        private bool _isUpdating;

        /// <summary>
        /// 已授予技能数量
        /// </summary>
        public int GrantedCount => _grantedAbilities.Count;

        /// <summary>
        /// 激活中技能数量
        /// </summary>
        public int ActiveCount => _activeAbilities.Count;

        // ============ 事件 ============

        /// <summary>
        /// 技能授予事件
        /// </summary>
        public event Action<GameplayAbilitySpec> OnAbilityGranted;

        /// <summary>
        /// 技能移除事件
        /// </summary>
        public event Action<GameplayAbilitySpec> OnAbilityRemoved;

        // ============ 构造函数 ============

        public AbilityContainer(AbilitySystemComponent owner)
        {
            _owner = owner;
        }

        // ============ 技能管理 ============

        /// <summary>
        /// 授予技能
        /// </summary>
        public GameplayAbilitySpec GrantAbility(SkillGraphData graphData)
        {
            if (graphData == null)
                return null;

            var spec = new GameplayAbilitySpec(graphData, _owner);
            _grantedAbilities.Add(spec);

            OnAbilityGranted?.Invoke(spec);
            return spec;
        }

        /// <summary>
        /// 移除技能
        /// </summary>
        public bool RemoveAbility(GameplayAbilitySpec spec)
        {
            if (spec == null || !_grantedAbilities.Contains(spec))
                return false;

            // 如果技能正在激活，先取消
            if (spec.IsActive)
            {
                spec.Cancel();
                _activeAbilities.Remove(spec);
            }

            _grantedAbilities.Remove(spec);
            OnAbilityRemoved?.Invoke(spec);
            return true;
        }

        /// <summary>
        /// 尝试激活技能
        /// </summary>
        public bool TryActivateAbility(GameplayAbilitySpec spec, AbilitySystemComponent target = null)
        {
            if (spec == null || !_grantedAbilities.Contains(spec))
                return false;

            // 检查是否被其他技能阻止
            if (IsAbilityBlocked(spec))
                return false;

            if (spec.Activate(target))
            {
                _activeAbilities.Add(spec);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 检查技能是否被阻止
        /// </summary>
        private bool IsAbilityBlocked(GameplayAbilitySpec spec)
        {
            if (spec.Tags.AssetTags.IsEmpty)
                return false;

            foreach (var activeAbility in _activeAbilities)
            {
                if (activeAbility.BlocksAbilityWithTags(spec.Tags.AssetTags))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 取消技能
        /// </summary>
        public void CancelAbility(GameplayAbilitySpec spec)
        {
            if (spec == null || !spec.IsActive)
                return;

            spec.Cancel();

            if (_isUpdating)
            {
                _pendingRemove.Add(spec);
            }
            else
            {
                _activeAbilities.Remove(spec);
            }
        }

        /// <summary>
        /// 结束技能
        /// </summary>
        public void EndAbility(GameplayAbilitySpec spec, bool wasCancelled = false)
        {
            if (spec == null || !spec.IsActive)
                return;

            spec.End(wasCancelled);

            if (_isUpdating)
            {
                _pendingRemove.Add(spec);
            }
            else
            {
                _activeAbilities.Remove(spec);
            }
        }

        /// <summary>
        /// 取消带有指定标签的所有技能
        /// </summary>
        public int CancelAbilitiesWithTags(GameplayTagSet tags)
        {
            if (tags.IsEmpty)
                return 0;

            int cancelledCount = 0;
            for (int i = _activeAbilities.Count - 1; i >= 0; i--)
            {
                var ability = _activeAbilities[i];
                if (ability.Tags.AssetTags.HasAnyTags(tags))
                {
                    CancelAbility(ability);
                    cancelledCount++;
                }
            }

            return cancelledCount;
        }

        /// <summary>
        /// 取消所有激活的技能
        /// </summary>
        public void CancelAllAbilities()
        {
            for (int i = _activeAbilities.Count - 1; i >= 0; i--)
            {
                CancelAbility(_activeAbilities[i]);
            }
        }

        // ============ 查询方法 ============

        /// <summary>
        /// 获取所有已授予的技能
        /// </summary>
        public IReadOnlyList<GameplayAbilitySpec> GetGrantedAbilities()
        {
            return _grantedAbilities;
        }

        /// <summary>
        /// 获取所有激活中的技能
        /// </summary>
        public IReadOnlyList<GameplayAbilitySpec> GetActiveAbilities()
        {
            return _activeAbilities;
        }

        /// <summary>
        /// 根据图表数据查找技能
        /// </summary>
        public GameplayAbilitySpec FindAbilityByGraphData(SkillGraphData graphData)
        {
            foreach (var spec in _grantedAbilities)
            {
                if (spec.GraphData == graphData)
                    return spec;
            }
            return null;
        }
        

        /// <summary>
        /// 根据技能ID查找技能
        /// </summary>
        public GameplayAbilitySpec FindAbilityById(int skillId)
        {
            foreach (var spec in _grantedAbilities)
            {
                if (spec.AbilityNodeData?.skillId == skillId)
                    return spec;
            }
            return null;
        }

        /// <summary>
        /// 检查是否拥有带指定标签的技能
        /// </summary>
        public bool HasAbilityWithTag(GameplayTag tag)
        {
            foreach (var spec in _grantedAbilities)
            {
                if (spec.Tags.AssetTags.HasTag(tag))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 检查是否有激活中的带指定标签的技能
        /// </summary>
        public bool HasActiveAbilityWithTag(GameplayTag tag)
        {
            foreach (var spec in _activeAbilities)
            {
                if (spec.Tags.AssetTags.HasTag(tag))
                    return true;
            }
            return false;
        }

        // ============ 更新 ============

        /// <summary>
        /// 每帧更新
        /// </summary>
        public void Tick(float deltaTime)
        {
            _isUpdating = true;

            for (int i = 0; i < _activeAbilities.Count; i++)
            {
                var ability = _activeAbilities[i];
                ability.Tick(deltaTime);

                // 检查技能是否已结束
                if (!ability.IsActive && !_pendingRemove.Contains(ability))
                {
                    _pendingRemove.Add(ability);
                }
            }

            _isUpdating = false;

            // 处理待移除的技能
            if (_pendingRemove.Count > 0)
            {
                foreach (var ability in _pendingRemove)
                {
                    _activeAbilities.Remove(ability);
                }
                _pendingRemove.Clear();
            }
        }

        /// <summary>
        /// 清空所有技能
        /// </summary>
        public void Clear()
        {
            CancelAllAbilities();
            _grantedAbilities.Clear();
            _activeAbilities.Clear();
            _pendingRemove.Clear();
        }
    }
}
