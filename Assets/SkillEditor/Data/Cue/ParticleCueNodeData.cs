using System;
using UnityEngine;

namespace SkillEditor.Data
{
    /// <summary>
    /// 粒子特效Cue节点数据
    /// </summary>
    [Serializable]
    public class ParticleCueNodeData : CueNodeData
    {
        /// <summary>
        /// 位置来源
        /// </summary>
        public PositionSourceType positionSource = PositionSourceType.ParentInput;

        /// <summary>
        /// 粒子特效预制体
        /// </summary>
        public GameObject particlePrefab;
        public string particleBindingName = "";
        public Vector3 particleOffset = Vector3.zero;
        public Vector3 particleScale = Vector3.one;
        public bool attachToTarget = true;
        public bool particleLoop = false;


    }
}
