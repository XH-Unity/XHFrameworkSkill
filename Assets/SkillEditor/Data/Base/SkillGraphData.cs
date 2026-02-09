using System.Collections.Generic;
using UnityEngine;

namespace SkillEditor.Data
{
    [CreateAssetMenu(fileName = "SkillGraph", menuName = "SkillEditor/SkillGraph")]
    public class SkillGraphData : ScriptableObject
    {
        [SerializeReference]
        public List<NodeData> nodes = new List<NodeData>();
        public List<ConnectionData> connections = new List<ConnectionData>();
    }
}
