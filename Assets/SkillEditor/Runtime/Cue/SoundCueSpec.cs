using UnityEngine;
using SkillEditor.Data;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// 音效Cue Spec
    /// 播放音效
    /// </summary>
    public class SoundCueSpec : GameplayCueSpec
    {
        // ============ 动态数据 ============

        /// <summary>
        /// 音效资源
        /// </summary>
        public AudioClip SoundClip { get; set; }

        /// <summary>
        /// 音量
        /// </summary>
        public float SoundVolume { get; set; }

        /// <summary>
        /// 是否循环
        /// </summary>
        public bool SoundLoop { get; set; }



        // ============ 静态数据访问 ============

        private SoundCueNodeData SoundNodeData => NodeData as SoundCueNodeData;

        // ============ 初始化 ============

        protected override void OnInitialize()
        {
            var nodeData = SoundNodeData;
            if (nodeData != null)
            {
                SoundClip = nodeData.soundClip;
                SoundVolume = nodeData.soundVolume;
                SoundLoop = nodeData.soundLoop;
                DestroyWithNode = nodeData.destroyWithNode;
            }
        }

        // ============ 执行 ============

        protected override void PlayCue(AbilitySystemComponent target)
        {
            var nodeData = SoundNodeData;
            if (nodeData == null)
                return;

            var source = GetTarget();

            // 播放音效
            ActiveCue = GameplayCueManager.Instance.PlaySoundCue(nodeData, source, target);

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
            SoundClip = null;
            SoundVolume = 1f;
            SoundLoop = false;
            DestroyWithNode = false;
        }
    }
}
