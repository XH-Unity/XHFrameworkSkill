using System.Collections.Generic;
using SkillEditor.Data;
using UnityEngine;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// Cue管理器 - 管理所有激活的Cue
    /// 负责Cue的播放、更新和清理
    /// </summary>
    public class GameplayCueManager
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        private static GameplayCueManager _instance;
        public static GameplayCueManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GameplayCueManager();
                return _instance;
            }
        }

        /// <summary>
        /// 所有激活的Cue
        /// </summary>
        private List<ActiveGameplayCue> _activeCues = new List<ActiveGameplayCue>();

        /// <summary>
        /// 待移除的Cue列表
        /// </summary>
        private List<ActiveGameplayCue> _pendingRemoval = new List<ActiveGameplayCue>();

        /// <summary>
        /// 资源加载器（项目需要实现）
        /// </summary>
        public ICueResourceLoader ResourceLoader { get; set; }

        // ============ 公共方法 ============

        /// <summary>
        /// 播放粒子Cue
        /// </summary>
        public ActiveGameplayCue PlayParticleCue(ParticleCueNodeData cueData, AbilitySystemComponent source, AbilitySystemComponent target)
        {
            if (cueData == null)
                return null;

            var activeCue = new ActiveGameplayCue();
            activeCue.IsLooping = cueData.particleLoop;

            // 加载并实例化粒子特效
            var particleObject = LoadAndInstantiateParticle(cueData, target);
            if (particleObject != null)
            {
                activeCue.AttachedObject = particleObject;

                // 获取所有粒子系统的最长时长
                if (!cueData.particleLoop)
                {
                    activeCue.Duration = GetParticleSystemDuration(particleObject);
                }
            }

            _activeCues.Add(activeCue);
            return activeCue;
        }

        /// <summary>
        /// 播放粒子Cue（使用位置和参数）
        /// </summary>
        public ActiveGameplayCue PlayParticleCue(
            GameObject prefab,
            Vector3 position,
            Vector3 scale,
            Transform attachTransform,
            bool isLoop)
        {
            if (prefab == null)
                return null;

            var activeCue = new ActiveGameplayCue();
            activeCue.IsLooping = isLoop;

            // 实例化粒子
            var instance = Object.Instantiate(prefab, position, Quaternion.identity, attachTransform);
            instance.transform.localScale = scale;

            activeCue.AttachedObject = instance;

            // 获取所有粒子系统的最长时长
            if (!isLoop)
            {
                activeCue.Duration = GetParticleSystemDuration(instance);
            }

            _activeCues.Add(activeCue);
            return activeCue;
        }

        /// <summary>
        /// 播放音效Cue
        /// </summary>
        public ActiveGameplayCue PlaySoundCue(SoundCueNodeData cueData, AbilitySystemComponent source, AbilitySystemComponent target)
        {
            if (cueData == null || cueData.soundClip == null)
                return null;

            var activeCue = new ActiveGameplayCue();
            activeCue.IsLooping = cueData.soundLoop;

            // 加载并播放音效
            var audioSource = PlaySound(cueData, target);
            if (audioSource != null)
            {
                activeCue.AttachedAudioSource = audioSource;

                // 获取音效时长
                if (audioSource.clip != null && !cueData.soundLoop)
                {
                    activeCue.Duration = audioSource.clip.length;
                }
            }

            _activeCues.Add(activeCue);
            return activeCue;
        }

        /// <summary>
        /// 播放飘字Cue
        /// </summary>
        public ActiveGameplayCue PlayFloatingTextCue(
            string text,
            Vector3 worldPosition,
            Color color,
            float fontSize,
            float duration,
            FloatingTextType textType)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            var activeCue = new ActiveGameplayCue();
            activeCue.IsLooping = false;
            activeCue.Duration = duration;

            // 通过 FloatingTextManager 创建飘字
            if (FloatingTextManager.Instance != null)
            {
                var floatingTextObject = FloatingTextManager.Instance.CreateFloatingText(
                    text, worldPosition, color, fontSize, duration, textType);

                if (floatingTextObject != null)
                {
                    activeCue.AttachedObject = floatingTextObject;
                }
            }
            else
            {
                Debug.LogWarning("[GameplayCueManager] FloatingTextManager 未初始化，请在场景中添加 FloatingTextManager 组件");
            }

            _activeCues.Add(activeCue);
            return activeCue;
        }

        /// <summary>
        /// 停止指定的Cue
        /// </summary>
        public void StopCue(ActiveGameplayCue activeCue)
        {
            if (activeCue == null)
                return;

            activeCue.Stop();
            _pendingRemoval.Add(activeCue);
        }

        /// <summary>
        /// 每帧更新
        /// </summary>
        public void Tick(float deltaTime)
        {
            // 更新所有激活的Cue
            foreach (var cue in _activeCues)
            {
                cue.Tick(deltaTime);

                if (cue.IsExpired)
                {
                    _pendingRemoval.Add(cue);
                }
            }

            // 移除过期的Cue
            if (_pendingRemoval.Count > 0)
            {
                foreach (var cue in _pendingRemoval)
                {
                    cue.Stop();
                    _activeCues.Remove(cue);
                }
                _pendingRemoval.Clear();
            }
        }

        /// <summary>
        /// 清理所有Cue
        /// </summary>
        public void Clear()
        {
            foreach (var cue in _activeCues)
            {
                cue.Stop();
            }
            _activeCues.Clear();
            _pendingRemoval.Clear();
        }

        // ============ 私有方法 ============

        /// <summary>
        /// 加载并实例化粒子特效
        /// </summary>
        private UnityEngine.GameObject LoadAndInstantiateParticle(ParticleCueNodeData cueData, AbilitySystemComponent target)
        {
            if (cueData.particlePrefab == null)
                return null;

            // 确定生成位置
            UnityEngine.Vector3 position = UnityEngine.Vector3.zero;
            UnityEngine.Quaternion rotation = UnityEngine.Quaternion.identity;
            UnityEngine.Transform parent = null;
            float facingDirection = 1f;

            if (target?.Owner != null)
            {
                var targetTransform = target.Owner.transform;

                // 获取目标朝向（2D角色通过localScale.x判断朝向）
                facingDirection = targetTransform.localScale.x >= 0 ? 1f : -1f;

                // 查找绑定点
                if (!string.IsNullOrEmpty(cueData.particleBindingName))
                {
                    var bindingPoint = targetTransform.Find(cueData.particleBindingName);
                    if (bindingPoint != null)
                    {
                        targetTransform = bindingPoint;
                    }
                }

                // 根据朝向调整偏移位置
                var adjustedOffset = cueData.particleOffset;
                adjustedOffset.x *= facingDirection;
                position = targetTransform.position + adjustedOffset;
                rotation = targetTransform.rotation;

                if (cueData.attachToTarget)
                {
                    parent = targetTransform;
                }
            }

            // 实例化粒子
            var instance = UnityEngine.Object.Instantiate(cueData.particlePrefab, position, rotation, parent);

            // 根据目标朝向调整特效缩放（翻转X轴）
            var scale = cueData.particleScale;
            scale.x *= facingDirection;
            instance.transform.localScale = scale;

            return instance;
        }

        /// <summary>
        /// 获取粒子系统的总时长（包括所有子节点）
        /// </summary>
        private float GetParticleSystemDuration(UnityEngine.GameObject particleObject)
        {
            // 获取所有粒子系统（包括子节点）
            var particleSystems = particleObject.GetComponentsInChildren<UnityEngine.ParticleSystem>();
            if (particleSystems == null || particleSystems.Length == 0)
                return 0f;

            float maxDuration = 0f;

            foreach (var ps in particleSystems)
            {
                var main = ps.main;

                // 如果是循环的粒子系统，跳过
                if (main.loop)
                    continue;

                // 计算这个粒子系统的总时长 = 延迟 + 发射时长 + 粒子最大生命周期
                float startDelay = main.startDelay.constantMax;
                float duration = main.duration;
                float startLifetime = main.startLifetime.constantMax;

                float totalDuration = startDelay + duration + startLifetime;

                if (totalDuration > maxDuration)
                {
                    maxDuration = totalDuration;
                }
            }

            return maxDuration;
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        private UnityEngine.AudioSource PlaySound(SoundCueNodeData cueData, AbilitySystemComponent target)
        {
            // 获取音效
            UnityEngine.AudioClip clip = cueData.soundClip;

            if (clip == null)
                return null;

            // 创建AudioSource
            UnityEngine.GameObject audioObject;
            if (target?.Owner != null)
            {
                audioObject = target.Owner;
            }
            else
            {
                audioObject = new UnityEngine.GameObject("CueSound");
            }

            var audioSource = audioObject.GetComponent<UnityEngine.AudioSource>();
            if (audioSource == null)
            {
                audioSource = audioObject.AddComponent<UnityEngine.AudioSource>();
            }

            // 配置并播放
            audioSource.clip = clip;
            audioSource.volume = cueData.soundVolume;
            audioSource.loop = cueData.soundLoop;
            audioSource.Play();

            return audioSource;
        }
    }

    /// <summary>
    /// Cue资源加载器接口
    /// 项目需要实现此接口以支持自定义资源加载
    /// </summary>
    public interface ICueResourceLoader
    {
        GameObject LoadParticle(string path);
    }
}
