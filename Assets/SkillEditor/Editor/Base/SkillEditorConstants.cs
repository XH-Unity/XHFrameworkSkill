using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 技能编辑器全局常量（委托到 SkillConstants）
    /// </summary>
    public static class SkillEditorConstants
    {
        public const int DEFAULT_FPS = SkillConstants.DEFAULT_FPS;

        public static int SecondsToFrames(float seconds) => SkillConstants.SecondsToFrames(seconds);

        public static float FramesToSeconds(int frames) => SkillConstants.FramesToSeconds(frames);
    }
}
