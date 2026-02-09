using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

using SkillEditor.Data;
namespace SkillEditor.Editor
{
    /// <summary>
    /// 音效Cue节点
    /// </summary>
    public class SoundCueNode : CueNode<SoundCueNodeData>
    {
        private ObjectField clipField;
        private Slider volumeSlider;
        private Toggle loopToggle;

        public SoundCueNode(Vector2 position) : base(NodeType.SoundCue, position) { }

        protected override string GetNodeTitle() => "音效";
        protected override float GetNodeWidth() => 120;

        protected override void CreateContent()
        {
         
        }

   
    }
}
