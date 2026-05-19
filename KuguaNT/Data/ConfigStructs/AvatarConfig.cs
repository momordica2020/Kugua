namespace Kugua.Data.ConfigStructs
{
    /// <summary>
    /// 个性项
    /// </summary>
    public class AvatarConfig
    {
        public string myQQ { get; set; }
        public string myName { get; set; }
        public string askName { get; set; }
        /// <summary>
        /// bot应答方式
        /// 0  全领域静默
        /// 1  仅供测试
        /// 2  仅供私聊和测试
        /// 3  全部开放
        /// </summary>
        public int answerState { get; set; }
        /// <summary>
        /// debug日志输出等级
        /// 0  不打印日志
        /// 1  仅正式日志
        /// 2  测试日志
        /// </summary>
        public int logState { get; set; }
        /// <summary>
        /// 测试人员qq
        /// </summary>
        public string adminQQ { get; set; }
        /// <summary>
        /// 测试群
        /// </summary>
        public string adminGroup { get; set; }
    }
  




}
