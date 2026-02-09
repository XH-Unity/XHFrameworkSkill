using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillEditor.Data
{
    /// <summary>
    /// 属性容器 - 管理一个实体的所有属性
    /// 简化设计：使用 Dictionary<AttrType, Attribute> 直接管理属性
    /// </summary>
    [Serializable]
    public class AttributeSetContainer
    {
        /// <summary>
        /// 属性字典 - 运行时使用
        /// </summary>
        [NonSerialized]
        private Dictionary<AttrType, Attribute> attributes;

        // ============ 事件 ============

        /// <summary>
        /// 任意属性变化时触发
        /// </summary>
        public event Action<Attribute, float, float> OnAnyAttributeChanged;

        // ============ 属性 ============

        /// <summary>
        /// 属性数量
        /// </summary>
        public int Count => attributes?.Count ?? 0;

        /// <summary>
        /// 所有属性类型
        /// </summary>
        public IEnumerable<AttrType> AttrTypes
        {
            get
            {
                EnsureDictionary();
                return attributes.Keys;
            }
        }

        // ============ 方法 ============

        /// <summary>
        /// 确保字典已初始化
        /// </summary>
        private void EnsureDictionary()
        {
            if (attributes == null)
            {
                attributes = new Dictionary<AttrType, Attribute>();
            }
        }

        /// <summary>
        /// 添加属性
        /// </summary>
        public Attribute AddAttribute(AttrType attrType, float defaultValue = 0f)
        {
            EnsureDictionary();

            if (attributes.ContainsKey(attrType))
            {
                Debug.LogWarning($"Attribute '{attrType}' already exists");
                return attributes[attrType];
            }

            var attribute = new Attribute(attrType, defaultValue);
            attributes[attrType] = attribute;

            // 绑定事件
            BindAttributeEvents(attribute);

            return attribute;
        }

        /// <summary>
        /// 添加 Meta 属性（临时占位符，如伤害值）
        /// </summary>
        public Attribute AddMetaAttribute(AttrType attrType)
        {
            var attribute = AddAttribute(attrType, 0f);
            attribute.SetAsMeta(true);
            return attribute;
        }

        /// <summary>
        /// 获取属性
        /// </summary>
        public Attribute GetAttribute(AttrType attrType)
        {
            EnsureDictionary();
            return attributes.TryGetValue(attrType, out var attr) ? attr : null;
        }

        /// <summary>
        /// 索引器 - 通过属性类型访问属性
        /// </summary>
        public Attribute this[AttrType attrType]
        {
            get => GetAttribute(attrType);
        }

        /// <summary>
        /// 移除属性
        /// </summary>
        public bool RemoveAttribute(AttrType attrType)
        {
            EnsureDictionary();

            if (attributes.TryGetValue(attrType, out var attr))
            {
                attr.ClearCallbacks();
                attr.ClearModifiers();
                attributes.Remove(attrType);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检查是否包含属性
        /// </summary>
        public bool HasAttribute(AttrType attrType)
        {
            EnsureDictionary();
            return attributes.ContainsKey(attrType);
        }

        /// <summary>
        /// 获取属性的 BaseValue
        /// </summary>
        public float GetBaseValue(AttrType attrType)
        {
            return GetAttribute(attrType)?.BaseValue ?? 0f;
        }

        /// <summary>
        /// 获取属性的 CurrentValue
        /// </summary>
        public float GetCurrentValue(AttrType attrType)
        {
            return GetAttribute(attrType)?.CurrentValue ?? 0f;
        }

        /// <summary>
        /// 设置属性的 BaseValue
        /// </summary>
        public bool SetBaseValue(AttrType attrType, float value)
        {
            var attr = GetAttribute(attrType);
            if (attr != null)
            {
                attr.BaseValue = value;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 设置属性的 CurrentValue
        /// </summary>
        public bool SetCurrentValue(AttrType attrType, float value)
        {
            var attr = GetAttribute(attrType);
            if (attr != null)
            {
                attr.CurrentValue = value;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 初始化属性值（同时设置 BaseValue 和 CurrentValue）
        /// </summary>
        public bool InitializeAttribute(AttrType attrType, float value)
        {
            var attr = GetAttribute(attrType);
            if (attr != null)
            {
                attr.Initialize(value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 从配置数据初始化所有属性
        /// Excel格式: [[attrType, defaultValue], [attrType, defaultValue], ...]
        /// 例如: [[1,100],[2,100],[200,20],[201,10]]
        /// </summary>
        public void InitializeFromConfig(List<int[]> configData)
        {
            EnsureDictionary();

            foreach (var item in configData)
            {
                if (item.Length >= 2)
                {
                    var attrType = (AttrType)item[0];
                    var defaultValue = (float)item[1];
                    AddAttribute(attrType, defaultValue);
                }
            }
        }

        /// <summary>
        /// 从配置数据初始化所有属性（浮点数版本）
        /// </summary>
        public void InitializeFromConfig(List<float[]> configData)
        {
            EnsureDictionary();

            foreach (var item in configData)
            {
                if (item.Length >= 2)
                {
                    var attrType = (AttrType)(int)item[0];
                    var defaultValue = item[1];
                    AddAttribute(attrType, defaultValue);
                }
            }
        }

        /// <summary>
        /// 绑定属性事件
        /// </summary>
        private void BindAttributeEvents(Attribute attribute)
        {
            attribute.OnPostBaseValueChange += (attr, oldValue, newValue) =>
            {
                OnAnyAttributeChanged?.Invoke(attr, oldValue, newValue);
            };

            attribute.OnPostCurrentValueChange += (attr, oldValue, newValue) =>
            {
                OnAnyAttributeChanged?.Invoke(attr, oldValue, newValue);
            };
        }

        /// <summary>
        /// 创建所有属性的快照
        /// </summary>
        public Dictionary<AttrType, float> CreateSnapshot()
        {
            EnsureDictionary();
            var snapshot = new Dictionary<AttrType, float>();

            foreach (var kvp in attributes)
            {
                snapshot[kvp.Key] = kvp.Value.CurrentValue;
            }

            return snapshot;
        }

        /// <summary>
        /// 获取所有属性
        /// </summary>
        public IEnumerable<Attribute> GetAllAttributes()
        {
            EnsureDictionary();
            return attributes.Values;
        }

        /// <summary>
        /// 重新计算所有属性的 CurrentValue
        /// </summary>
        public void RecalculateAll(ModifierCalculationContext context = null)
        {
            EnsureDictionary();
            foreach (var attr in attributes.Values)
            {
                attr.Recalculate(context);
            }
        }

        /// <summary>
        /// 清除所有属性
        /// </summary>
        public void Clear()
        {
            if (attributes != null)
            {
                foreach (var attr in attributes.Values)
                {
                    attr.ClearCallbacks();
                    attr.ClearModifiers();
                }
                attributes.Clear();
            }
        }
    }
}
