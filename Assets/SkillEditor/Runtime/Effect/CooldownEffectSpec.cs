using SkillEditor.Data;
using SkillEditor.Runtime.Utils;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// 冷却效果Spec
    /// 支持普通CD和充能CD两种模式
    /// </summary>
    public class CooldownEffectSpec : GameplayEffectSpec
    {
        private CooldownEffectNodeData CooldownNodeData => NodeData as CooldownEffectNodeData;

        // ============ 充能CD状态 ============

        /// <summary>
        /// 当前充能数
        /// </summary>
        public int CurrentCharges { get; private set; }

        /// <summary>
        /// 最大充能数
        /// </summary>
        public int MaxCharges { get; private set; }

        /// <summary>
        /// 每层充能时间
        /// </summary>
        public float ChargeTime { get; private set; }

        /// <summary>
        /// 当前充能计时器
        /// </summary>
        public float ChargeTimer { get; private set; }

        /// <summary>
        /// 是否是充能CD
        /// </summary>
        public bool IsChargeCooldown => CooldownNodeData?.cooldownType == CooldownType.Charge;

        /// <summary>
        /// 是否正在充能
        /// </summary>
        public bool IsCharging => IsChargeCooldown && CurrentCharges < MaxCharges;

        /// <summary>
        /// 充能进度 (0-1)
        /// </summary>
        public float ChargeProgress => ChargeTime > 0 ? 1f - (ChargeTimer / ChargeTime) : 1f;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            var nodeData = CooldownNodeData;
            if (nodeData != null && nodeData.cooldownType == CooldownType.Charge)
            {
                MaxCharges = nodeData.maxCharges;
                ChargeTime = FormulaEvaluator.EvaluateSimple(nodeData.chargeTime, 10f);
                CurrentCharges = MaxCharges;  // 初始满充能
                ChargeTimer = 0f;
            }
        }

        public override void Execute()
        {
            if (Context == null) return;

            var nodeData = CooldownNodeData;
            if (nodeData == null) return;

            if (nodeData.cooldownType == CooldownType.Normal)
            {
                // ========== 普通CD模式 ==========
                // 调用基类的 Execute，走标准的持续效果流程
                base.Execute();
            }
            else
            {
                // ========== 充能CD模式 ==========
                ExecuteChargeCooldown();
            }
        }

        /// <summary>
        /// 执行充能CD逻辑
        /// </summary>
        private void ExecuteChargeCooldown()
        {
            if (CurrentCharges > 0)
            {
                CurrentCharges--;

                // 如果之前是满的，开始充能计时
                if (ChargeTimer <= 0 && CurrentCharges < MaxCharges)
                {
                    ChargeTimer = ChargeTime;
                }

                // 更新 CD 标签状态
                UpdateChargeCooldownTag();

                // 确保 Effect 被注册到 Container 以便 Tick
                EnsureRegistered();
            }
        }

        /// <summary>
        /// 确保充能效果被注册到 EffectContainer
        /// </summary>
        private void EnsureRegistered()
        {
            var target = GetTarget();
            if (target == null) return;

            Target = target;
            IsRunning = true;

            // 检查是否已经注册
            var existingEffect = target.EffectContainer.FindEffectByNodeGuid(NodeGuid);
            if (existingEffect == null)
            {
                target.EffectContainer.AddEffect(this);
            }
        }

        public override void Tick(float deltaTime)
        {
            var nodeData = CooldownNodeData;
            if (nodeData == null) return;

            if (nodeData.cooldownType == CooldownType.Normal)
            {
                // 普通CD：调用基类 Tick（处理持续时间）
                base.Tick(deltaTime);
            }
            else
            {
                // 充能CD：处理充能恢复
                TickChargeCooldown(deltaTime);
            }
        }

        /// <summary>
        /// 充能CD的Tick逻辑
        /// </summary>
        private void TickChargeCooldown(float deltaTime)
        {
            // 未满时才计时
            if (CurrentCharges < MaxCharges && ChargeTimer > 0)
            {
                ChargeTimer -= deltaTime;

                if (ChargeTimer <= 0)
                {
                    // 恢复一层充能
                    CurrentCharges++;

                    // 还没满，继续计时
                    if (CurrentCharges < MaxCharges)
                    {
                        ChargeTimer = ChargeTime;
                    }
                    else
                    {
                        ChargeTimer = 0f;
                    }

                    // 更新 CD 标签状态
                    UpdateChargeCooldownTag();
                }
            }
        }

        /// <summary>
        /// 更新充能CD的标签状态
        /// </summary>
        private void UpdateChargeCooldownTag()
        {
            if (Target == null) return;

            if (CurrentCharges <= 0)
            {
                // 没有充能了，添加 CD 标签阻止技能释放
                if (!Tags.GrantedTags.IsEmpty)
                {
                    Target.OwnedTags.AddTags(Tags.GrantedTags);
                }
            }
            else
            {
                // 有充能，移除 CD 标签允许技能释放
                if (!Tags.GrantedTags.IsEmpty)
                {
                    Target.OwnedTags.RemoveTags(Tags.GrantedTags);
                }
            }
        }

        public override void Reset()
        {
            base.Reset();

            if (IsChargeCooldown)
            {
                CurrentCharges = MaxCharges;
                ChargeTimer = 0f;
            }
        }
    }
}
