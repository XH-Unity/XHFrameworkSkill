using SkillEditor.Data;
using UnityEngine;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// 放置物效果Spec
    /// 负责生成放置物并管理其生命周期
    /// 支持进入/离开/停留三种事件
    /// </summary>
    public class PlacementEffectSpec : GameplayEffectSpec
    {
        private PlacementEffectNodeData PlacementNodeData => NodeData as PlacementEffectNodeData;
        private PlacementController _placementController;
        private GameObject _placementObject;

        protected override void OnInitialHook(AbilitySystemComponent target)
        {
            var nodeData = PlacementNodeData;
            if (nodeData == null) return;

            // 使用 PositionSourceType 获取放置位置
            Vector3 position = Context.GetPosition(nodeData.positionSource, nodeData.positionBindingName);

            // 生成放置物
            SpawnPlacement(position);

            // 将放置物对象设置到上下文中，供子节点使用
            Context.PlacementObject = _placementObject;
        }

        /// <summary>
        /// 生成放置物
        /// </summary>
        private void SpawnPlacement(Vector3 position)
        {
            var nodeData = PlacementNodeData;

            // 创建放置物GameObject
            if (nodeData.placementPrefab != null)
            {
                _placementObject = Object.Instantiate(nodeData.placementPrefab, position, Quaternion.identity);
            }
            else
            {
                // 没有预制体时创建一个简单的GameObject
                _placementObject = new GameObject("Placement");
                _placementObject.transform.position = position;
            }

            // 如果启用碰撞，添加控制器
            if (nodeData.enableCollision)
            {
                _placementController = _placementObject.GetComponent<PlacementController>();
                if (_placementController == null)
                {
                    _placementController = _placementObject.AddComponent<PlacementController>();
                }

                // 初始化控制器
                _placementController.Initialize(new PlacementInitData
                {
                    CollisionRadius = nodeData.collisionRadius,
                    CollisionTargetTags = nodeData.collisionTargetTags,
                    CollisionExcludeTags = nodeData.collisionExcludeTags,
                    SourceASC = Source
                });

                // 注册事件
                _placementController.OnEnter += OnTargetEnter;
                _placementController.OnExit += OnTargetExit;
            }
        }

        /// <summary>
        /// 目标进入回调
        /// </summary>
        private void OnTargetEnter(AbilitySystemComponent target)
        {
            if (target == null) return;

            // 创建带有目标的上下文
            var ctx = Context.CreateWithParentInput(target);

            // 执行进入时端口
            SpecExecutor.ExecuteConnectedNodes(SkillId, NodeGuid, "进入时", ctx);
        }

        /// <summary>
        /// 目标离开回调
        /// </summary>
        private void OnTargetExit(AbilitySystemComponent target)
        {
            if (target == null) return;

            // 创建带有目标的上下文
            var ctx = Context.CreateWithParentInput(target);

            // 执行离开时端口
            SpecExecutor.ExecuteConnectedNodes(SkillId, NodeGuid, "离开时", ctx);
        }
   

        /// <summary>
        /// Effect取消时清理放置物
        /// </summary>
        public override void Cancel()
        {
            if (_placementController != null)
            {
                // 触发所有当前目标的离开事件
                _placementController.TriggerAllExitEvents();

                // 取消事件订阅
                _placementController.OnEnter -= OnTargetEnter;
                _placementController.OnExit -= OnTargetExit;
                _placementController = null;
            }

            if (_placementObject != null)
            {
                Object.Destroy(_placementObject);
                _placementObject = null;
            }
            base.Cancel();
        }
    }

    /// <summary>
    /// 放置物初始化数据
    /// </summary>
    public struct PlacementInitData
    {
        public float CollisionRadius;
        public GameplayTagSet CollisionTargetTags;
        public GameplayTagSet CollisionExcludeTags;
        public AbilitySystemComponent SourceASC;
    }
}
