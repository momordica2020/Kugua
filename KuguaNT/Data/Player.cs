using System.Numerics;
using System.Text.Json.Serialization;

namespace Kugua
{

    #region 群组


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

        [JsonIgnore]
        private long useTimes = 0;
        public long UseTimes
        {
            get { return useTimes; }
            set
            {
                useTimes = value;

                if (LastRespectTime.Minute == DateTime.Now.Minute)
                {
                    Last1MinRespectNum++;
                }
                else
                {
                    Last1MinRespectNum = 0;
                }

                LastRespectTime = DateTime.Now;

            }
        }

        [JsonIgnore]
        public DateTime LastRespectTime = DateTime.Now;

        [JsonIgnore]
        public int Last1MinRespectNum = 0;

        [JsonIgnore]
        public int delayMs
        {
            get
            {
                if (Last1MinRespectNum > 10)
                {
                    //match limit
                    return 1000 * (Last1MinRespectNum / 10);
                }
                else
                {
                    return 500;   
                }
            }
        }

        public HashSet<string> Tags { get; set; }

        /// <summary>
        /// 判断该群是否含特定标签
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool Is(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return false;
            return Tags?.Contains(tag.Trim()) ?? false;
        }
    }



   
 #endregion


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

        [JsonIgnore]
        private long useTimes = 0;
        public long UseTimes
        {
            get { return useTimes; }
            set
            {
                useTimes = value;

                if (LastRespectTime.Minute == DateTime.Now.Minute)
                {
                    Last1MinRespectNum++;
                }
                else
                {
                    Last1MinRespectNum = 0;
                }

                LastRespectTime = DateTime.Now;




            }
        }

        [JsonIgnore]
        public DateTime LastRespectTime = DateTime.Now;

        [JsonIgnore]
        public int Last1MinRespectNum = 0;

        [JsonIgnore]
        public int delayMs
        {
            get
            {
                if (Last1MinRespectNum > 10)
                {
                    //match limit
                    return 1000 * (Last1MinRespectNum/10);  
                }
                else
                {
                    return 500; 
                }
            }
        }

        public HashSet<string> Tags { get; set; }



        public BigInteger Money { get; set; }
        public DateTime LastSignTime { get; set; }
        public long SignTimes { get; set; }


        /// <summary>
        /// 该用户是否包含特定标签
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool Is(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return false;
            return Tags?.Contains(tag.Trim()) ?? false;
        }
    }


}
