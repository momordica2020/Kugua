using System.Numerics;

namespace Kugua
{
    #region 用户和 群组



    public enum PlayerType
    {
        Normal,
        Blacklist,
        Admin,
    }
    public class Player
    {
        //public long Id { get; set; }
        public string Name { get; set; }
        public string Mark { get; set; }
        public PlayerType Type { get; set; }
        public long UseTimes { get; set; }



        public HashSet<string> Tags { get; set; }



        public BigInteger Money { get; set; }
        public DateTime LastSignTime { get; set; }
        public long SignTimes { get; set; }

        public bool Is(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return false;
            return Tags?.Contains(tag.Trim()) ?? false;
        }
    }

    public enum PlaygroupType
    {
        Normal,
        Blacklist,
        Test,
    }

    public class Playgroup
    {
        //public long id { get; set; }
        public string Name { get; set; }
        public PlaygroupType Type { get; set; }

        public long UseTimes { get; set; }



        public HashSet<string> Tags { get; set; }

        public bool Is(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return false;
            return Tags?.Contains(tag.Trim()) ?? false;
        }
    }



    #endregion

    #region 配置文件相关结构体

    public class AppConfigs
    {
        public string Version { get; set; }
        public DateTime LateUpdated { get; set; }
        public string ResourcePath { get; set; }

        public AvatarConfigs Avatar { get; set; }
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
        /// 测试人员qq
        /// </summary>
        public string adminQQ { get; set; }
        /// <summary>
        /// 测试群
        /// </summary>
        public string adminGroup { get; set; }
    }


    #endregion
}
