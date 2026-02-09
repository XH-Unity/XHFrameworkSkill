using System;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// 激活的Cue - 正在播放的Cue实例
    /// 管理Cue的生命周期
    /// </summary>
    public class ActiveGameplayCue
    {
        /// <summary>
        /// 是否已过期
        /// </summary>
        public bool IsExpired { get; private set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public float StartTime { get; private set; }

        /// <summary>
        /// 持续时间（0表示无限）
        /// </summary>
        public float Duration { get; set; }

        /// <summary>
        /// 已播放时间
        /// </summary>
        public float ElapsedTime { get; private set; }

        /// <summary>
        /// 是否循环
        /// </summary>
        public bool IsLooping { get; set; }

        /// <summary>
        /// 关联的GameObject（如粒子特效实例）
        /// </summary>
        public UnityEngine.GameObject AttachedObject { get; set; }

        /// <summary>
        /// 关联的AudioSource（如音效实例）
        /// </summary>
        public UnityEngine.AudioSource AttachedAudioSource { get; set; }

        /// <summary>
        /// 是否被刷新过
        /// </summary>
        public bool WasRefreshed { get; set; }

        /// <summary>
        /// 移除时的回调
        /// </summary>
        public event Action OnRemoved;

        // ============ 构造函数 ============

        public ActiveGameplayCue()
        {
            IsExpired = false;
            StartTime = UnityEngine.Time.time;
            ElapsedTime = 0f;
            Duration = 0f;
            IsLooping = false;
            WasRefreshed = false;
        }

        // ============ 生命周期 ============

        /// <summary>
        /// 每帧更新
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (IsExpired)
                return;

            ElapsedTime += deltaTime;

            // 检查是否到期
            if (Duration > 0 && ElapsedTime >= Duration)
            {
                if (!IsLooping)
                {
                    Expire();
                }
                else
                {
                    // 循环播放，重置时间
                    ElapsedTime = 0f;
                }
            }
        }

        /// <summary>
        /// 标记为过期
        /// </summary>
        public void Expire()
        {
            if (IsExpired)
                return;

            IsExpired = true;
        }

        /// <summary>
        /// 停止并清理
        /// </summary>
        public void Stop()
        {
            Expire();

            // 清理关联的GameObject
            if (AttachedObject != null)
            {
                UnityEngine.Object.Destroy(AttachedObject);
                AttachedObject = null;
            }

            // 停止关联的AudioSource
            if (AttachedAudioSource != null)
            {
                AttachedAudioSource.Stop();
                AttachedAudioSource = null;
            }

            // 触发移除回调
            OnRemoved?.Invoke();
        }

        /// <summary>
        /// 获取剩余时间
        /// </summary>
        public float GetRemainingTime()
        {
            if (Duration <= 0)
                return float.MaxValue;

            return UnityEngine.Mathf.Max(0f, Duration - ElapsedTime);
        }

        public override string ToString()
        {
            return $"[ActiveCue] Elapsed={ElapsedTime:F2}s, Expired={IsExpired}";
        }
    }
}
