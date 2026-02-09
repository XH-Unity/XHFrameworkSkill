using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;

using SkillEditor.Data;
namespace SkillEditor.Editor
{
    public class SoundCueNodeInspector : CueNodeInspector
    {
        protected override void BuildCueInspectorUI(VisualElement container, SkillNodeBase node)
        {
            if (node is SoundCueNode soundCueNode)
            {
                var data = soundCueNode.TypedData;
                if (data == null) return;

                // 音效资源（拖拽AudioClip）
                var clipField = new ObjectField("音效资源") { objectType = typeof(AudioClip) };
                clipField.value = data.soundClip;
                clipField.RegisterValueChangedCallback(evt =>
                {
                    data.soundClip = evt.newValue as AudioClip;
                    soundCueNode.SyncUIFromData();
                    soundCueNode.NotifyConnectedTracksUpdateDuration();
                });
                container.Add(clipField);

                var volumeSlider = new Slider("音量", 0f, 1f) { value = data.soundVolume };
                volumeSlider.style.marginBottom = 4;
                volumeSlider.RegisterValueChangedCallback(evt =>
                {
                    data.soundVolume = evt.newValue;
                    soundCueNode.SyncUIFromData();
                });
                container.Add(volumeSlider);

                var loopToggle = new Toggle("循环播放") { value = data.soundLoop };
                loopToggle.RegisterValueChangedCallback(evt =>
                {
                    data.soundLoop = evt.newValue;
                    soundCueNode.SyncUIFromData();
                    soundCueNode.NotifyConnectedTracksUpdateDuration();
                });
                container.Add(loopToggle);
            }
        }
    }
}
