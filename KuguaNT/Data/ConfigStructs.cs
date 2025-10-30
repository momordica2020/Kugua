namespace Kugua
{
    #region 配置文件相关结构体

    public class AppConfigs
    {
        public string Version { get; set; }
        public DateTime LateUpdated { get; set; }
        public string ResourcePath { get; set; }

        public AvatarConfigs Avatar { get; set; }

        public AIConfigs AI {  get; set; }
        public NetConfigs Net { get; set; }

        public Dictionary<string, Resource> Resources { get; set; }
    }

    public enum ResourceType
    {
        File,
        Path,
    }

    public class Resource
    {
        //public string Name { get; set; }
        public ResourceType Type { get; set; }
        public string Path { get; set; }
    }

    public class NetConfigs
    {
        public string QQWS { get; set; }
        public string QQHTTP {  get; set; }
        public string LocalWS { get; set; }


        public string TTSUri {  get; set; }
        public string OllamaUri { get; set; }
        public string OllamaUriG { get; set; }
        public string OllamaModel { get; set; }
    }


    /// <summary>
    /// 个性项
    /// </summary>
    public class AvatarConfigs
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

    public class AIConfigs
    {
        public string HSApiKey { get; set; }
        public string HSApiUrl { get; set; }
        public string HSModelNameVision { get; set; }
        public string HSModelNameChat { get; set; }

        public string HSModelNameImage {  get; set; }
    }
    #endregion




}
