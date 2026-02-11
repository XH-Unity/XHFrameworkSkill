using System;
using System.Collections.Generic;
using SkillEditor.Data;
using SkillEditor.Runtime.Utils;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// 技能冷却信息（统一接口，支持普通CD和充能CD）
    /// </summary>
    public class SkillCooldownInfo
    {
        /// <summary>
        /// 是否在CD中
        /// </summary>
        public bool IsOnCooldown;

        /// <summary>
        /// 是否是充能CD
        /// </summary>
        public bool IsChargeCooldown;

        // ============ 普通CD ============

        /// <summary>
        /// 剩余CD时间
        /// </summary>
        public float RemainingTime;

        /// <summary>
        /// 总CD时间
        /// </summary>
        public float TotalDuration;

        // ============ 充能CD ============

        /// <summary>
        /// 当前充能数
        /// </summary>
        public int CurrentCharges;

        /// <summary>
        /// 最大充能数
        /// </summary>
        public int MaxCharges;

        /// <summary>
        /// 下一层充能进度 (0-1)
        /// </summary>
        public float ChargeProgress;

        /// <summary>
        /// 下一层充能剩余时间
        /// </summary>
        public float ChargeTimeRemaining;
    }

    /// <summary>
    /// 时间效果运行时数据
    /// </summary>
    public class TimeEffectRuntime
    {
        public float TriggerTime { get; set; }
        public string PortId { get; set; }
        public bool HasTriggered { get; set; }
    }

    /// <summary>
    /// 时间Cue运行时数据 - 有开始和结束时间，管理Cue生命周期
    /// </summary>
    public class TimeCueRuntime
    {
        public float StartTime { get; set; }
        public float EndTime { get; set; }
        public string PortId { get; set; }
        public bool HasStarted { get; set; }
        public bool HasEnded { get; set; }
        /// <summary>
        /// 触发的Cue Spec列表（用于生命周期管理）
        /// </summary>
        public List<GameplayCueSpec> TriggeredCueSpecs { get; set; } = new List<GameplayCueSpec>();
    }

    /// <summary>
    /// 技能运行时实例 - 对应GAS的FGameplayAbilitySpec
    /// 每个授予的技能都有一个Spec实例，包含运行时状态和执行逻辑
    /// </summary>
    public class GameplayAbilitySpec
    {
        /// <summary>
        /// 唯一标识符
        /// </summary>
        public string SpecId { get; private set; }

        /// <summary>
        /// 技能ID（用于从数据中心获取数据）
        /// </summary>
        public string SkillId { get; private set; }

        /// <summary>
        /// 拥有此技能的ASC
        /// </summary>
        public AbilitySystemComponent Owner { get; private set; }

        /// <summary>
        /// 当前状态
        /// </summary>
        public AbilityState State { get; private set; } = AbilityState.Inactive;

        /// <summary>
        /// 技能等级
        /// </summary>
        public int Level { get; set; } = 1;

        /// <summary>
        /// 标签容器
        /// </summary>
        public AbilityTagContainer Tags { get; private set; }

        /// <summary>
        /// 激活时间
        /// </summary>
        public float ActivationTime { get; private set; }

        /// <summary>
        /// 是否正在激活
        /// </summary>
        public bool IsActive => State == AbilityState.Active;

        /// <summary>
        /// 是否正在执行
        /// </summary>
        public bool IsRunning { get; private set; }

        // ============ 静态数据访问 ============

        /// <summary>
        /// 技能图表数据（从数据中心获取）
        /// </summary>
        public SkillGraphData GraphData => SkillDataCenter.Instance.GetSkillGraph(SkillId);

        /// <summary>
        /// 技能节点数据
        /// </summary>
        public AbilityNodeData AbilityNodeData { get; private set; }

        /// <summary>
        /// Ability节点的guid
        /// </summary>
        private string _abilityNodeGuid;

        // ============ 执行相关 ============

        /// <summary>
        /// 执行上下文
        /// </summary>
        private SpecExecutionContext _context;

        /// <summary>
        /// 正在执行的Effect列表（技能管理持续/周期Effect）
        /// </summary>
        private readonly List<GameplayEffectSpec> _runningEffects = new List<GameplayEffectSpec>();

        /// <summary>
        /// 待移除的Effect
        /// </summary>
        private readonly List<GameplayEffectSpec> _pendingRemoveEffects = new List<GameplayEffectSpec>();

        // ============ 缓存的节点数据 ============

        /// <summary>
        /// 消耗节点Guid
        /// </summary>
        private string _costNodeGuid;

        /// <summary>
        /// 冷却节点Guid
        /// </summary>
        private string _cooldownNodeGuid;

        /// <summary>
        /// 动画节点Guid
        /// </summary>
        private string _animationNodeGuid;

        // ============ 动画相关 ============

        /// <summary>
        /// 动画名称
        /// </summary>
        private string _animationName;

        /// <summary>
        /// 动画时长
        /// </summary>
        private float _animationDuration;

        /// <summary>
        /// 是否循环播放动画
        /// </summary>
        private bool _isAnimationLooping;

        /// <summary>
        /// 当前播放时间
        /// </summary>
        private float _currentPlayTime;

        /// <summary>
        /// 时间效果列表
        /// </summary>
        private List<TimeEffectRuntime> _timeEffects;

        /// <summary>
        /// 时间Cue列表
        /// </summary>
        private List<TimeCueRuntime> _timeCues;

        // ============ 事件 ============

        /// <summary>
        /// 技能激活事件
        /// </summary>
        public event Action<GameplayAbilitySpec> OnActivated;

        /// <summary>
        /// 技能结束事件
        /// </summary>
        public event Action<GameplayAbilitySpec, bool> OnEnded;

        // ============ 构造函数 ============

        public GameplayAbilitySpec(SkillGraphData graphData, AbilitySystemComponent owner)
        {
            SpecId = Guid.NewGuid().ToString();
            Owner = owner;
       
            // 使用ScriptableObject的name作为skillId
            if (graphData != null)
            {
                SkillId = graphData.name;
                // 注册到数据中心
                SkillDataCenter.Instance.RegisterSkillGraph(graphData);
            }

            // 查找Ability节点
            FindAbilityNode();

            // 查找Cost和Cooldown节点
            FindCostAndCooldownNodes();

            // 构建标签容器
            if (AbilityNodeData != null)
            {
                Tags = new AbilityTagContainer(AbilityNodeData);
            }
          
        }

        /// <summary>
        /// 从图表中查找Ability节点
        /// </summary>
        private void FindAbilityNode()
        {
            var graphData = GraphData;
            if (graphData?.nodes == null) return;

            foreach (var node in graphData.nodes)
            {
                if (node is AbilityNodeData abilityNode)
                {
                    AbilityNodeData = abilityNode;
                    _abilityNodeGuid = abilityNode.guid;
                    break;
                }
            }

            // 从连接的AnimationNode读取动画配置
            FindAnimationNode();
        }

        /// <summary>
        /// 从AbilityNode的"动画"端口连接查找AnimationNode，读取动画配置和时间轴数据
        /// </summary>
        private void FindAnimationNode()
        {
            _timeEffects = new List<TimeEffectRuntime>();
            _timeCues = new List<TimeCueRuntime>();

            if (string.IsNullOrEmpty(_abilityNodeGuid)) return;

            var connectedNodes = SkillDataCenter.Instance.GetConnectedNodes(SkillId, _abilityNodeGuid, "动画");
            if (connectedNodes == null) return;

            foreach (var node in connectedNodes)
            {
                if (node is AnimationNodeData animNode)
                {
                    _animationNodeGuid = animNode.guid;

                    // 初始化动画配置（帧数转秒）
                    _animationName = animNode.animationName;
                    int durationFrames = (int)FormulaEvaluator.EvaluateSimple(animNode.animationDuration, 1f);
                    _animationDuration = SkillConstants.FramesToSeconds(durationFrames);
                    _isAnimationLooping = animNode.isAnimationLooping;

                    // 初始化时间效果列表（帧转秒）
                    if (animNode.timeEffects != null)
                    {
                        foreach (var te in animNode.timeEffects)
                        {
                            _timeEffects.Add(new TimeEffectRuntime
                            {
                                TriggerTime = SkillConstants.FramesToSeconds(te.triggerTime),
                                PortId = te.portId,
                                HasTriggered = false
                            });
                        }
                    }

                    // 初始化时间Cue列表（帧转秒）
                    if (animNode.timeCues != null)
                    {
                        foreach (var tc in animNode.timeCues)
                        {
                            _timeCues.Add(new TimeCueRuntime
                            {
                                StartTime = SkillConstants.FramesToSeconds(tc.startTime),
                                EndTime = tc.endTime < 0 ? -1f : SkillConstants.FramesToSeconds(tc.endTime),
                                PortId = tc.portId,
                                HasStarted = false,
                                HasEnded = false
                            });
                        }
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// 从图表中查找Cost和Cooldown节点
        /// </summary>
        private void FindCostAndCooldownNodes()
        {
            var graphData = GraphData;
            if (graphData?.connections == null || AbilityNodeData == null)
                return;

            foreach (var conn in graphData.connections)
            {
                if (conn.outputNodeGuid != AbilityNodeData.guid)
                    continue;

                // 消耗端口
                if (conn.outputPortName == "消耗")
                {
                    _costNodeGuid = conn.inputNodeGuid;
                }
                // 冷却端口
                else if (conn.outputPortName == "冷却")
                {
                    _cooldownNodeGuid = conn.inputNodeGuid;
                }
            }
        }

        // ============ 生命周期方法 ============

        /// <summary>
        /// 检查是否可以激活
        /// </summary>
        public bool CanActivate()
        {
            if (State == AbilityState.Active)
                return false;

            if (Owner == null)
                return false;

            // 1. 检查激活所需标签
            if (!Tags.ActivationRequiredTags.IsEmpty)
            {
                if (!Owner.HasAllTags(Tags.ActivationRequiredTags))
                    return false;
            }

            // 2. 检查激活阻止标签
            if (!Tags.ActivationBlockedTags.IsEmpty)
            {
                if (Owner.HasAnyTags(Tags.ActivationBlockedTags))
                    return false;
            }

            // 4. 检查消耗
            if (!CanAffordCost())
                return false;

            return true;
        }

        /// <summary>
        /// 检查技能是否在冷却中（支持普通CD和充能CD）
        /// </summary>
        public bool IsOnCooldown()
        {
            // 先检查是否是充能技能
            var cdEffect = GetCooldownEffect();
            if (cdEffect != null && cdEffect.IsChargeCooldown)
            {
                return cdEffect.CurrentCharges <= 0;
            }

            // 普通 CD 检查标签
            var cooldownTag = GetCooldownTag();
            if (!cooldownTag.IsEmpty)
            {
                return Owner.HasTag(cooldownTag);
            }
            return false;
        }

        /// <summary>
        /// 获取冷却效果（支持普通CD和充能CD）
        /// </summary>
        public CooldownEffectSpec GetCooldownEffect()
        {
            if (string.IsNullOrEmpty(_cooldownNodeGuid))
                return null;

            return Owner.EffectContainer.FindEffectByNodeGuid(_cooldownNodeGuid) as CooldownEffectSpec;
        }

        /// <summary>
        /// 获取剩余冷却时间（普通CD）
        /// </summary>
        public GameplayEffectSpec GetRemainingCooldown()
        {
            var cooldownTag = GetCooldownTag();
            if (!cooldownTag.IsEmpty)
            {
                var cooldownEffect = Owner.EffectContainer.FindEffectByGrantedTag(cooldownTag);
                if (cooldownEffect != null)
                {
                    return cooldownEffect;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取冷却信息（统一接口，支持普通CD和充能CD）
        /// </summary>
        public SkillCooldownInfo GetCooldownInfo()
        {
            var info = new SkillCooldownInfo();
            var cdEffect = GetCooldownEffect();

            if (cdEffect == null)
            {
                info.IsOnCooldown = false;
                return info;
            }

            info.IsChargeCooldown = cdEffect.IsChargeCooldown;

            if (cdEffect.IsChargeCooldown)
            {
                // 充能CD信息
                info.CurrentCharges = cdEffect.CurrentCharges;
                info.MaxCharges = cdEffect.MaxCharges;
                info.ChargeProgress = cdEffect.ChargeProgress;
                info.ChargeTimeRemaining = cdEffect.ChargeTimer;
                info.IsOnCooldown = cdEffect.CurrentCharges <= 0;
            }
            else
            {
                // 普通CD信息
                info.RemainingTime = cdEffect.RemainingTime;
                info.TotalDuration = cdEffect.Duration;
                info.IsOnCooldown = cdEffect.RemainingTime > 0;
            }

            return info;
        }

        /// <summary>
        /// 从连接的冷却节点获取冷却标签
        /// </summary>
        private GameplayTag GetCooldownTag()
        {
            if (string.IsNullOrEmpty(_cooldownNodeGuid))
                return default;

            var cooldownNodeData = SkillDataCenter.Instance.GetNodeData(SkillId, _cooldownNodeGuid) as CooldownEffectNodeData;
            if (cooldownNodeData != null && cooldownNodeData.grantedTags.Tags != null && cooldownNodeData.grantedTags.Tags.Length > 0)
            {
                return cooldownNodeData.grantedTags.Tags[0];
            }
            return default;
        }

        /// <summary>
        /// 检查是否能支付消耗
        /// </summary>
        public bool CanAffordCost()
        {
            if (string.IsNullOrEmpty(_costNodeGuid))
                return true;

            var costNodeData = SkillDataCenter.Instance.GetNodeData(SkillId, _costNodeGuid) as CostEffectNodeData;
            if (costNodeData?.attributeModifiers == null)
                return true;

            if (Owner?.Attributes == null)
                return true;

            foreach (var modData in costNodeData.attributeModifiers)
            {
                var modifier = AttributeModifier.FromData(modData);
                float costValue = UnityEngine.Mathf.Abs(modifier.CalculateMagnitude(null));

                float? currentValue = Owner.Attributes.GetCurrentValue(modifier.TargetAttrType);
                if (!currentValue.HasValue)
                    continue;

                if (currentValue.Value < costValue)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 激活技能
        /// </summary>
        internal bool Activate(AbilitySystemComponent target = null)
        {
            if (!CanActivate())
                return false;

            State = AbilityState.Active;
            IsRunning = true;
            ActivationTime = UnityEngine.Time.time;

            // 注册标签变化监听（用于 OngoingBlockedTags 检查）
            RegisterTagListener();
            Owner.OnTGameplayEvent += OnGameplayEvent;
            // 添加激活时授予的标签
            if (!Tags.ActivationOwnedTags.IsEmpty)
            {
                Owner.OwnedTags.AddTags(Tags.ActivationOwnedTags);
            }

            // 取消带有指定标签的其他技能
            if (!Tags.CancelAbilitiesWithTags.IsEmpty)
            {
                Owner.Abilities.CancelAbilitiesWithTags(Tags.CancelAbilitiesWithTags);
            }

            // 创建执行上下文
            _context = new SpecExecutionContext
            {
                AbilitySpec = this,
                Caster = Owner,
                MainTarget = target,
                AbilityLevel = Level
            };

            if (target != null)
            {
                _context.AddTarget(target);
            }

            // 重置播放时间和时间效果状态
            _currentPlayTime = 0f;
            ResetTimeEffects();
            ResetTimeCues();

            // 播放动画
            PlayAnimation(_animationName,_isAnimationLooping);

            // 执行消耗节点（扣除资源）
            if (!string.IsNullOrEmpty(_abilityNodeGuid))
            {
                SpecExecutor.ExecuteConnectedNodes(SkillId, _abilityNodeGuid, "消耗", _context);
            }

            // 执行冷却节点（应用冷却）
            if (!string.IsNullOrEmpty(_abilityNodeGuid))
            {
                SpecExecutor.ExecuteConnectedNodes(SkillId, _abilityNodeGuid, "冷却", _context);
            }

            // 从Ability节点的"激活"端口开始执行（使用SpecExecutor）
            if (!string.IsNullOrEmpty(_abilityNodeGuid))
            {
                SpecExecutor.ExecuteConnectedNodes(SkillId, _abilityNodeGuid, "激活", _context);
            }

            OnActivated?.Invoke(this);
            return true;
        }

        /// <summary>
        /// 结束技能
        /// </summary>
        internal void End(bool wasCancelled = false)
        {
            if (State != AbilityState.Active)
                return;

            // 取消标签变化监听
            UnregisterTagListener();
            Owner.OnTGameplayEvent -= OnGameplayEvent;
            State = wasCancelled ? AbilityState.Cancelled : AbilityState.Ended;
            IsRunning = false;

            // 停止动画
            StopAnimation();

            // 停止所有时间Cue
            StopAllTimeCues();

            // 只取消标记为"随技能结束而取消"的Effect
            // CD、Buff等Effect不会被取消，它们由ASC管理生命周期
            foreach (var effect in _runningEffects)
            {
                if (effect == null) continue;

                var effectData = effect.EffectNodeData;
                if (effectData != null)
                {
                    // 通过ASC的EffectContainer移除，确保IsExpired被正确设置
                    if (effect.Target != null)
                    {
                        effect.Target.EffectContainer.RemoveEffect(effect);
                    }
                    else
                    {
                        effect.Remove();
                    }
                }
            }
            _runningEffects.Clear();
            _pendingRemoveEffects.Clear();

            // 移除激活时授予的标签
            if (!Tags.ActivationOwnedTags.IsEmpty)
            {
                Owner.OwnedTags.RemoveTags(Tags.ActivationOwnedTags);
            }

            OnEnded?.Invoke(this, wasCancelled);

            // 重置状态以便下次激活
            State = AbilityState.Inactive;
        }

        /// <summary>
        /// 取消技能
        /// </summary>
        internal void Cancel()
        {
            End(true);
        }

        // ============ 标签监听（OngoingBlockedTags）============

        /// <summary>
        /// 注册标签变化监听
        /// </summary>
        private void RegisterTagListener()
        {
            if (Owner?.OwnedTags == null) return;

            // 只有配置了 OngoingBlockedTags 才需要监听
            if (!Tags.OngoingBlockedTags.IsEmpty)
            {
                Owner.OwnedTags.OnTagAdded += OnOwnerTagAdded;
            }
        }

        /// <summary>
        /// 取消标签变化监听
        /// </summary>
        private void UnregisterTagListener()
        {
            if (Owner?.OwnedTags == null) return;

            if (!Tags.OngoingBlockedTags.IsEmpty)
            {
                Owner.OwnedTags.OnTagAdded -= OnOwnerTagAdded;
            }
        }

        /// <summary>
        /// 拥有者标签添加时的回调
        /// </summary>
        private void OnOwnerTagAdded(GameplayTag tag)
        {
            // 检查是否是 OngoingBlockedTags 中的标签
            if (Tags.OngoingBlockedTags.HasTag(tag))
            {
                // 获得了阻止标签，取消技能
                Cancel();
            }
        }
        
        public void OnGameplayEvent(GameplayEventType  gameplayEvent)
        {
            foreach (var AbilityEventPortData in AbilityNodeData.eventOutputPorts)
            {
                if (AbilityEventPortData.eventType==gameplayEvent)
                {
                    SpecExecutor.ExecuteConnectedNodes(SkillId, _abilityNodeGuid,  AbilityEventPortData.PortId, _context);
                }
            }
        }

        /// <summary>
        /// 每帧更新
        /// </summary>
        internal void Tick(float deltaTime)
        {
            if (State != AbilityState.Active)
                return;

            // 更新播放时间
            _currentPlayTime += deltaTime;

            // 检查时间效果触发
            CheckTimeEffectTriggers();

            // 检查时间Cue触发
            CheckTimeCueTriggers();

            UpdateRunningEffects(deltaTime);
        }

        /// <summary>
        /// 更新运行中的Effect
        /// </summary>
        private void UpdateRunningEffects(float deltaTime)
        {
            for (int i = 0; i < _runningEffects.Count; i++)
            {
                var effect = _runningEffects[i];
                if (effect == null)
                    continue;

                // 调用持续/周期效果的Tick
                if (effect.EffectNodeData?.durationType != EffectDurationType.Instant)
                {
                    effect.Tick(deltaTime);
                }

                if (!effect.IsRunning)
                {
                    _pendingRemoveEffects.Add(effect);
                }
            }

            if (_pendingRemoveEffects.Count > 0)
            {
                foreach (var effect in _pendingRemoveEffects)
                {
                    _runningEffects.Remove(effect);
                }
                _pendingRemoveEffects.Clear();
            }
        }

        // ============ Effect注册 ============

        /// <summary>
        /// 注册运行中的效果Spec（供SpecExecutor调用）
        /// </summary>
        internal void RegisterRunningEffect(GameplayEffectSpec effectSpec)
        {
            if (effectSpec != null && effectSpec.IsRunning && !_runningEffects.Contains(effectSpec))
            {
                _runningEffects.Add(effectSpec);
            }
        }

        // ============ 查询方法 ============

        /// <summary>
        /// 检查是否阻止其他技能激活
        /// </summary>
        public bool BlocksAbilityWithTags(GameplayTagSet abilityTags)
        {
            if (!IsActive || Tags.BlockAbilitiesWithTags.IsEmpty)
                return false;

            return abilityTags.HasAnyTags(Tags.BlockAbilitiesWithTags);
        }

        public override string ToString()
        {
            int skillId = AbilityNodeData?.skillId ?? 0;
            return $"[Skill:{skillId}] State={State}, Level={Level}";
        }

        // ============ 动画控制 ============

        /// <summary>
        /// 播放动画
        /// </summary>
        private void PlayAnimation(string name,bool loop)
        {
            if (Owner?.Owner == null || string.IsNullOrEmpty(name))
                return;

            var animator = Owner.Owner.GetComponent<AnimationComponent>();
            if (animator != null)
            {
                animator.PlayAnimation(name,loop);
            }
        }

        /// <summary>
        /// 停止动画
        /// </summary>
        private void StopAnimation()
        {
            // 动画停止逻辑（如果需要）
            // 通常动画会自然结束或被下一个动画覆盖
            PlayAnimation("Stand", true);
        }

        /// <summary>
        /// 重置所有时间效果状态
        /// </summary>
        private void ResetTimeEffects()
        {
            if (_timeEffects == null) return;

            foreach (var te in _timeEffects)
            {
                te.HasTriggered = false;
            }
        }

        /// <summary>
        /// 检查时间效果触发
        /// </summary>
        private void CheckTimeEffectTriggers()
        {
            if (_timeEffects == null || _context == null) return;

            foreach (var te in _timeEffects)
            {
                if (!te.HasTriggered && _currentPlayTime >= te.TriggerTime)
                {
                    te.HasTriggered = true;
                    // 通过端口ID执行连接的节点（时间效果端口在AnimationNode上）
                    SpecExecutor.ExecuteConnectedNodes(SkillId, _animationNodeGuid, te.PortId, _context);
                }
            }
        }

        // ============ 时间Cue控制 ============

        /// <summary>
        /// 重置所有时间Cue状态
        /// </summary>
        private void ResetTimeCues()
        {
            if (_timeCues == null) return;

            foreach (var tc in _timeCues)
            {
                tc.HasStarted = false;
                tc.HasEnded = false;
                tc.TriggeredCueSpecs.Clear();
            }
        }

        /// <summary>
        /// 检查时间Cue触发
        /// </summary>
        private void CheckTimeCueTriggers()
        {
            if (_timeCues == null || _context == null) return;

            foreach (var tc in _timeCues)
            {
                // 检查开始触发
                if (!tc.HasStarted && _currentPlayTime >= tc.StartTime)
                {
                    tc.HasStarted = true;
                    // 触发连接的Cue节点，并记录触发的CueSpec（时间Cue端口在AnimationNode上）
                    var triggeredSpecs = SpecExecutor.ExecuteConnectedCueNodes(SkillId, _animationNodeGuid, tc.PortId, _context);
                    foreach (var triggeredSpec in triggeredSpecs)
                    {
                        if (triggeredSpec != null&&triggeredSpec.DestroyWithNode)
                        {
                            tc.TriggeredCueSpecs.Add(triggeredSpec);
                        }
                    }
                }

                // 检查结束触发
                if (tc.HasStarted && !tc.HasEnded)
                {
                    float effectiveEndTime = tc.EndTime < 0 ? _animationDuration : tc.EndTime;
                    if (_currentPlayTime >= effectiveEndTime)
                    {
                        tc.HasEnded = true;
                        // 停止所有触发的Cue
                        StopTimeCueSpecs(tc);
                    }
                }
            }
        }

        /// <summary>
        /// 停止时间Cue触发的所有CueSpec
        /// </summary>
        private void StopTimeCueSpecs(TimeCueRuntime timeCue)
        {
            foreach (var cueSpec in timeCue.TriggeredCueSpecs)
            {
                cueSpec?.Stop();
            }
            timeCue.TriggeredCueSpecs.Clear();
        }

        /// <summary>
        /// 停止所有时间Cue
        /// </summary>
        private void StopAllTimeCues()
        {
            if (_timeCues == null) return;

            foreach (var tc in _timeCues)
            {
                if (tc.HasStarted && !tc.HasEnded)
                {
                    StopTimeCueSpecs(tc);
                    tc.HasEnded = true;
                }
            }
        }
    }
}
