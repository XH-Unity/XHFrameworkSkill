namespace SkillEditor.Data
{
    /// <summary>
    /// 技能系统全局常量（Data层，Runtime和Editor均可访问）
    /// </summary>
    public static class SkillConstants
    {
        /// <summary>
        /// 默认帧率
        /// </summary>
        public const int DEFAULT_FPS = 30;

        /// <summary>
        /// 帧转秒
        /// </summary>
        public static float FramesToSeconds(int frames) => (float)frames / DEFAULT_FPS;

        /// <summary>
        /// 秒转帧
        /// </summary>
        public static int SecondsToFrames(float seconds) => (int)(seconds * DEFAULT_FPS + 0.5f);
    }
}
