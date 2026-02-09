using SkillEditor.Data;
using UnityEngine;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// 粒子特效Cue Spec
    /// 播放粒子特效
    /// </summary>
    public class ParticleCueSpec : GameplayCueSpec
    {
        // ============ 动态数据 ============

        /// <summary>
        /// 粒子预制体
        /// </summary>
        public GameObject ParticlePrefab { get; set; }

        /// <summary>
        /// 位置来源类型
        /// </summary>
        public PositionSourceType PositionSource { get; set; }

        /// <summary>
        /// 绑定点名称
        /// </summary>
        public string ParticleBindingName { get; set; }

        /// <summary>
        /// 位置偏移
        /// </summary>
        public Vector3 ParticleOffset { get; set; }

        /// <summary>
        /// 缩放
        /// </summary>
        public Vector3 ParticleScale { get; set; }

        /// <summary>
        /// 是否附着到目标
        /// </summary>
        public bool AttachToTarget { get; set; }

        /// <summary>
        /// 是否循环
        /// </summary>
        public bool ParticleLoop { get; set; }



        // ============ 静态数据访问 ============

        private ParticleCueNodeData ParticleNodeData => NodeData as ParticleCueNodeData;

        // ============ 初始化 ============

        protected override void OnInitialize()
        {
            var nodeData = ParticleNodeData;
            if (nodeData != null)
            {
                ParticlePrefab = nodeData.particlePrefab;
                PositionSource = nodeData.positionSource;
                ParticleBindingName = nodeData.particleBindingName;
                ParticleOffset = nodeData.particleOffset;
                ParticleScale = nodeData.particleScale;
                AttachToTarget = nodeData.attachToTarget;
                ParticleLoop = nodeData.particleLoop;
                DestroyWithNode = nodeData.destroyWithNode;
            }
        }

        // ============ 执行 ============

        protected override void PlayCue(AbilitySystemComponent target)
        {
            var nodeData = ParticleNodeData;
            if (nodeData == null)
                return;

            // 获取位置来源对象，用于确定朝向和挂点
            var sourceObject = Context.GetSourceObject(PositionSource);

            // 获取朝向（2D角色通过localScale.x判断：>=0朝左，<0朝右）
            float facingDirection = 1f;
            if (sourceObject != null)
            {
                facingDirection = sourceObject.transform.localScale.x >= 0 ? 1f : -1f;
            }

            // 使用 PositionSourceType 获取播放位置，根据朝向翻转X偏移
            Vector3 position = Context.GetPosition(PositionSource, ParticleBindingName);
            Vector3 adjustedOffset = ParticleOffset;
            adjustedOffset.x *= facingDirection;
            position += adjustedOffset;

            // 根据朝向翻转X缩放
            Vector3 adjustedScale = ParticleScale;
            adjustedScale.x *= facingDirection;

            // 获取附着的 Transform（如果需要附着）
            Transform attachTransform = null;
            if (AttachToTarget)
            {
                if (sourceObject != null)
                {
                    // 如果有挂点，附着到挂点
                    if (!string.IsNullOrEmpty(ParticleBindingName))
                    {
                        var bindingPoint = sourceObject.transform.Find(ParticleBindingName);
                        attachTransform = bindingPoint ?? sourceObject.transform;
                    }
                    else
                    {
                        attachTransform = sourceObject.transform;
                    }
                }
            }

            // 播放粒子特效
            ActiveCue = GameplayCueManager.Instance.PlayParticleCue(
                nodeData.particlePrefab,
                position,
                adjustedScale,
                attachTransform,
                ParticleLoop
            );

            if (ActiveCue != null)
            {
                IsRunning = true;
            }
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
            ParticlePrefab = null;
            PositionSource = PositionSourceType.ParentInput;
            ParticleBindingName = "";
            ParticleOffset = Vector3.zero;
            ParticleScale = Vector3.one;
            AttachToTarget = true;
            ParticleLoop = false;
            DestroyWithNode = false;
        }
    }
}
