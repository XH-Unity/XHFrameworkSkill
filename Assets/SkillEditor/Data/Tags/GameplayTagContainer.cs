using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillEditor.Data
{
    /// <summary>
    /// 游戏标签容器 - 可变的标签集合（带引用计数）
    /// 用于运行时动态添加/移除标签
    /// </summary>
    [Serializable]
    public class GameplayTagContainer
    {
        [SerializeField] private List<GameplayTag> _tags = new List<GameplayTag>();

        /// <summary>
        /// 标签引用计数
        /// </summary>
        private Dictionary<int, int> _tagCounts = new Dictionary<int, int>();

        /// <summary>
        /// 标签变化事件（通用）
        /// </summary>
        public event Action OnTagsChanged;

        /// <summary>
        /// 标签首次添加事件（计数 0→1 时触发）
        /// </summary>
        public event Action<GameplayTag> OnTagAdded;

        /// <summary>
        /// 标签完全移除事件（计数 1→0 时触发）
        /// </summary>
        public event Action<GameplayTag> OnTagRemoved;

        /// <summary>
        /// 标签计数变化事件（可选，用于 UI 显示层数等）
        /// </summary>
        public event Action<GameplayTag, int, int> OnTagCountChanged;

        /// <summary>
        /// 标签列表（只读）
        /// </summary>
        public IReadOnlyList<GameplayTag> Tags => _tags;

        /// <summary>
        /// 标签数量
        /// </summary>
        public int Count => _tags.Count;

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => _tags.Count == 0;

        /// <summary>
        /// 获取标签的引用计数
        /// </summary>
        public int GetTagCount(GameplayTag tag)
        {
            if (!tag.IsValid) return 0;
            return _tagCounts.TryGetValue(tag.GetHashCode(), out int count) ? count : 0;
        }

        /// <summary>
        /// 添加标签（带引用计数）
        /// </summary>
        public void AddTag(GameplayTag tag)
        {
            if (!tag.IsValid) return;

            int hashCode = tag.GetHashCode();
            int oldCount = _tagCounts.TryGetValue(hashCode, out int c) ? c : 0;
            int newCount = oldCount + 1;
            _tagCounts[hashCode] = newCount;

            if (oldCount == 0)
            {
                // 首次添加，加入列表
                _tags.Add(tag);
                OnTagAdded?.Invoke(tag);
            }

            OnTagCountChanged?.Invoke(tag, oldCount, newCount);
            OnTagsChanged?.Invoke();
        }

        /// <summary>
        /// 添加多个标签
        /// </summary>
        public void AddTags(GameplayTagSet tagSet)
        {
            if (tagSet.IsEmpty) return;

            for (int i = 0; i < tagSet.Count; i++)
            {
                AddTag(tagSet[i]);
            }
        }

        /// <summary>
        /// 移除标签（带引用计数）
        /// </summary>
        public bool RemoveTag(GameplayTag tag)
        {
            if (!tag.IsValid) return false;

            int hashCode = tag.GetHashCode();
            if (!_tagCounts.TryGetValue(hashCode, out int oldCount) || oldCount <= 0)
                return false;

            int newCount = oldCount - 1;

            if (newCount <= 0)
            {
                // 完全移除
                _tagCounts.Remove(hashCode);
                for (int i = _tags.Count - 1; i >= 0; i--)
                {
                    if (_tags[i] == tag)
                    {
                        _tags.RemoveAt(i);
                        break;
                    }
                }
                OnTagRemoved?.Invoke(tag);
            }
            else
            {
                _tagCounts[hashCode] = newCount;
            }

            OnTagCountChanged?.Invoke(tag, oldCount, newCount);
            OnTagsChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 移除多个标签
        /// </summary>
        public void RemoveTags(GameplayTagSet tagSet)
        {
            if (tagSet.IsEmpty) return;

            for (int i = 0; i < tagSet.Count; i++)
            {
                RemoveTag(tagSet[i]);
            }
        }

        /// <summary>
        /// 清空所有标签
        /// </summary>
        public void Clear()
        {
            if (_tags.Count > 0)
            {
                // 触发所有标签的移除事件
                foreach (var tag in _tags)
                {
                    OnTagRemoved?.Invoke(tag);
                }
                _tags.Clear();
                _tagCounts.Clear();
                OnTagsChanged?.Invoke();
            }
        }

        /// <summary>
        /// 检查是否包含指定标签（支持层级匹配）
        /// </summary>
        public bool HasTag(GameplayTag tag)
        {
            if (!tag.IsValid || IsEmpty) return false;

            for (int i = 0; i < _tags.Count; i++)
            {
                if (_tags[i].HasTag(tag))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 检查是否包含指定标签（精确匹配）
        /// </summary>
        public bool HasTagExact(GameplayTag tag)
        {
            if (!tag.IsValid || IsEmpty) return false;

            for (int i = 0; i < _tags.Count; i++)
            {
                if (_tags[i] == tag)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 检查是否包含所有指定标签
        /// </summary>
        public bool HasAllTags(GameplayTagSet other)
        {
            if (other.IsEmpty) return true;
            if (IsEmpty) return false;

            for (int i = 0; i < other.Count; i++)
            {
                if (!HasTag(other[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 检查是否包含任意一个指定标签
        /// </summary>
        public bool HasAnyTags(GameplayTagSet other)
        {
            if (other.IsEmpty || IsEmpty) return false;

            for (int i = 0; i < other.Count; i++)
            {
                if (HasTag(other[i]))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 检查是否不包含任何指定标签
        /// </summary>
        public bool HasNoneTags(GameplayTagSet other)
        {
            return !HasAnyTags(other);
        }

        /// <summary>
        /// 转换为不可变的 GameplayTagSet
        /// </summary>
        public GameplayTagSet ToTagSet()
        {
            return new GameplayTagSet(_tags);
        }

        /// <summary>
        /// 从 GameplayTagSet 设置标签
        /// </summary>
        public void SetFromTagSet(GameplayTagSet tagSet)
        {
            Clear();
            if (!tagSet.IsEmpty)
            {
                foreach (var tag in tagSet.Tags)
                {
                    AddTag(tag);
                }
            }
        }

        public override string ToString()
        {
            if (IsEmpty) return "[]";
            return "[" + string.Join(", ", _tags.ConvertAll(t => t.ToString())) + "]";
        }
    }
}
