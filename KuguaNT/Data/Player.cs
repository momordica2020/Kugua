using System.Numerics;

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

        public long UseTimes { get; set; }



        public HashSet<string> Tags { get; set; }

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


}
