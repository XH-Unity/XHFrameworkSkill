using System.Collections.Generic;

namespace SkillEditor.Runtime.Target
{
    /// <summary>
    /// 目标提供者接口
    /// 用于抽象不同的目标获取方式
    /// </summary>
    public interface ITargetProvider
    {
        /// <summary>
        /// 获取单个目标
        /// </summary>
        AbilitySystemComponent GetTarget();

        /// <summary>
        /// 获取所有目标
        /// </summary>
        List<AbilitySystemComponent> GetTargets();

        /// <summary>
        /// 目标数量
        /// </summary>
        int TargetCount { get; }

        /// <summary>
        /// 是否有有效目标
        /// </summary>
        bool HasValidTargets { get; }

        /// <summary>
        /// 清除目标
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// 简单目标提供者 - 直接持有目标列表
    /// </summary>
    public class SimpleTargetProvider : ITargetProvider
    {
        private List<AbilitySystemComponent> _targets = new List<AbilitySystemComponent>();

        public int TargetCount => _targets.Count;

        public bool HasValidTargets => _targets.Count > 0;

        /// <summary>
        /// 添加目标
        /// </summary>
        public void AddTarget(AbilitySystemComponent target)
        {
            if (target != null && !_targets.Contains(target))
            {
                _targets.Add(target);
            }
        }

        /// <summary>
        /// 添加多个目标
        /// </summary>
        public void AddTargets(IEnumerable<AbilitySystemComponent> targets)
        {
            if (targets == null)
                return;

            foreach (var target in targets)
            {
                AddTarget(target);
            }
        }

        /// <summary>
        /// 移除目标
        /// </summary>
        public void RemoveTarget(AbilitySystemComponent target)
        {
            _targets.Remove(target);
        }

        public AbilitySystemComponent GetTarget()
        {
            return _targets.Count > 0 ? _targets[0] : null;
        }

        public List<AbilitySystemComponent> GetTargets()
        {
            return new List<AbilitySystemComponent>(_targets);
        }

        public void Clear()
        {
            _targets.Clear();
        }
    }

    /// <summary>
    /// 延迟目标提供者 - 在需要时才执行搜索
    /// </summary>
    public class LazyTargetProvider : ITargetProvider
    {
        private System.Func<List<AbilitySystemComponent>> _searchFunc;
        private List<AbilitySystemComponent> _cachedTargets;
        private bool _isCached;

        public LazyTargetProvider(System.Func<List<AbilitySystemComponent>> searchFunc)
        {
            _searchFunc = searchFunc;
            _isCached = false;
        }

        public int TargetCount
        {
            get
            {
                EnsureCached();
                return _cachedTargets?.Count ?? 0;
            }
        }

        public bool HasValidTargets
        {
            get
            {
                EnsureCached();
                return _cachedTargets != null && _cachedTargets.Count > 0;
            }
        }

        public AbilitySystemComponent GetTarget()
        {
            EnsureCached();
            return _cachedTargets?.Count > 0 ? _cachedTargets[0] : null;
        }

        public List<AbilitySystemComponent> GetTargets()
        {
            EnsureCached();
            return _cachedTargets != null ? new List<AbilitySystemComponent>(_cachedTargets) : new List<AbilitySystemComponent>();
        }

        public void Clear()
        {
            _cachedTargets?.Clear();
            _isCached = false;
        }

        /// <summary>
        /// 强制刷新缓存
        /// </summary>
        public void Refresh()
        {
            _isCached = false;
            EnsureCached();
        }

        private void EnsureCached()
        {
            if (!_isCached && _searchFunc != null)
            {
                _cachedTargets = _searchFunc();
                _isCached = true;
            }
        }
    }

    /// <summary>
    /// 单目标提供者 - 只持有一个目标
    /// </summary>
    public class SingleTargetProvider : ITargetProvider
    {
        private AbilitySystemComponent _target;

        public SingleTargetProvider(AbilitySystemComponent target = null)
        {
            _target = target;
        }

        public int TargetCount => _target != null ? 1 : 0;

        public bool HasValidTargets => _target != null;

        public void SetTarget(AbilitySystemComponent target)
        {
            _target = target;
        }

        public AbilitySystemComponent GetTarget()
        {
            return _target;
        }

        public List<AbilitySystemComponent> GetTargets()
        {
            var list = new List<AbilitySystemComponent>();
            if (_target != null)
                list.Add(_target);
            return list;
        }

        public void Clear()
        {
            _target = null;
        }
    }
}
