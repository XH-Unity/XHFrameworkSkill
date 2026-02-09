using UnityEngine;
using SkillEditor.Data;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkillEditor.Editor
{
    /// <summary>
    /// Cue时长辅助工具 - 从Cue节点数据提取资源时长
    /// </summary>
    public static class CueDurationHelper
    {
        /// <summary>
        /// 统一入口：根据NodeData类型获取Cue时长（帧数）
        /// </summary>
        /// <returns>时长帧数，无法获取时返回-1</returns>
        public static int GetCueDurationFrames(NodeData nodeData)
        {
            if (nodeData == null) return -1;

            float durationSeconds = -1f;

            if (nodeData is ParticleCueNodeData particleData)
            {
                durationSeconds = GetParticleDuration(particleData);
            }
            else if (nodeData is SoundCueNodeData soundData)
            {
                durationSeconds = GetSoundDuration(soundData);
            }

            if (durationSeconds <= 0f) return -1;

            return SkillEditorConstants.SecondsToFrames(durationSeconds);
        }

        /// <summary>
        /// 从ParticleCueNodeData获取粒子特效时长（秒）
        /// 复用GameplayCueManager.GetParticleSystemDuration的逻辑
        /// </summary>
        public static float GetParticleDuration(ParticleCueNodeData data)
        {
            if (data == null || data.particlePrefab == null) return -1f;

            // 循环粒子没有固定时长
            if (data.particleLoop) return -1f;

#if UNITY_EDITOR
            // 在编辑器中，从prefab获取ParticleSystem组件
            var particleSystems = data.particlePrefab.GetComponentsInChildren<ParticleSystem>(true);
            if (particleSystems == null || particleSystems.Length == 0) return -1f;

            float maxDuration = 0f;
            foreach (var ps in particleSystems)
            {
                var main = ps.main;
                if (main.loop) continue;

                // delay + duration + startLifetime
                float startDelay = main.startDelay.constantMax;
                float duration = main.duration;
                float startLifetime = main.startLifetime.constantMax;
                float totalDuration = startDelay + duration + startLifetime;

                if (totalDuration > maxDuration)
                    maxDuration = totalDuration;
            }

            return maxDuration > 0f ? maxDuration : -1f;
#else
            return -1f;
#endif
        }

        /// <summary>
        /// 从SoundCueNodeData获取音效时长（秒）
        /// </summary>
        public static float GetSoundDuration(SoundCueNodeData data)
        {
            if (data == null || data.soundClip == null) return -1f;

            // 循环音效没有固定时长
            if (data.soundLoop) return -1f;

            return data.soundClip.length;
        }
    }
}
