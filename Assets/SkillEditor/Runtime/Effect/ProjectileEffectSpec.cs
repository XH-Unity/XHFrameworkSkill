using SkillEditor.Data;
using UnityEngine;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// 投射物效果Spec
    /// 负责生成投射物并管理其生命周期
    /// 注意：这是一个特殊的Effect，生命周期由投射物控制
    /// </summary>
    public class ProjectileEffectSpec : GameplayEffectSpec
    {
        private ProjectileEffectNodeData ProjectileNodeData => NodeData as ProjectileEffectNodeData;
        private ProjectileController _projectileController;
        private GameObject _projectileObject;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            // 强制设置为永久效果，生命周期由投射物控制
            Duration = -1f;
        }

        protected override void OnInitialHook(AbilitySystemComponent target)
        {
            var nodeData = ProjectileNodeData;
            if (nodeData == null) return;

            // 使用 PositionSourceType 获取发射位置
            Vector2 launchPosition = Context.GetPosition(nodeData.launchPositionSource, nodeData.launchBindingName);

            // 使用 PositionSourceType 获取目标位置
            Vector2 targetPosition = Context.GetPosition(nodeData.targetPositionSource, nodeData.targetBindingName);

            // 获取目标单位（仅单位模式需要）
            AbilitySystemComponent targetUnit = null;
            if (nodeData.projectileTargetType == ProjectileTargetType.Unit)
            {
                // 根据目标位置来源获取对应的 ASC
                targetUnit = GetTargetUnitFromPositionSource(nodeData.targetPositionSource);
            }

            // 计算发射方向
            Vector2 direction = (targetPosition - launchPosition).normalized;

            // 应用偏移角度（仅点模式）
            if (nodeData.projectileTargetType == ProjectileTargetType.Position && Mathf.Abs(nodeData.offsetAngle) > 0.01f)
            {
                direction = RotateVector2(direction, -nodeData.offsetAngle);
            }

            // 生成投射物
            SpawnProjectile(launchPosition, targetPosition, direction, targetUnit);

            // 将投射物对象设置到上下文中，供子节点使用
            Context.ProjectileObject = _projectileObject;
        }

        /// <summary>
        /// 根据 PositionSourceType 获取目标单位
        /// </summary>
        private AbilitySystemComponent GetTargetUnitFromPositionSource(PositionSourceType sourceType)
        {
            switch (sourceType)
            {
                case PositionSourceType.Caster:
                    return Context.Caster;
                case PositionSourceType.MainTarget:
                    return Context.MainTarget;
                case PositionSourceType.ParentInput:
                    return Context.ParentInputTarget;
                default:
                    return Context.MainTarget;
            }
        }

        /// <summary>
        /// 生成投射物
        /// </summary>
        private void SpawnProjectile(Vector2 launchPosition, Vector2 targetPosition, Vector2 direction, AbilitySystemComponent targetUnit)
        {
            var nodeData = ProjectileNodeData;

            // 创建投射物GameObject
            if (nodeData.projectilePrefab != null)
            {
                _projectileObject = Object.Instantiate(nodeData.projectilePrefab, launchPosition, Quaternion.identity);
            }
            else
            {
                // 没有预制体时创建一个简单的GameObject
                _projectileObject = new GameObject("Projectile");
                _projectileObject.transform.position = launchPosition;
            }

            // 添加或获取ProjectileController
            var controller = _projectileObject.GetComponent<ProjectileController>();
            if (controller == null)
            {
                controller = _projectileObject.AddComponent<ProjectileController>();
            }

            // 初始化投射物
            controller.Initialize(new ProjectileInitData
            {
                LaunchPosition = launchPosition,
                TargetPosition = targetPosition,
                Direction = direction,
                TargetUnit = targetUnit,
                TargetType = nodeData.projectileTargetType,
                FlyOver = nodeData.flyOver,
                CurveHeight = nodeData.curveHeight,
                Speed = nodeData.speed,
                MaxDistance = nodeData.maxDistance,
                CollisionRadius = nodeData.collisionRadius,
                IsPiercing = nodeData.isPiercing,
                MaxPierceCount = nodeData.maxPierceCount,
                CollisionTargetTags = nodeData.collisionTargetTags,
                CollisionExcludeTags = nodeData.collisionExcludeTags,
                TargetBindingName = nodeData.targetBindingName,
                SkillId = SkillId,
                NodeGuid = NodeGuid,
                Context = Context,
                SourceASC = Source
            });

            // 保存引用
            _projectileController = controller;

            // 注册事件
            controller.OnHit += OnProjectileHit;
            controller.OnReachTarget += OnProjectileReachTarget;
            controller.OnDestroy += OnProjectileDestroy;
        }

        /// <summary>
        /// 投射物命中回调
        /// </summary>
        private void OnProjectileHit(AbilitySystemComponent hitTarget, Vector2 hitPosition)
        {
            if (hitTarget == null) return;

            // 创建带有命中目标的上下文
            var hitContext = Context.CreateWithParentInput(hitTarget);
            hitContext.SetCustomData("HitPosition", hitPosition);
            // 确保投射物对象在上下文中
            hitContext.ProjectileObject = _projectileObject;

            // 执行碰撞时端口
            SpecExecutor.ExecuteConnectedNodes(SkillId, NodeGuid, "碰撞时", hitContext);
        }

        /// <summary>
        /// 投射物到达目标回调
        /// </summary>
        private void OnProjectileReachTarget(Vector2 position)
        {
            var ctx = GetExecutionContext();
            ctx.SetCustomData("ReachPosition", position);
            // 确保投射物对象在上下文中
            ctx.ProjectileObject = _projectileObject;

            // 执行到达目标位置端口
            SpecExecutor.ExecuteConnectedNodes(SkillId, NodeGuid, "到达目标位置", ctx);
        }

        /// <summary>
        /// 投射物销毁回调
        /// </summary>
        private void OnProjectileDestroy()
        {
            _projectileController = null;
            _projectileObject = null;

            // 投射物销毁时，结束Effect
            Remove();
        }

        /// <summary>
        /// 取消Effect时，也要销毁投射物
        /// </summary>
        public override void Cancel()
        {
            // 如果投射物还存在，销毁它
            if (_projectileController != null)
            {
                // 先取消事件订阅，避免循环调用
                _projectileController.OnHit -= OnProjectileHit;
                _projectileController.OnReachTarget -= OnProjectileReachTarget;
                _projectileController.OnDestroy -= OnProjectileDestroy;

                // 销毁投射物
                if (_projectileController.gameObject != null)
                {
                    Object.Destroy(_projectileController.gameObject);
                }
                _projectileController = null;
                _projectileObject = null;
            }

            base.Cancel();
        }

        /// <summary>
        /// 旋转2D向量
        /// </summary>
        private Vector2 RotateVector2(Vector2 v, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }
    }

    /// <summary>
    /// 投射物初始化数据
    /// </summary>
    public struct ProjectileInitData
    {
        public Vector2 LaunchPosition;
        public Vector2 TargetPosition;
        public Vector2 Direction;
        public AbilitySystemComponent TargetUnit;
        public ProjectileTargetType TargetType;
        public bool FlyOver;
        public float CurveHeight;
        public float Speed;
        public float MaxDistance;
        public float CollisionRadius;
        public bool IsPiercing;
        public int MaxPierceCount;
        public GameplayTagSet CollisionTargetTags;
        public GameplayTagSet CollisionExcludeTags;
        public string TargetBindingName;
        public string SkillId;
        public string NodeGuid;
        public SpecExecutionContext Context;
        public AbilitySystemComponent SourceASC;
    }
}
