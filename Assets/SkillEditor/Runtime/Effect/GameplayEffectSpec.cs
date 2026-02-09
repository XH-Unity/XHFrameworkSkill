using System;
using System.Collections.Generic;
using SkillEditor.Data;
using SkillEditor.Runtime.Utils;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// 效果Spec基类
    /// 瞬时效果: 不授予标签，直接修改BaseValue（永久）
    /// 持续效果: Apply时授予标签+添加Modifier（临时），Remove时移除
    /// </summary>
    public class GameplayEffectSpec
    {
        // ============ 基础标识 ============
        public string SpecId { get; private set; }
        public string SkillId { get; private set; }
        public string NodeGuid { get; private set; }
        public SpecExecutionContext Context { get; private set; }
        public AbilitySystemComponent Source { get; private set; }
        public AbilitySystemComponent Target { get; protected set; }
        public int Level { get; set; } = 1;
        public int StackCount { get; set; } = 1;
        public bool IsRunning { get; protected set; }
        protected bool IsCancelled { get; private set; }

        // ============ 运行时数据（可被修改） ============
        public EffectTagContainer Tags { get; protected set; }
        public float Duration { get; set; }
        public float Period { get; set; }
        public List<AttributeModifier> Modifiers { get; protected set; } = new List<AttributeModifier>();
        public Dictionary<string, float> SetByCallerValues { get; private set; } = new Dictionary<string, float>();
        public Dictionary<AttrType, float> SnapshotValues { get; private set; } = new Dictionary<AttrType, float>();

        // ============ 运行时状态 ============
        public float ActivationTime { get; private set; }
        public bool IsApplied { get; private set; }
        public bool IsExpired { get; private set; }
        public bool WasRefreshed { get; private set; }
        protected float ElapsedTime { get; set; }
        protected float PeriodTimer { get; set; }
        private List<GameplayCueSpec> _triggeredCueSpecs;

        // ============ 静态数据访问 ============
        protected NodeData NodeData => SkillDataCenter.Instance.GetNodeData(SkillId, NodeGuid);
        public EffectNodeData EffectNodeData => NodeData as EffectNodeData;

        public float RemainingTime => EffectNodeData?.durationType == EffectDurationType.Duration ? Math.Max(0f, Duration - ElapsedTime) : -1f;

        // ============ 初始化 ============
        public virtual void Initialize(string skillId, string nodeGuid, SpecExecutionContext context)
        {
            SpecId = Guid.NewGuid().ToString();
            SkillId = skillId;
            NodeGuid = nodeGuid;
            Context = context;
            Source = context?.Caster;
            Level = context?.AbilityLevel ?? 1;
            IsRunning = false;
            IsCancelled = false;
            IsApplied = false;
            IsExpired = false;
            WasRefreshed = false;
            _triggeredCueSpecs = new List<GameplayCueSpec>();

            var effectData = EffectNodeData;
            if (effectData != null)
                Tags = new EffectTagContainer(effectData);

            if (Source?.Attributes != null)
                SnapshotValues = Source.Attributes.CreateSnapshot();

            // 初始化可修改的运行时数据
            Duration = FormulaEvaluator.EvaluateSimple(effectData?.duration, 0f);
            Period = FormulaEvaluator.EvaluateSimple(effectData?.period, 1f);

            if (effectData?.attributeModifiers != null)
            {
                Modifiers.Clear();
                foreach (var modData in effectData.attributeModifiers)
                    Modifiers.Add(AttributeModifier.FromData(modData));
            }

            OnInitialize();
        }

        protected virtual void OnInitialize()
        {
        }

        // ============ 执行入口 ============
        public virtual void Execute()
        {
            if (Context == null) return;

            var target = GetTarget();
            if (target == null || !CanApplyTo(target)) return;

            IsRunning = true;
            var effectData = EffectNodeData;

            if (effectData?.durationType == EffectDurationType.Instant)
            {
                // === 瞬时效果 ===
                ExecuteInitialFlow(target);
                ExecuteCompleteFlow();
                IsRunning = false;
            }
            else
            {
                // === 持续效果 ===
                var existingEffect = target.EffectContainer.FindStackableEffect(this);

                if (existingEffect != null)
                {
                    // 堆叠处理
                    var existingData = existingEffect.EffectNodeData;
                    int stackLimit = existingData?.stackLimit ?? 0;
                    bool isAtStackLimit = stackLimit > 0 && existingEffect.StackCount >= stackLimit;
                    if (isAtStackLimit)
                    {
                        var overflowPolicy = existingData?.stackOverflowPolicy ?? StackOverflowPolicy.DenyApplication;
                        if (overflowPolicy == StackOverflowPolicy.AllowOverflowEffect)
                            SpecExecutor.ExecuteConnectedNodes(SkillId, NodeGuid, "溢出", GetExecutionContext());
                        if (overflowPolicy == StackOverflowPolicy.DenyApplication)
                        {
                            existingEffect.Refresh();
                            WasRefreshed = true;
                            return;
                        }
                    }
                    existingEffect.AddStack();
                    WasRefreshed = true;
                }
                else
                {
                    // 新效果
                    ExecuteInitialFlow(target);

                    if (effectData?.isPeriodic == true && effectData?.executeOnApplication == true)
                        ExecutePeriodicFlow();

                    if (effectData?.durationType == EffectDurationType.Duration && Duration <= 0)
                    {
                        ExecuteCompleteFlow();
                        IsRunning = false;
                    }
                    else
                    {
                        // 加入Container并应用
                        target.EffectContainer.AddEffect(this);
                        Target = target;
                        IsApplied = true;
                        ActivationTime = UnityEngine.Time.time;

                        if (!Tags.GrantedTags.IsEmpty)
                            Target.OwnedTags.AddTags(Tags.GrantedTags);
                        if (!Tags.RemoveGameplayEffectsWithTags.IsEmpty)
                            Target.RemoveActiveEffectsWithTags(Tags.RemoveGameplayEffectsWithTags);
                        RegisterTagListener();
                        // 持续效果：添加Modifier到属性
                        if (Target.Attributes != null && Modifiers?.Count > 0)
                        {
                            var modContext = CreateCalculationContext(Target);
                            foreach (var modifier in Modifiers)
                            {
                                var attribute = Target.Attributes.GetAttribute(modifier.TargetAttrType);
                                if (attribute != null)
                                {
                                    attribute.AddModifier(modifier, this);
                                    attribute.Recalculate(modContext);
                                }
                            }
                        }
                    }
                }
            }
        }

        // ============ 三大流程（被多处调用） ============

        /// <summary>
        /// 初始流程: 初始Cue → (瞬时:属性修改) → 初始效果 → 初始钩子
        /// 调用处: Execute(瞬时), Execute(持续-新效果)
        /// </summary>
        private void ExecuteInitialFlow(AbilitySystemComponent target)
        {
            var ctx = GetExecutionContext();

            // 瞬时效果：直接修改属性BaseValue
            if (EffectNodeData?.durationType == EffectDurationType.Instant && target?.Attributes != null && Modifiers?.Count > 0)
            {
                var calcContext = CreateCalculationContext(target);
                foreach (var modifier in Modifiers)
                {
                    var attribute = target.Attributes.GetAttribute(modifier.TargetAttrType);
                    if (attribute == null) continue;
                    float magnitude = modifier.CalculateMagnitude(calcContext);
                    switch (modifier.Operation)
                    {
                        case ModifierOperation.Add: attribute.BaseValue += magnitude; break;
                        case ModifierOperation.Multiply: attribute.BaseValue *= magnitude; break;
                        case ModifierOperation.Divide: if (Math.Abs(magnitude) > 0.0001f) attribute.BaseValue /= magnitude; break;
                        case ModifierOperation.Override: attribute.BaseValue = magnitude; break;
                    }
                }
            }

            SpecExecutor.ExecuteConnectedNodes(SkillId, NodeGuid, "初始效果", ctx);
            OnInitialHook(target);
        }

        /// <summary>
        /// 周期流程: 周期Cue → 每周期执行 → 周期钩子
        /// 调用处: Execute(周期立即执行), Tick(周期触发)
        /// </summary>
        private void ExecutePeriodicFlow()
        {
            var ctx = GetExecutionContext();
            SpecExecutor.ExecuteConnectedNodes(SkillId, NodeGuid, "每周期执行", ctx);
            OnPeriodicHook();
        }

        /// <summary>
        /// 完成流程: 完成Cue → 完成效果 → 完成钩子
        /// 调用处: Execute(瞬时), Execute(持续-Duration<=0), Remove()
        /// </summary>
        private void ExecuteCompleteFlow()
        {
            var ctx = GetExecutionContext();
            SpecExecutor.ExecuteConnectedNodes(SkillId, NodeGuid, "完成效果", ctx);
            OnCompleteHook();
        }

        // ============ 三个钩子（子类可重写） ============
        protected virtual void OnInitialHook(AbilitySystemComponent target) { }
        protected virtual void OnPeriodicHook() { }
        protected virtual void OnCompleteHook() { }

        /// <summary>
        /// 获取执行上下文（子类可重写，如Buff提供子上下文）
        /// 基类直接返回传入的上下文，不修改 StackCount
        /// 只有 BuffEffectSpec 等"触发源"才会创建新上下文并设置 StackCount
        /// </summary>
        protected virtual SpecExecutionContext GetExecutionContext()
        {
            return Context;
        }

        // ============ Tick ============
        public virtual void Tick(float deltaTime)
        {
            if (IsExpired || !IsApplied) return;

            ElapsedTime += deltaTime;
            var effectData = EffectNodeData;

            if (effectData?.isPeriodic == true && Period > 0)
            {
                PeriodTimer += deltaTime;
                if (PeriodTimer >= Period)
                {
                    PeriodTimer -= Period;
                    ExecutePeriodicFlow();
                }
            }

            if (effectData?.durationType == EffectDurationType.Duration && Duration > 0 && ElapsedTime >= Duration)
                Expire();
        }

        // ============ 刷新 ============
        public void Refresh()
        {
            var effectData = EffectNodeData;
            if (effectData?.stackDurationRefreshPolicy == StackDurationRefreshPolicy.RefreshOnSuccessfulApplication)
            {
                ElapsedTime = 0f;
                ActivationTime = UnityEngine.Time.time;
            }
            if (effectData?.stackPeriodResetPolicy == StackPeriodResetPolicy.ResetOnSuccessfulApplication)
                PeriodTimer = 0f;

            WasRefreshed = true;
            SpecExecutor.ExecuteConnectedNodes(SkillId, NodeGuid, "刷新时", GetExecutionContext());
        }

        // ============ 过期和移除 ============
        public void Expire()
        {
            if (IsExpired) return;

            var policy = EffectNodeData?.stackExpirationPolicy ?? StackExpirationPolicy.ClearEntireStack;
            switch (policy)
            {
                case StackExpirationPolicy.ClearEntireStack:
                    IsExpired = true;
                    Remove();
                    break;
                case StackExpirationPolicy.RemoveSingleStackAndRefreshDuration:
                    if (StackCount > 1)
                    {
                        StackCount--;
                        ActivationTime = UnityEngine.Time.time;
                        ElapsedTime = 0f;
                        RecalculateModifiers();
                    }
                    else
                    {
                        IsExpired = true;
                        Remove();
                    }
                    break;
                case StackExpirationPolicy.RefreshDuration:
                    if (StackCount > 0)
                    {
                        ActivationTime = UnityEngine.Time.time;
                        ElapsedTime = 0f;
                    }
                    else
                    {
                        IsExpired = true;
                        Remove();
                    }
                    break;
            }
        }

        public void Remove()
        {
            // 取消Cue
            if (_triggeredCueSpecs != null)
            {
                foreach (var cue in _triggeredCueSpecs)
                    if (cue != null && cue.IsRunning) cue.Cancel();
                _triggeredCueSpecs.Clear();
            }

            // 移除属性修改器
            if (Target?.Attributes != null)
            {
                foreach (var modifier in Modifiers)
                {
                    var attribute = Target.Attributes.GetAttribute(modifier.TargetAttrType);
                    if (attribute != null)
                    {
                        attribute.RemoveModifiersFromSource(this);
                        attribute.Recalculate();
                    }
                }
            }

            // 移除标签
            if (Target != null && !Tags.GrantedTags.IsEmpty)
                Target.OwnedTags.RemoveTags(Tags.GrantedTags);
            UnregisterTagListener();
            SpecExecutor.ExecuteConnectedNodes(SkillId, NodeGuid, "全部移除后", GetExecutionContext());
            ExecuteCompleteFlow();

            IsExpired = true;
            IsRunning = false;
        }

        public virtual void Cancel()
        {
            if (!IsRunning && !IsApplied) return;

            IsCancelled = true;
            IsRunning = false;

            if (_triggeredCueSpecs != null)
            {
                foreach (var cue in _triggeredCueSpecs)
                    if (cue != null && cue.IsRunning) cue.Cancel();
                _triggeredCueSpecs.Clear();
            }

            if (Target?.Attributes != null)
            {
                foreach (var modifier in Modifiers)
                {
                    var attribute = Target.Attributes.GetAttribute(modifier.TargetAttrType);
                    if (attribute != null)
                    {
                        attribute.RemoveModifiersFromSource(this);
                        attribute.Recalculate();
                    }
                }
            }

            if (Target != null && !Tags.GrantedTags.IsEmpty)
                Target.OwnedTags.RemoveTags(Tags.GrantedTags);

            SpecExecutor.ExecuteConnectedNodes(SkillId, NodeGuid, "全部移除后", GetExecutionContext());
        }

        // ============ 堆叠 ============
        public bool AddStack(int count = 1)
        {
            var effectData = EffectNodeData;
            int stackLimit = effectData?.stackLimit ?? 0;
            var overflowPolicy = effectData?.stackOverflowPolicy ?? StackOverflowPolicy.DenyApplication;

            if (stackLimit > 0 && StackCount >= stackLimit && overflowPolicy == StackOverflowPolicy.DenyApplication)
            {
                Refresh();
                return false;
            }

            int newStack = stackLimit > 0 ? Math.Min(StackCount + count, stackLimit) : StackCount + count;
            if (newStack == StackCount) return false;

            StackCount = newStack;
            Refresh();
            RecalculateModifiers();
            return true;
        }

        public bool RemoveStack(int count = 1)
        {
            int newStack = Math.Max(0, StackCount - count);
            if (newStack == StackCount) return false;

            StackCount = newStack;
            if (newStack == 0) Expire();
            else RecalculateModifiers();
            return true;
        }

        private void RecalculateModifiers()
        {
            if (Target?.Attributes == null) return;

            // 重新计算属性（属性计算时会从 Source 获取 StackCount）
            foreach (var modifier in Modifiers)
            {
                var attribute = Target.Attributes.GetAttribute(modifier.TargetAttrType);
                attribute?.MarkDirty();
                attribute?.Recalculate();
            }
        }
        
        /// <summary>
        /// 注册标签变化监听
        /// </summary>
        private void RegisterTagListener()
        {
            if (Target?.OwnedTags == null) return;

            // 只有配置了 OngoingBlockedTags 才需要监听
            if (!Tags.OngoingRequiredTags.IsEmpty)
            {
                Target.OwnedTags.OnTagAdded += OnOwnerTagAdded;
            }
        }

        /// <summary>
        /// 取消标签变化监听
        /// </summary>
        private void UnregisterTagListener()
        {
            if (Target?.OwnedTags == null) return;

            if (!Tags.OngoingRequiredTags.IsEmpty)
            {
                Target.OwnedTags.OnTagAdded -= OnOwnerTagAdded;
            }
        }
        /// <summary>
        /// 拥有者标签添加时的回调
        /// </summary>
        private void OnOwnerTagAdded(GameplayTag tag)
        {
            // 检查是否是 OngoingBlockedTags 中的标签
            if (Tags.OngoingRequiredTags.HasTag(tag))
            {
                // 获得了阻止标签，取消技能
                Remove();
            }
        }

        // ============ Cue管理 ============
        internal void RegisterTriggeredCue(GameplayCueSpec cueSpec)
        {
            if (cueSpec != null && !_triggeredCueSpecs.Contains(cueSpec))
                _triggeredCueSpecs.Add(cueSpec);
        }

        // ============ 辅助方法 ============
        protected AbilitySystemComponent GetTarget()
        {
            var nodeData = NodeData;
            return nodeData == null ? Context?.MainTarget : Context?.GetTarget(nodeData.targetType);
        }

        public bool CanApplyTo(AbilitySystemComponent target)
        {
            if (target == null) return false;
            if (!Tags.ApplicationRequiredTags.IsEmpty && !target.HasAllTags(Tags.ApplicationRequiredTags)) return false;
            if (!Tags.ApplicationImmunityTags.IsEmpty && target.HasAnyTags(Tags.ApplicationImmunityTags)) return false;
            return true;
        }

        public ModifierCalculationContext CreateCalculationContext(AbilitySystemComponent target)
        {
            return new ModifierCalculationContext
            {
                SourceAttributes = Source?.Attributes,
                TargetAttributes = target?.Attributes,
                SnapshotValues = SnapshotValues,
                EffectLevel = Level
            };
        }

        public void SetSetByCallerValue(string key, float value) => SetByCallerValues[key] = value;
        public float GetSetByCallerValue(string key, float defaultValue = 0f) =>
            SetByCallerValues.TryGetValue(key, out float value) ? value : defaultValue;

        public virtual void Reset()
        {
            IsRunning = false;
            IsCancelled = false;
            IsApplied = false;
            IsExpired = false;
            WasRefreshed = false;
            ElapsedTime = 0f;
            PeriodTimer = 0f;
            StackCount = 1;
            Modifiers?.Clear();
            SetByCallerValues?.Clear();
            SnapshotValues?.Clear();
            _triggeredCueSpecs?.Clear();
        }
    }
}
