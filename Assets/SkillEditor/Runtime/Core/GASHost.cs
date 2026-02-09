using System.Collections.Generic;
using UnityEngine;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// GAS全局更新驱动器 - 单例模式
    /// 负责驱动所有ASC的Tick更新，避免每个ASC单独挂载MonoBehaviour
    /// </summary>
    public class GASHost : MonoBehaviour
    {
        private static GASHost _instance;
        public static GASHost Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[GASHost]");
                    _instance = go.AddComponent<GASHost>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        /// <summary>
        /// 注册的ASC列表
        /// </summary>
        private readonly List<AbilitySystemComponent> _registeredASCs = new List<AbilitySystemComponent>();

        /// <summary>
        /// 待移除的ASC列表（避免在遍历时修改集合）
        /// </summary>
        private readonly List<AbilitySystemComponent> _pendingRemove = new List<AbilitySystemComponent>();

        /// <summary>
        /// 待添加的ASC列表
        /// </summary>
        private readonly List<AbilitySystemComponent> _pendingAdd = new List<AbilitySystemComponent>();

        /// <summary>
        /// 是否正在更新
        /// </summary>
        private bool _isUpdating;

        /// <summary>
        /// 时间缩放（用于暂停等功能）
        /// </summary>
        public float TimeScale { get; set; } = 1f;

        /// <summary>
        /// 是否暂停
        /// </summary>
        public bool IsPaused { get; set; }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Update()
        {
            if (IsPaused) return;

            float deltaTime = Time.deltaTime * TimeScale;

            _isUpdating = true;

            // 更新所有注册的ASC
            for (int i = 0; i < _registeredASCs.Count; i++)
            {
                var asc = _registeredASCs[i];
                if (asc != null)
                {
                    asc.Tick(deltaTime);
                }
            }

            // 更新Cue管理器（处理特效/音效的生命周期）
            GameplayCueManager.Instance.Tick(deltaTime);

            _isUpdating = false;

            // 处理待添加的ASC
            if (_pendingAdd.Count > 0)
            {
                _registeredASCs.AddRange(_pendingAdd);
                _pendingAdd.Clear();
            }

            // 处理待移除的ASC
            if (_pendingRemove.Count > 0)
            {
                foreach (var asc in _pendingRemove)
                {
                    _registeredASCs.Remove(asc);
                }
                _pendingRemove.Clear();
            }
        }

        /// <summary>
        /// 注册ASC
        /// </summary>
        public void Register(AbilitySystemComponent asc)
        {
            if (asc == null) return;

            if (_isUpdating)
            {
                if (!_pendingAdd.Contains(asc) && !_registeredASCs.Contains(asc))
                {
                    _pendingAdd.Add(asc);
                }
            }
            else
            {
                if (!_registeredASCs.Contains(asc))
                {
                    _registeredASCs.Add(asc);
                }
            }
        }

        /// <summary>
        /// 注销ASC
        /// </summary>
        public void Unregister(AbilitySystemComponent asc)
        {
            if (asc == null) return;

            if (_isUpdating)
            {
                if (!_pendingRemove.Contains(asc))
                {
                    _pendingRemove.Add(asc);
                }
            }
            else
            {
                _registeredASCs.Remove(asc);
            }

            // 同时从待添加列表移除
            _pendingAdd.Remove(asc);
        }

        /// <summary>
        /// 获取注册的ASC数量
        /// </summary>
        public int RegisteredCount => _registeredASCs.Count;

        /// <summary>
        /// 获取所有注册的ASC（只读）
        /// </summary>
        public IReadOnlyList<AbilitySystemComponent> RegisteredASCs => _registeredASCs;

        /// <summary>
        /// 清空所有注册的ASC
        /// </summary>
        public void ClearAll()
        {
            _registeredASCs.Clear();
            _pendingAdd.Clear();
            _pendingRemove.Clear();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// 编辑器下手动触发更新（用于测试）
        /// </summary>
        public void EditorTick(float deltaTime)
        {
            if (IsPaused) return;

            _isUpdating = true;

            for (int i = 0; i < _registeredASCs.Count; i++)
            {
                var asc = _registeredASCs[i];
                if (asc != null)
                {
                    asc.Tick(deltaTime * TimeScale);
                }
            }

            // 更新Cue管理器（处理特效/音效的生命周期）
            GameplayCueManager.Instance.Tick(deltaTime * TimeScale);

            _isUpdating = false;

            if (_pendingAdd.Count > 0)
            {
                _registeredASCs.AddRange(_pendingAdd);
                _pendingAdd.Clear();
            }

            if (_pendingRemove.Count > 0)
            {
                foreach (var asc in _pendingRemove)
                {
                    _registeredASCs.Remove(asc);
                }
                _pendingRemove.Clear();
            }
        }
#endif
    }
}
