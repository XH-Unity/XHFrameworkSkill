using System;
using UnityEngine;

namespace SkillEditor.Data
{
    /// <summary>
    /// 游戏标签 - 类似虚幻引擎的 Gameplay Tag
    /// 支持层级结构，如 "Ability.Attack.Melee"
    /// </summary>
    [Serializable]
    public struct GameplayTag : IEquatable<GameplayTag>
    {
        [SerializeField] private string _name;
        [SerializeField] private int _hashCode;
        [SerializeField] private string _shortName;
        [SerializeField] private int[] _ancestorHashCodes;
        [SerializeField] private string[] _ancestorNames;

        /// <summary>
        /// 完整标签名称，如 "Ability.Attack.Melee"
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// 标签哈希值，用于快速比较
        /// </summary>
        public int HashCode => _hashCode;

        /// <summary>
        /// 短名称（最后一级），如 "Melee"
        /// </summary>
        public string ShortName => _shortName;

        /// <summary>
        /// 所有祖先标签的哈希值
        /// </summary>
        public int[] AncestorHashCodes => _ancestorHashCodes;

        /// <summary>
        /// 所有祖先标签的名称
        /// </summary>
        public string[] AncestorNames => _ancestorNames;

        /// <summary>
        /// 标签是否有效
        /// </summary>
        public bool IsValid => !string.IsNullOrEmpty(_name);

        /// <summary>
        /// 标签是否为空（与 IsValid 相反）
        /// </summary>
        public bool IsEmpty => string.IsNullOrEmpty(_name);

        /// <summary>
        /// 标签深度（层级数）
        /// </summary>
        public int Depth => _ancestorNames?.Length ?? 0;

        /// <summary>
        /// 空标签
        /// </summary>
        public static GameplayTag None => new GameplayTag();

        /// <summary>
        /// 创建一个游戏标签
        /// </summary>
        /// <param name="name">完整标签名称，如 "Ability.Attack.Melee"</param>
        public GameplayTag(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                _name = null;
                _hashCode = 0;
                _shortName = null;
                _ancestorHashCodes = Array.Empty<int>();
                _ancestorNames = Array.Empty<string>();
                return;
            }

            _name = name;
            _hashCode = name.GetHashCode();

            // 解析层级结构
            var parts = name.Split('.');
            _shortName = parts[parts.Length - 1];

            // 构建祖先列表（不包含自身）
            if (parts.Length > 1)
            {
                _ancestorHashCodes = new int[parts.Length - 1];
                _ancestorNames = new string[parts.Length - 1];

                string ancestorPath = "";
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    if (i > 0) ancestorPath += ".";
                    ancestorPath += parts[i];
                    _ancestorNames[i] = ancestorPath;
                    _ancestorHashCodes[i] = ancestorPath.GetHashCode();
                }
            }
            else
            {
                _ancestorHashCodes = Array.Empty<int>();
                _ancestorNames = Array.Empty<string>();
            }
        }

        /// <summary>
        /// 检查是否拥有指定标签（支持层级匹配）
        /// 例如：标签 "Ability.Attack.Melee" 拥有 "Ability"、"Ability.Attack"、"Ability.Attack.Melee"
        /// </summary>
        public bool HasTag(GameplayTag tag)
        {
            if (!tag.IsValid) return false;
            if (!IsValid) return false;

            // 完全匹配
            if (_hashCode == tag._hashCode) return true;

            // 检查是否是祖先
            if (_ancestorHashCodes != null)
            {
                for (int i = 0; i < _ancestorHashCodes.Length; i++)
                {
                    if (_ancestorHashCodes[i] == tag._hashCode)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 检查是否是另一个标签的后代
        /// </summary>
        public bool IsDescendantOf(GameplayTag other)
        {
            if (!other.IsValid || !IsValid) return false;
            return HasTag(other) && _hashCode != other._hashCode;
        }

        /// <summary>
        /// 检查是否是另一个标签的祖先
        /// </summary>
        public bool IsAncestorOf(GameplayTag other)
        {
            return other.IsDescendantOf(this);
        }

        /// <summary>
        /// 获取父标签
        /// </summary>
        public GameplayTag GetParent()
        {
            if (_ancestorNames == null || _ancestorNames.Length == 0)
                return None;
            return new GameplayTag(_ancestorNames[_ancestorNames.Length - 1]);
        }

        #region 相等性比较

        public bool Equals(GameplayTag other)
        {
            return _hashCode == other._hashCode;
        }

        public override bool Equals(object obj)
        {
            return obj is GameplayTag other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public static bool operator ==(GameplayTag left, GameplayTag right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GameplayTag left, GameplayTag right)
        {
            return !left.Equals(right);
        }

        #endregion

        public override string ToString()
        {
            return _name ?? "None";
        }

        /// <summary>
        /// 隐式转换：字符串 -> GameplayTag
        /// </summary>
        public static implicit operator GameplayTag(string name)
        {
            return new GameplayTag(name);
        }

        /// <summary>
        /// 隐式转换：GameplayTag -> 字符串
        /// </summary>
        public static implicit operator string(GameplayTag tag)
        {
            return tag._name;
        }
    }
}
