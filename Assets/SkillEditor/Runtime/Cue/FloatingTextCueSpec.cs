using SkillEditor.Data;
using UnityEngine;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// 飘字Cue Spec
    /// 显示伤害、治疗、状态等飘字
    /// </summary>
    public class FloatingTextCueSpec : GameplayCueSpec
    {
        // ============ 动态数据 ============

        public PositionSourceType PositionSource { get; set; }
        public string PositionBindingName { get; set; }
        public FloatingTextType TextType { get; set; }
        public string FixedText { get; set; }
        public string ContextDataKey { get; set; }
        public Color TextColor { get; set; }
        public float FontSize { get; set; }
        public float Duration { get; set; }
        public Vector2 Offset { get; set; }
        public Vector2 MoveDirection { get; set; }

        // ============ 静态数据访问 ============

        private FloatingTextCueNodeData FloatingTextNodeData => NodeData as FloatingTextCueNodeData;

        // ============ 初始化 ============

        protected override void OnInitialize()
        {
            var nodeData = FloatingTextNodeData;
            if (nodeData != null)
            {
                PositionSource = nodeData.positionSource;
                PositionBindingName = nodeData.positionBindingName;
                TextType = nodeData.textType;
                FixedText = nodeData.fixedText;
                ContextDataKey = nodeData.contextDataKey;
                TextColor = nodeData.textColor;
                FontSize = nodeData.fontSize;
                Duration = nodeData.duration;
                Offset = nodeData.offset;
                MoveDirection = nodeData.moveDirection;
                DestroyWithNode = nodeData.destroyWithNode;
            }
        }

        // ============ 执行 ============

        protected override void PlayCue(AbilitySystemComponent target)
        {
            var nodeData = FloatingTextNodeData;
            if (nodeData == null)
                return;

            // 获取 DamageResult（如果有）
            Data.DamageResult? damageResult = null;
            if (!string.IsNullOrEmpty(ContextDataKey) && Context != null)
            {
                var value = Context.GetCustomData<object>(ContextDataKey, null);
                if (value is Data.DamageResult dr)
                {
                    damageResult = dr;
                }
            }

            // 获取显示文本
            string displayText = GetDisplayText(damageResult);
            if (string.IsNullOrEmpty(displayText))
                return;

            // 根据 DamageResult 决定颜色和字体大小
            Color finalColor = TextColor;
            float finalFontSize = FontSize;

            if (damageResult.HasValue)
            {
                if (damageResult.Value.IsMiss)
                {
                    finalColor = nodeData.missColor;
                }
                else if (damageResult.Value.IsCritical)
                {
                    finalColor = nodeData.criticalColor;
                    finalFontSize = nodeData.criticalFontSize;
                }
            }

            // 使用 PositionSourceType 获取显示位置
            Vector3 worldPosition = Context.GetPosition(PositionSource, PositionBindingName);
            worldPosition += new Vector3(Offset.x, Offset.y, 0);

            // 播放飘字
            ActiveCue = GameplayCueManager.Instance.PlayFloatingTextCue(
                displayText,
                worldPosition,
                finalColor,
                finalFontSize,
                Duration,
                TextType
            );

            if (ActiveCue != null)
            {
                IsRunning = true;
            }
        }

        /// <summary>
        /// 获取显示文本
        /// </summary>
        private string GetDisplayText(Data.DamageResult? damageResult)
        {
            string text = "";

            // 优先处理 DamageResult
            if (damageResult.HasValue)
            {
                var dr = damageResult.Value;
                if (dr.IsMiss)
                {
                    text = "Miss";
                }
                else
                {
                    text = "-" + UnityEngine.Mathf.RoundToInt(dr.Damage).ToString();
                    if (dr.IsCritical)
                    {
                        text = "暴击! " + text;
                    }
                }
            }
            // 其他类型的上下文数据
            else if (!string.IsNullOrEmpty(ContextDataKey) && Context != null)
            {
                var value = Context.GetCustomData<object>(ContextDataKey, null);
                if (value != null)
                {
                    string numText = "";
                    if (value is float floatValue)
                    {
                        numText = UnityEngine.Mathf.RoundToInt(floatValue).ToString();
                    }
                    else if (value is int intValue)
                    {
                        numText = intValue.ToString();
                    }
                    else
                    {
                        numText = value.ToString();
                    }

                    // 根据飘字类型添加前缀
                    switch (TextType)
                    {
                        case FloatingTextType.Damage:
                            text = "-" + numText;
                            break;
                        case FloatingTextType.Heal:
                        case FloatingTextType.Experience:
                        case FloatingTextType.Gold:
                            text = "+" + numText;
                            break;
                        default:
                            text = numText;
                            break;
                    }
                }
            }

            // 如果没有动态数据，使用固定文本
            if (string.IsNullOrEmpty(text))
            {
                text = FixedText;
            }

            return text;
        }

        protected override void StopCue()
        {
            if (ActiveCue != null)
            {
                GameplayCueManager.Instance.StopCue(ActiveCue);
                ActiveCue = null;
            }
        }

        public override void Reset()
        {
            base.Reset();
            PositionSource = PositionSourceType.ParentInput;
            PositionBindingName = "";
            TextType = FloatingTextType.Damage;
            FixedText = "";
            ContextDataKey = "Damage";
            TextColor = Color.white;
            FontSize = 32f;
            Duration = 1.5f;
            Offset = new Vector2(0, 1f);
            MoveDirection = new Vector2(0, 1f);
            DestroyWithNode = false;
        }
    }
}
